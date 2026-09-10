namespace BankManagementSystem.Utils;

using System.Drawing.Printing;
using System.Text;
using BankManagementSystem.Data;
using BankManagementSystem.Services;

/// <summary>表格导出 CSV / Excel 表格、复制摘要、打印预览、打开目录。</summary>
public static class DataExportHelper
{
    private static readonly LogService Log = new();

    /// <summary>程序运行目录（exe / dll 所在文件夹）。</summary>
    public static string AppRunFolder =>
        AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    /// <summary>当前导出目录：优先使用账号页已保存设置，否则为程序运行目录下的 Exports。</summary>
    public static string ExportFolder
    {
        get
        {
            var custom = DatabaseHelper.GetSetting("ExportFolder");
            if (!string.IsNullOrWhiteSpace(custom))
            {
                try
                {
                    Directory.CreateDirectory(custom);
                    return custom;
                }
                catch { /* fall through to default */ }
            }
            return DefaultExportFolder;
        }
    }

    /// <summary>默认导出目录：当前程序运行文件夹下的 Exports。</summary>
    public static string DefaultExportFolder
    {
        get
        {
            var dir = Path.Combine(AppRunFolder, "Exports");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static void SetExportFolder(string path)
    {
        Directory.CreateDirectory(path);
        DatabaseHelper.SetSetting("ExportFolder", path);
    }

    public static Button CreateToolButton(string text, EventHandler onClick, int minWidth = 100)
    {
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(minWidth, 34),
            Margin = new Padding(0, 6, 8, 0),
            Padding = new Padding(12, 2, 12, 2)
        };
        UiTheme.StylePrimaryButton(b);
        b.AutoSize = true;
        b.MinimumSize = new Size(minWidth, 34);
        b.Padding = new Padding(12, 2, 12, 2);
        b.Click += onClick;
        return b;
    }

    /// <summary>向工具栏追加：导出CSV、导出为表格、复制摘要、打印预览、打开目录。</summary>
    public static void AppendExportButtons(
        Control.ControlCollection host,
        Func<DataGridView?> getGrid,
        string title,
        Func<string>? getSummary = null,
        string? openFolderPath = null)
    {
        host.Add(CreateToolButton("导出CSV", (_, _) =>
        {
            var g = getGrid();
            if (g == null || g.Columns.Count == 0) { MessageBox.Show("当前没有可导出的表格数据。"); return; }
            ExportToCsv(g, title);
        }, 100));

        host.Add(CreateToolButton("导出为表格", (_, _) =>
        {
            var g = getGrid();
            if (g == null || g.Columns.Count == 0) { MessageBox.Show("当前没有可导出的表格数据。"); return; }
            ExportToExcelTable(g, title);
        }, 110));

        host.Add(CreateToolButton("复制摘要", (_, _) =>
        {
            var g = getGrid();
            var summary = getSummary?.Invoke();
            if (string.IsNullOrWhiteSpace(summary) && g != null)
                summary = BuildGridSummary(g, title);
            if (string.IsNullOrWhiteSpace(summary))
            {
                MessageBox.Show("没有可复制的摘要内容。");
                return;
            }
            CopyText(summary!, title);
        }, 100));

        host.Add(CreateToolButton("打印预览", (_, _) =>
        {
            var g = getGrid();
            if (g == null || g.Rows.Count == 0) { MessageBox.Show("当前没有可打印的表格数据。"); return; }
            ShowPrintPreview(g, title);
        }, 100));

        host.Add(CreateToolButton("打开目录", (_, _) =>
        {
            OpenDirectory(openFolderPath ?? ExportFolder);
        }, 100));
    }

    public static DataGridView? FindFirstGrid(Control? root)
    {
        if (root == null) return null;
        if (root is DataGridView dgv) return dgv;
        foreach (Control c in root.Controls)
        {
            var found = FindFirstGrid(c);
            if (found != null) return found;
        }
        return null;
    }

    public static IEnumerable<DataGridView> FindAllGrids(Control? root)
    {
        if (root == null) yield break;
        if (root is DataGridView dgv)
        {
            yield return dgv;
            yield break;
        }
        foreach (Control c in root.Controls)
        {
            foreach (var g in FindAllGrids(c))
                yield return g;
        }
    }

    public static string? ExportToCsv(DataGridView grid, string title)
    {
        try
        {
            var path = Path.Combine(ExportFolder, Sanitize(title) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
            var sb = new StringBuilder();
            var cols = VisibleColumns(grid).ToList();
            sb.AppendLine(string.Join(",", cols.Select(c => CsvEscape(c.HeaderText))));
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                sb.AppendLine(string.Join(",", cols.Select(c => CsvEscape(row.Cells[c.Index].Value?.ToString() ?? ""))));
            }
            // UTF-8 BOM，便于 Excel 正确识别中文
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            Log.Write("导出CSV", title, path);
            if (MessageBox.Show($"已导出 CSV：\n{path}\n\n是否打开所在目录？", "导出成功",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                OpenDirectory(ExportFolder);
            return path;
        }
        catch (Exception ex)
        {
            MessageBox.Show("导出 CSV 失败：" + ex.Message);
            return null;
        }
    }

    /// <summary>导出为 Excel 可打开的 HTML 表格（.xls）。</summary>
    public static string? ExportToExcelTable(DataGridView grid, string title)
    {
        try
        {
            var path = Path.Combine(ExportFolder, Sanitize(title) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls");
            var cols = VisibleColumns(grid).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
            sb.AppendLine("<head><meta charset=\"utf-8\"/>");
            sb.AppendLine("<!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>");
            sb.AppendLine("<x:Name>Sheet1</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions>");
            sb.AppendLine("</x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]-->");
            sb.AppendLine("<style>td,th{border:1px solid #999;padding:4px;font-family:Microsoft YaHei UI;font-size:11pt;}</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine($"<h3>{HtmlEscape(title)}</h3>");
            sb.AppendLine($"<p>导出时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine("<table>");
            sb.Append("<tr>");
            foreach (var c in cols) sb.Append($"<th>{HtmlEscape(c.HeaderText)}</th>");
            sb.AppendLine("</tr>");
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                sb.Append("<tr>");
                foreach (var c in cols)
                    sb.Append($"<td>{HtmlEscape(row.Cells[c.Index].Value?.ToString() ?? "")}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table></body></html>");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
            Log.Write("导出表格", title, path);
            if (MessageBox.Show($"已导出表格文件（可用 Excel 打开）：\n{path}\n\n是否打开文件？", "导出成功",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            return path;
        }
        catch (Exception ex)
        {
            MessageBox.Show("导出表格失败：" + ex.Message);
            return null;
        }
    }

    public static void ExportKeyValuesToCsv(IEnumerable<(string Key, string Value)> rows, string title)
    {
        var grid = new DataGridView();
        grid.Columns.Add("项目", "项目");
        grid.Columns.Add("内容", "内容");
        foreach (var (k, v) in rows)
            grid.Rows.Add(k, v);
        ExportToCsv(grid, title);
        grid.Dispose();
    }

    public static void ExportKeyValuesToTable(IEnumerable<(string Key, string Value)> rows, string title)
    {
        var grid = new DataGridView();
        grid.Columns.Add("项目", "项目");
        grid.Columns.Add("内容", "内容");
        foreach (var (k, v) in rows)
            grid.Rows.Add(k, v);
        ExportToExcelTable(grid, title);
        grid.Dispose();
    }

    public static string BuildGridSummary(DataGridView grid, string title)
    {
        var cols = VisibleColumns(grid).ToList();
        var rowCount = grid.Rows.Cast<DataGridViewRow>().Count(r => !r.IsNewRow);
        var sb = new StringBuilder();
        sb.AppendLine($"【{title}】");
        sb.AppendLine($"导出/摘要时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"行数：{rowCount}　列数：{cols.Count}");
        sb.AppendLine("列：" + string.Join("、", cols.Select(c => c.HeaderText)));
        if (grid.CurrentRow != null && !grid.CurrentRow.IsNewRow)
        {
            sb.AppendLine("当前选中行：");
            foreach (var c in cols)
                sb.AppendLine($"  {c.HeaderText} = {grid.CurrentRow.Cells[c.Index].Value}");
        }
        else if (rowCount > 0)
        {
            sb.AppendLine("首行预览：");
            var first = grid.Rows.Cast<DataGridViewRow>().First(r => !r.IsNewRow);
            foreach (var c in cols.Take(8))
                sb.AppendLine($"  {c.HeaderText} = {first.Cells[c.Index].Value}");
        }
        return sb.ToString();
    }

    public static void CopyText(string text, string module)
    {
        try
        {
            Clipboard.SetText(text);
            Log.Write("复制摘要", module, $"len={text.Length}");
            MessageBox.Show("摘要已复制到剪贴板。", "复制摘要");
        }
        catch (Exception ex)
        {
            MessageBox.Show("复制失败：" + ex.Message);
        }
    }

    public static void ShowPrintPreview(DataGridView grid, string title)
    {
        try
        {
            var cols = VisibleColumns(grid).ToList();
            var rows = grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToList();
            int pageIndex = 0;
            int rowsPerPage = 35;
            var doc = new PrintDocument();
            doc.DocumentName = title;
            doc.DefaultPageSettings.Landscape = cols.Count > 6;
            doc.PrintPage += (_, e) =>
            {
                if (e.Graphics == null) return;
                float y = e.MarginBounds.Top;
                using var titleFont = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
                using var font = new Font("Microsoft YaHei UI", 8F);
                using var brush = new SolidBrush(Color.Black);
                e.Graphics.DrawString($"{title}  —  第 {pageIndex + 1} 页  —  {DateTime.Now:yyyy-MM-dd HH:mm}",
                    titleFont, brush, e.MarginBounds.Left, y);
                y += 28;
                var colWidth = Math.Max(60f, (e.MarginBounds.Width - 10f) / Math.Max(1, cols.Count));
                float x = e.MarginBounds.Left;
                foreach (var c in cols)
                {
                    e.Graphics.DrawString(c.HeaderText, font, brush, new RectangleF(x, y, colWidth, 18));
                    x += colWidth;
                }
                y += 20;
                e.Graphics.DrawLine(Pens.Gray, e.MarginBounds.Left, y, e.MarginBounds.Right, y);
                y += 4;
                int start = pageIndex * rowsPerPage;
                int end = Math.Min(start + rowsPerPage, rows.Count);
                for (int i = start; i < end; i++)
                {
                    x = e.MarginBounds.Left;
                    foreach (var c in cols)
                    {
                        var text = rows[i].Cells[c.Index].Value?.ToString() ?? "";
                        if (text.Length > 18) text = text[..18] + "…";
                        e.Graphics.DrawString(text, font, brush, new RectangleF(x, y, colWidth, 16));
                        x += colWidth;
                    }
                    y += 18;
                    if (y > e.MarginBounds.Bottom - 20) break;
                }
                pageIndex++;
                e.HasMorePages = pageIndex * rowsPerPage < rows.Count;
                if (!e.HasMorePages) pageIndex = 0;
            };

            using var preview = new PrintPreviewDialog
            {
                Document = doc,
                Width = 1000,
                Height = 700,
                StartPosition = FormStartPosition.CenterParent,
                Text = "打印预览 - " + title
            };
            Log.Write("打印预览", title, $"rows={rows.Count}");
            preview.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show("打印预览失败：" + ex.Message);
        }
    }

    public static void OpenDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            System.Diagnostics.Process.Start("explorer.exe", path);
            Log.Write("打开目录", "导出", path);
        }
        catch (Exception ex)
        {
            MessageBox.Show("无法打开目录：" + ex.Message);
        }
    }

    private static IEnumerable<DataGridViewColumn> VisibleColumns(DataGridView grid)
        => grid.Columns.Cast<DataGridViewColumn>()
            .Where(c => c.Visible && !string.Equals(c.Name, "Id", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(c.HeaderText, "Id", StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.DisplayIndex);

    private static string CsvEscape(string s)
    {
        if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }

    private static string HtmlEscape(string s)
        => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "export" : name.Trim();
    }
}
