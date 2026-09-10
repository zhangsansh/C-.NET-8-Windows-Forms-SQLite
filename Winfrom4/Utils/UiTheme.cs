namespace BankManagementSystem.Utils;

public static class UiTheme
{
    public static readonly Color Primary = Color.FromArgb(15, 76, 129);
    public static readonly Color PrimaryDark = Color.FromArgb(10, 55, 95);
    public static readonly Color Accent = Color.FromArgb(0, 150, 136);
    public static readonly Color Bg = Color.FromArgb(245, 247, 250);
    public static readonly Color Panel = Color.White;
    public static readonly Color TextMuted = Color.FromArgb(100, 110, 120);
    public static readonly Color Danger = Color.FromArgb(198, 40, 40);
    public static readonly Color Warning = Color.FromArgb(245, 124, 0);

    public static void StyleTopNavButton(Button btn, bool active = false)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 100, 155);
        btn.Cursor = Cursors.Hand;
        btn.Size = new Size(92, 68);
        btn.Margin = new Padding(2, 4, 2, 4);
        btn.TextAlign = ContentAlignment.BottomCenter;
        btn.Padding = new Padding(2, 28, 2, 6);
        btn.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Regular);
        btn.Image = null;
        if (active)
        {
            btn.BackColor = Accent;
            btn.ForeColor = Color.White;
        }
        else
        {
            btn.BackColor = Primary;
            btn.ForeColor = Color.White;
        }
    }

    /// <summary>在按钮上绘制字形图标（不使用 Image，避免 ImageAnimator 异常）。</summary>
    public static void AttachNavGlyph(Button btn, string glyph)
    {
        void Handler(object? sender, PaintEventArgs e)
        {
            if (sender is not Button b) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Font font;
            try { font = new Font("Segoe MDL2 Assets", 18F, FontStyle.Regular, GraphicsUnit.Pixel); }
            catch { font = new Font("Segoe UI Symbol", 16F, FontStyle.Regular, GraphicsUnit.Pixel); }

            using (font)
            using (var brush = new SolidBrush(b.ForeColor))
            {
                var rect = new RectangleF(0, 4, b.Width, 28);
                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(glyph, font, brush, rect, sf);
            }
        }

        btn.Paint += Handler;
    }

    public static void StylePrimaryButton(Button btn)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = Primary;
        btn.ForeColor = Color.White;
        btn.Cursor = Cursors.Hand;
        btn.Font = new Font("Microsoft YaHei UI", 9.5F);
        btn.Height = 34;
        btn.Padding = new Padding(8, 0, 8, 0);
    }

    public static void StyleDangerButton(Button btn)
    {
        StylePrimaryButton(btn);
        btn.BackColor = Danger;
    }

    public static void StyleDataGrid(DataGridView dgv)
    {
        dgv.BackgroundColor = Color.White;
        dgv.BorderStyle = BorderStyle.FixedSingle;
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Primary;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        dgv.ColumnHeadersHeight = 36;
        dgv.RowTemplate.Height = 30;
        dgv.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F);
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 230, 225);
        dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 244, 248);
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        // DisplayedCells：列宽按内容展开，超出时出现横向/纵向滚动条
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgv.ScrollBars = ScrollBars.Both;
        dgv.RowHeadersVisible = false;
        dgv.AllowUserToResizeColumns = true;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
    }

    /// <summary>页面级可滚动容器，保证内容超出时可拖动滑条查看全部信息。</summary>
    public static Panel CreateScrollHost(Control content, Padding? padding = null)
    {
        var host = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Bg,
            Padding = padding ?? new Padding(8)
        };
        content.Dock = DockStyle.Top;
        content.AutoSize = true;
        host.Controls.Add(content);
        host.Resize += (_, _) =>
        {
            if (content.Dock == DockStyle.Top)
                content.Width = Math.Max(host.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4, 400);
        };
        return host;
    }

    /// <summary>表格外再包一层带边框的滚动区域（表格本身 Dock.Fill 时用）。</summary>
    public static Panel WrapGrid(DataGridView grid)
    {
        var wrap = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
            BackColor = Color.FromArgb(220, 225, 230),
            AutoScroll = false
        };
        grid.Dock = DockStyle.Fill;
        StyleDataGrid(grid);
        wrap.Controls.Add(grid);
        return wrap;
    }

    public static Panel CreateCard(string title, Control content, int height = 180)
    {
        var panel = new Panel
        {
            BackColor = Panel,
            Height = height,
            Margin = new Padding(8),
            Padding = new Padding(12)
        };
        panel.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(220, 225, 230));
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };

        var lbl = new Label
        {
            Text = title,
            Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
            ForeColor = Primary,
            Dock = DockStyle.Top,
            Height = 28
        };
        content.Dock = DockStyle.Fill;
        panel.Controls.Add(content);
        panel.Controls.Add(lbl);
        return panel;
    }
}
