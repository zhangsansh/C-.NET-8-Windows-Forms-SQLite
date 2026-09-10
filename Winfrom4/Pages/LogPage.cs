namespace BankManagementSystem.Pages;

using BankManagementSystem.Data;
using BankManagementSystem.Services;
using BankManagementSystem.Utils;

public class LogPage : UserControl
{
    private readonly LogService _log = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly TextBox _keyword = new() { Width = 160 };
    private readonly DateTimePicker _from = new() { Format = DateTimePickerFormat.Short, Width = 120 };
    private readonly DateTimePicker _to = new() { Format = DateTimePickerFormat.Short, Width = 120 };
    private readonly TextBox _logFolder = new() { Dock = DockStyle.Fill };
    private readonly CheckBox _enableFile = new() { Text = "启用按日文件日志", AutoSize = true };

    public LogPage()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Bg;
        Padding = new Padding(12);
        _from.Value = DateTime.Today.AddDays(-7);
        _to.Value = DateTime.Today;
        BuildUi();
        LoadData();
    }

    private void BuildUi()
    {
        // 筛选 / 导出 / 设置 / 表格 分行，避免按钮挤在一行互相遮挡或被裁切
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var filter = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 2, 0, 0)
        };
        filter.Controls.Add(new Label { Text = "搜索", AutoSize = true, Margin = new Padding(0, 10, 4, 0) });
        filter.Controls.Add(_keyword);
        filter.Controls.Add(new Label { Text = "从", AutoSize = true, Margin = new Padding(8, 10, 4, 0) });
        filter.Controls.Add(_from);
        filter.Controls.Add(new Label { Text = "到", AutoSize = true, Margin = new Padding(8, 10, 4, 0) });
        filter.Controls.Add(_to);

        var search = DataExportHelper.CreateToolButton("搜索", (_, _) => LoadData(), 80);
        var reset = DataExportHelper.CreateToolButton("重置", (_, _) =>
        {
            _keyword.Clear();
            _from.Value = DateTime.Today.AddDays(-7);
            _to.Value = DateTime.Today;
            LoadData();
        }, 80);
        var open = DataExportHelper.CreateToolButton("打开日志目录", (_, _) =>
        {
            var folder = DatabaseHelper.LogFolder;
            Directory.CreateDirectory(folder);
            System.Diagnostics.Process.Start("explorer.exe", folder);
        }, 120);
        filter.Controls.Add(search);
        filter.Controls.Add(reset);
        filter.Controls.Add(open);
        _keyword.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { LoadData(); e.SuppressKeyPress = true; }
        };
        root.Controls.Add(filter, 0, 0);

        var exportBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            AutoScroll = true,
            Padding = new Padding(0, 2, 0, 0)
        };
        DataExportHelper.AppendExportButtons(
            exportBar.Controls,
            () => _grid,
            "操作日志",
            () => DataExportHelper.BuildGridSummary(_grid, "操作日志"),
            DatabaseHelper.LogFolder);
        root.Controls.Add(exportBar, 0, 1);

        // 设置区：独立行，保存按钮不与输入框重叠
        var settings = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(12, 8, 12, 8)
        };
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        settings.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        settings.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        settings.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        var title = new Label
        {
            Text = "日志参数设置（按日写入 log_yyyyMMdd.txt，同时写入 SQLite）",
            AutoSize = true,
            ForeColor = UiTheme.Primary,
            Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold),
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 0, 0)
        };
        settings.SetColumnSpan(title, 3);
        settings.Controls.Add(title, 0, 0);

        var dirLabel = new Label
        {
            Text = "日志目录",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 10, 8, 0)
        };
        _logFolder.Text = DatabaseHelper.GetSetting("LogFolder", DatabaseHelper.LogFolder);
        _logFolder.Margin = new Padding(0, 6, 8, 0);
        settings.Controls.Add(dirLabel, 0, 1);
        settings.Controls.Add(_logFolder, 1, 1);

        var save = new Button
        {
            Text = "保存设置",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(120, 34),
            Padding = new Padding(14, 4, 14, 4),
            Margin = new Padding(4, 4, 0, 0),
            Anchor = AnchorStyles.Left
        };
        UiTheme.StylePrimaryButton(save);
        save.AutoSize = true;
        save.MinimumSize = new Size(120, 34);
        save.Padding = new Padding(14, 4, 14, 4);
        save.Click += (_, _) => SaveSettings();
        settings.Controls.Add(save, 2, 1);

        _enableFile.Checked = DatabaseHelper.GetSetting("EnableFileLog", "1") == "1";
        _enableFile.Margin = new Padding(0, 8, 0, 0);
        settings.SetColumnSpan(_enableFile, 2);
        settings.Controls.Add(_enableFile, 1, 2);

        if (!Session.IsAdmin)
        {
            _logFolder.ReadOnly = true;
            _enableFile.Enabled = false;
            save.Enabled = false;
        }

        root.Controls.Add(settings, 0, 2);
        root.Controls.Add(UiTheme.WrapGrid(_grid), 0, 3);
        Controls.Add(root);
    }

    private void SaveSettings()
    {
        if (!Session.IsAdmin)
        {
            MessageBox.Show("仅管理员可修改日志设置");
            return;
        }
        var path = _logFolder.Text.Trim();
        try { Directory.CreateDirectory(path); }
        catch (Exception ex)
        {
            MessageBox.Show("目录无效：" + ex.Message);
            return;
        }
        DatabaseHelper.SetSetting("LogFolder", path);
        DatabaseHelper.SetSetting("EnableFileLog", _enableFile.Checked ? "1" : "0");
        _log.Write("更新日志设置", "日志", $"目录={path}, 文件日志={_enableFile.Checked}");
        MessageBox.Show("日志设置已保存");
    }

    private void LoadData()
    {
        var from = _from.Value.Date;
        var to = _to.Value.Date.AddDays(1).AddTicks(-1);
        var logs = _log.GetLogs(from, to, _keyword.Text);
        if (Session.IsCustomer)
            logs = logs.Where(l => l.Username == Session.CurrentUser?.Username).ToList();

        _grid.DataSource = logs.Select(l => new
        {
            时间 = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            用户 = l.Username,
            角色 = l.Role,
            模块 = l.Module,
            操作 = l.Action,
            详情 = l.Detail,
            日志文件 = Path.GetFileName(l.LogFilePath)
        }).ToList();
    }
}
