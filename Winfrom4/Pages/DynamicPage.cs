namespace BankManagementSystem.Pages;

using System.Diagnostics;
using BankManagementSystem.Controls;
using BankManagementSystem.Data;
using BankManagementSystem.Models;
using BankManagementSystem.Services;
using BankManagementSystem.Utils;

/// <summary>自定义页面运行时：标题 + 业务用户控件 + 用户按钮控件 + 自定义按钮。</summary>
public class DynamicPageView : UserControl
{
    private readonly CustomPage _page;
    private readonly LogService _log = new();
    private Panel? _contentHost;

    public DynamicPageView(CustomPage page)
    {
        _page = page;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Bg;
        Build();
    }

    private void Build()
    {
        Controls.Clear();
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));

        var contentWidget = PageWidgetRegistry.Get(_page.ControlKey);
        var barWidget = PageWidgetRegistry.GetButtonBar(_page.ButtonBarKey);

        var header = new Panel { Dock = DockStyle.Fill };
        header.Controls.Add(new Label
        {
            Text = _page.Title,
            Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            AutoSize = true,
            Location = new Point(0, 0)
        });
        header.Controls.Add(new Label
        {
            Text = $"{_page.Description}    ［内容：{contentWidget.DisplayName}｜按钮栏：{barWidget.DisplayName}］",
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(0, 36)
        });
        root.Controls.Add(header, 0, 0);

        _contentHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8) };
        try
        {
            var control = contentWidget.Create();
            control.Dock = DockStyle.Fill;
            contentWidget.Apply(control, PageWidgetRegistry.ParseConfig(_page.ControlConfigJson));
            _contentHost.Controls.Add(control);
        }
        catch (Exception ex)
        {
            _contentHost.Controls.Add(new Label
            {
                Text = "用户控件加载失败：" + ex.Message,
                Dock = DockStyle.Fill,
                ForeColor = UiTheme.Danger,
                TextAlign = ContentAlignment.MiddleCenter
            });
        }
        root.Controls.Add(_contentHost, 0, 1);

        var barHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(250, 252, 255), Padding = new Padding(4) };
        try
        {
            var bar = barWidget.Create();
            bar.Dock = DockStyle.Fill;
            barWidget.Apply(bar, PageWidgetRegistry.ParseConfig(_page.ButtonBarConfigJson));
            if (bar is IToolbarHost toolbar)
                toolbar.ToolbarAction += OnToolbarAction;
            barHost.Controls.Add(bar);
        }
        catch (Exception ex)
        {
            barHost.Controls.Add(new Label { Text = "按钮控件加载失败：" + ex.Message, Dock = DockStyle.Fill, ForeColor = UiTheme.Danger });
        }
        root.Controls.Add(barHost, 0, 2);

        var customBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            AutoScroll = true,
            Padding = new Padding(0, 4, 0, 0)
        };
        var buttons = CustomPageService.ParseButtons(_page.ButtonsJson);
        if (buttons.Count == 0)
        {
            customBar.Controls.Add(new Label
            {
                Text = "未配置额外自定义按钮（可在编辑页添加 Message/Refresh/OpenUrl 等）。",
                AutoSize = true,
                ForeColor = UiTheme.TextMuted
            });
        }
        foreach (var btnDef in buttons)
        {
            var b = new Button
            {
                Text = btnDef.Text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(100, 34),
                Margin = new Padding(0, 0, 10, 6),
                Padding = new Padding(12, 2, 12, 2)
            };
            UiTheme.StylePrimaryButton(b);
            b.AutoSize = true;
            b.MinimumSize = new Size(100, 34);
            var def = btnDef;
            b.Click += (_, _) => Execute(def);
            customBar.Controls.Add(b);
        }
        root.Controls.Add(customBar, 0, 3);
        Controls.Add(root);
    }

    private DataGridView? ContentGrid() => DataExportHelper.FindFirstGrid(_contentHost);

    private void OnToolbarAction(string action)
    {
        _log.Write($"按钮栏:{action}", _page.Title, action);
        var grid = ContentGrid();
        switch (action)
        {
            case "Refresh":
                Build();
                break;
            case "Add":
            case "Edit":
            case "Delete":
            case "View":
            case "Save":
                MessageBox.Show($"用户按钮控件动作：{action}\n可与内容区「增删改查」类页面控件配合使用。\n正式维护请到对应业务页。", _page.Title);
                break;
            case "Search":
            case "Reset":
            case "AdvancedSearch":
                if (action == "Refresh" || grid == null)
                    MessageBox.Show($"筛选动作：{action}", _page.Title);
                else
                    MessageBox.Show($"已触发 {action}。内容区表格当前约 {grid.Rows.Count} 行。", _page.Title);
                break;
            case "ExportCsv":
                if (grid == null) { MessageBox.Show("当前内容区没有可导出的表格。"); return; }
                DataExportHelper.ExportToCsv(grid, _page.Title);
                break;
            case "ExportTable":
                if (grid == null) { MessageBox.Show("当前内容区没有可导出的表格。"); return; }
                DataExportHelper.ExportToExcelTable(grid, _page.Title);
                break;
            case "CopySummary":
                {
                    var summary = grid != null
                        ? DataExportHelper.BuildGridSummary(grid, _page.Title)
                        : $"{_page.Title}\n{_page.Description}\n{DateTime.Now:yyyy-MM-dd HH:mm}";
                    DataExportHelper.CopyText(summary, _page.Title);
                }
                break;
            case "Print":
                if (grid == null) { MessageBox.Show("当前内容区没有可打印的表格。"); return; }
                DataExportHelper.ShowPrintPreview(grid, _page.Title);
                break;
            case "OpenExportFolder":
                DataExportHelper.OpenDirectory(DataExportHelper.ExportFolder);
                break;
            case "OpenLogFolder":
                DataExportHelper.OpenDirectory(DatabaseHelper.LogFolder);
                break;
            case "Deposit":
            case "Withdraw":
            case "Transfer":
            case "InterestCalc":
                MessageBox.Show($"金融动作：{action}\n正式存取款请到「客户」页；利息可选用利息试算内容控件。", _page.Title);
                break;
            case "HomeHelp":
            case "Help":
                MessageBox.Show("在「页面扩展」中可为每页选择内容用户控件与按钮用户控件。\n导出类按钮栏可对内容区表格执行 CSV/表格导出、复制、打印。", "使用帮助");
                break;
            case "About":
                MessageBox.Show("华夏示范银行管理系统\n分层架构：Pages → Controls → Services → Data", "关于系统");
                break;
            default:
                MessageBox.Show($"未处理动作：{action}", _page.Title);
                break;
        }
    }

    private void Execute(PageButtonDef def)
    {
        _log.Write($"自定义按钮:{def.Text}", _page.Title, $"{def.Action} {def.Param}");
        switch (def.Action)
        {
            case "Message":
                MessageBox.Show(string.IsNullOrWhiteSpace(def.Param) ? def.Text : def.Param, _page.Title);
                break;
            case "Refresh":
                Build();
                break;
            case "OpenUrl":
                if (!string.IsNullOrWhiteSpace(def.Param))
                {
                    try { Process.Start(new ProcessStartInfo(def.Param) { UseShellExecute = true }); }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
                break;
            case "OpenFolder":
                if (!string.IsNullOrWhiteSpace(def.Param) && Directory.Exists(def.Param))
                    Process.Start("explorer.exe", def.Param);
                else
                    MessageBox.Show("目录不存在：" + def.Param);
                break;
            default:
                MessageBox.Show($"执行自定义动作：{def.Action}\n参数：{def.Param}", _page.Title);
                break;
        }
    }
}

public class PageManagerPage : UserControl
{
    private readonly CustomPageService _svc = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private List<CustomPage> _data = new();
    public event Action? PagesChanged;

    public PageManagerPage()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Bg;
        Padding = new Padding(12);
        Build();
        LoadData();
    }

    private void Build()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = false };
        Button Mk(string t, EventHandler h)
            => DataExportHelper.CreateToolButton(t, h, 100);
        bar.Controls.Add(Mk("刷新", (_, _) => LoadData()));
        bar.Controls.Add(Mk("新增页面", BtnAdd));
        bar.Controls.Add(Mk("编辑页面", BtnEdit));
        bar.Controls.Add(Mk("删除页面", BtnDelete));
        DataExportHelper.AppendExportButtons(
            bar.Controls,
            () => _grid,
            "自定义页面列表",
            () => DataExportHelper.BuildGridSummary(_grid, "自定义页面列表"));
        bar.Controls.Add(new Label
        {
            Text = "可分别选择「业务用户控件」与「用户按钮控件」，并配置各自功能",
            AutoSize = true,
            Margin = new Padding(8, 12, 0, 0),
            ForeColor = UiTheme.TextMuted
        });
        root.Controls.Add(bar, 0, 0);
        root.Controls.Add(UiTheme.WrapGrid(_grid), 0, 1);
        Controls.Add(root);
    }

    private void LoadData()
    {
        _data = _svc.GetAll();
        _grid.DataSource = _data.Select(p => new
        {
            标题 = p.Title,
            标识 = p.PageKey,
            内容控件 = PageWidgetRegistry.Get(p.ControlKey).DisplayName,
            按钮控件 = PageWidgetRegistry.GetButtonBar(p.ButtonBarKey).DisplayName,
            说明 = p.Description,
            管理员 = p.VisibleToAdmin ? "是" : "否",
            超管 = p.VisibleToSuperAdmin ? "是" : "否",
            客户 = p.VisibleToCustomer ? "是" : "否",
            排序 = p.SortOrder,
            自定义按钮 = CustomPageService.ParseButtons(p.ButtonsJson).Count,
            Id = p.Id
        }).ToList();
        if (_grid.Columns["Id"] != null) _grid.Columns["Id"].Visible = false;
    }

    private CustomPage? Selected()
    {
        if (_grid.CurrentRow == null) return null;
        var id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        return _data.FirstOrDefault(p => p.Id == id);
    }

    private void BtnAdd(object? s, EventArgs e)
    {
        using var dlg = new CustomPageEditDialog();
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.Result == null) return;
        _svc.Add(dlg.Result);
        LoadData();
        PagesChanged?.Invoke();
    }

    private void BtnEdit(object? s, EventArgs e)
    {
        var p = Selected();
        if (p == null) { MessageBox.Show("请选择页面"); return; }
        using var dlg = new CustomPageEditDialog(p);
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.Result == null) return;
        _svc.Update(dlg.Result);
        LoadData();
        PagesChanged?.Invoke();
    }

    private void BtnDelete(object? s, EventArgs e)
    {
        var p = Selected();
        if (p == null) { MessageBox.Show("请选择页面"); return; }
        if (MessageBox.Show($"删除页面 {p.Title}？", "确认", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        _svc.Delete(p.Id);
        LoadData();
        PagesChanged?.Invoke();
    }
}

/// <summary>新建/编辑：选择内容用户控件 + 用户按钮控件并分别配置功能。</summary>
public class CustomPageEditDialog : Form
{
    public CustomPage? Result { get; private set; }

    private readonly TextBox _title = new();
    private readonly TextBox _key = new();
    private readonly TextBox _desc = new();
    private readonly TextBox _order = new() { Width = 80, Text = "0" };
    private readonly ComboBox _widget = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _widgetDesc = new() { AutoSize = false, ForeColor = UiTheme.TextMuted };
    private readonly CheckedListBox _features = new() { CheckOnClick = true };
    private readonly ComboBox _buttonBar = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _buttonBarDesc = new() { AutoSize = false, ForeColor = UiTheme.TextMuted };
    private readonly CheckedListBox _buttonFeatures = new() { CheckOnClick = true };
    private readonly TextBox _notice = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly CheckBox _va = new() { Text = "管理员可见", Checked = true, AutoSize = true };
    private readonly CheckBox _vs = new() { Text = "超管可见", Checked = true, AutoSize = true };
    private readonly CheckBox _vc = new() { Text = "客户可见", AutoSize = true };
    private readonly DataGridView _btnGrid = new();
    private readonly int _id;

    public CustomPageEditDialog(CustomPage? existing = null)
    {
        _id = existing?.Id ?? 0;
        Text = existing == null ? "新增自定义页面" : "编辑自定义页面";
        ClientSize = new Size(760, 780);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Font = new Font("Microsoft YaHei UI", 9.5F);
        BuildLayout(existing);
    }

    private void BuildLayout(CustomPage? existing)
    {
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
        var layout = new TableLayoutPanel
        {
            Location = new Point(0, 0),
            ColumnCount = 2,
            AutoSize = true,
            Width = 720
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int row = 0;
        void AddRow(string label, Control c, int height = 34)
        {
            layout.RowCount = row + 1;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            layout.Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 8, 0)
            }, 0, row);
            c.Dock = DockStyle.Fill;
            c.Margin = new Padding(0, 4, 0, 4);
            layout.Controls.Add(c, 1, row);
            row++;
        }

        AddRow("标题", _title);
        AddRow("标识 Key", _key);
        AddRow("说明", _desc);

        foreach (var w in PageWidgetRegistry.ContentWidgets)
            _widget.Items.Add(w);
        _widget.DisplayMember = nameof(IPageWidget.DisplayName);
        _widget.SelectedIndexChanged += (_, _) => ReloadContentFeatures();
        AddRow("内容用户控件", _widget);
        AddRow("内容说明", _widgetDesc, 48);
        AddRow("内容功能", _features, 110);
        AddRow("公告文本", _notice, 72);

        foreach (var w in PageWidgetRegistry.ButtonBarWidgets)
            _buttonBar.Items.Add(w);
        _buttonBar.DisplayMember = nameof(IPageWidget.DisplayName);
        _buttonBar.SelectedIndexChanged += (_, _) => ReloadButtonFeatures();
        AddRow("用户按钮控件", _buttonBar);
        AddRow("按钮栏说明", _buttonBarDesc, 40);
        AddRow("按钮栏功能", _buttonFeatures, 110);

        var vis = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoSize = true };
        vis.Controls.Add(new Label { Text = "排序", AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
        vis.Controls.Add(_order);
        _va.Margin = new Padding(12, 6, 8, 0);
        _vs.Margin = new Padding(0, 6, 8, 0);
        _vc.Margin = new Padding(0, 6, 0, 0);
        vis.Controls.Add(_va);
        vis.Controls.Add(_vs);
        vis.Controls.Add(_vc);
        AddRow("权限", vis, 40);

        layout.RowCount = row + 1;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        var tip = new Label
        {
            Text = "额外自定义按钮（Action: Message / Refresh / OpenUrl / OpenFolder / Custom）",
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 0)
        };
        layout.Controls.Add(tip, 0, row);
        layout.SetColumnSpan(tip, 2);
        row++;

        _btnGrid.AllowUserToAddRows = true;
        _btnGrid.AllowUserToDeleteRows = true;
        _btnGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _btnGrid.RowHeadersVisible = false;
        _btnGrid.Columns.Add("Text", "按钮文字");
        _btnGrid.Columns.Add("Action", "动作");
        _btnGrid.Columns.Add("Param", "参数");
        layout.RowCount = row + 1;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        _btnGrid.Dock = DockStyle.Fill;
        layout.Controls.Add(_btnGrid, 0, row);
        layout.SetColumnSpan(_btnGrid, 2);

        scroll.Controls.Add(layout);
        scroll.Resize += (_, _) => layout.Width = Math.Max(680, scroll.ClientSize.Width - 28);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12, 8, 12, 8)
        };
        var ok = new Button { Text = "保存", AutoSize = true, MinimumSize = new Size(100, 32), Padding = new Padding(16, 2, 16, 2) };
        var cancel = new Button { Text = "取消", AutoSize = true, MinimumSize = new Size(100, 32), DialogResult = DialogResult.Cancel, Margin = new Padding(0, 0, 8, 0), Padding = new Padding(16, 2, 16, 2) };
        UiTheme.StylePrimaryButton(ok);
        UiTheme.StylePrimaryButton(cancel);
        ok.AutoSize = true;
        cancel.AutoSize = true;
        cancel.BackColor = Color.FromArgb(120, 130, 140);
        ok.Click += (_, _) => Save();
        bottom.Controls.Add(ok);
        bottom.Controls.Add(cancel);

        Controls.Add(scroll);
        Controls.Add(bottom);
        AcceptButton = ok;
        CancelButton = cancel;

        if (existing != null)
        {
            _title.Text = existing.Title;
            _key.Text = existing.PageKey;
            _desc.Text = existing.Description;
            _order.Text = existing.SortOrder.ToString();
            _va.Checked = existing.VisibleToAdmin;
            _vs.Checked = existing.VisibleToSuperAdmin;
            _vc.Checked = existing.VisibleToCustomer;
            var cfg = PageWidgetRegistry.ParseConfig(existing.ControlConfigJson);
            _notice.Text = cfg.NoticeText;
            SelectCombo(_widget, existing.ControlKey);
            ApplyChecks(_features, cfg);
            SelectCombo(_buttonBar, existing.ButtonBarKey);
            ApplyChecks(_buttonFeatures, PageWidgetRegistry.ParseConfig(existing.ButtonBarConfigJson));
            foreach (var b in CustomPageService.ParseButtons(existing.ButtonsJson))
                _btnGrid.Rows.Add(b.Text, b.Action, b.Param);
        }
        else
        {
            _key.Text = "page_" + DateTime.Now.ToString("HHmmss");
            SelectCombo(_widget, "product_crud");
            SelectCombo(_buttonBar, "btn_crud");
            _btnGrid.Rows.Add("示例提示", "Message", "这是额外自定义按钮");
            _notice.Text = "欢迎使用银行管理系统自定义页面。";
        }
    }

    private static void SelectCombo(ComboBox combo, string key)
    {
        for (int i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is IPageWidget w && w.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                combo.SelectedIndex = i;
                return;
            }
        }
        if (combo.Items.Count > 0) combo.SelectedIndex = 0;
    }

    private void ReloadContentFeatures()
    {
        _features.Items.Clear();
        if (_widget.SelectedItem is not IPageWidget w)
        {
            _widgetDesc.Text = "";
            return;
        }
        _widgetDesc.Text = w.Description;
        foreach (var f in w.Features)
            _features.Items.Add(f, f.DefaultEnabled);
        _notice.Enabled = w.Key == "notice_board";
    }

    private void ReloadButtonFeatures()
    {
        _buttonFeatures.Items.Clear();
        if (_buttonBar.SelectedItem is not IPageWidget w)
        {
            _buttonBarDesc.Text = "";
            return;
        }
        _buttonBarDesc.Text = w.Description;
        foreach (var f in w.Features)
            _buttonFeatures.Items.Add(f, f.DefaultEnabled);
    }

    private static void ApplyChecks(CheckedListBox list, WidgetConfig cfg)
    {
        for (int i = 0; i < list.Items.Count; i++)
        {
            if (list.Items[i] is WidgetFeature f)
                list.SetItemChecked(i, cfg.IsEnabled(f.Key, f.DefaultEnabled));
        }
    }

    private static WidgetConfig CollectConfig(CheckedListBox list, string notice = "")
    {
        var cfg = new WidgetConfig { NoticeText = notice };
        for (int i = 0; i < list.Items.Count; i++)
        {
            if (list.Items[i] is WidgetFeature f)
                cfg.Features[f.Key] = list.GetItemChecked(i);
        }
        return cfg;
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_title.Text) || string.IsNullOrWhiteSpace(_key.Text))
        {
            MessageBox.Show("标题和标识不能为空");
            return;
        }
        if (!int.TryParse(_order.Text, out var order)) order = 0;
        if (_widget.SelectedItem is not IPageWidget widget)
        {
            MessageBox.Show("请选择内容用户控件");
            return;
        }
        if (_buttonBar.SelectedItem is not IPageWidget buttonBar)
        {
            MessageBox.Show("请选择用户按钮控件");
            return;
        }

        var buttons = new List<PageButtonDef>();
        foreach (DataGridViewRow gridRow in _btnGrid.Rows)
        {
            if (gridRow.IsNewRow) continue;
            var text = gridRow.Cells[0].Value?.ToString()?.Trim();
            if (string.IsNullOrEmpty(text)) continue;
            buttons.Add(new PageButtonDef
            {
                Text = text,
                Action = gridRow.Cells[1].Value?.ToString()?.Trim() ?? "Message",
                Param = gridRow.Cells[2].Value?.ToString() ?? ""
            });
        }

        Result = new CustomPage
        {
            Id = _id,
            Title = _title.Text.Trim(),
            PageKey = _key.Text.Trim(),
            Description = _desc.Text.Trim(),
            SortOrder = order,
            VisibleToAdmin = _va.Checked,
            VisibleToSuperAdmin = _vs.Checked,
            VisibleToCustomer = _vc.Checked,
            ControlKey = widget.Key,
            ControlConfigJson = PageWidgetRegistry.SerializeConfig(CollectConfig(_features, _notice.Text.Trim())),
            ButtonBarKey = buttonBar.Key,
            ButtonBarConfigJson = PageWidgetRegistry.SerializeConfig(CollectConfig(_buttonFeatures)),
            ButtonsJson = CustomPageService.SerializeButtons(buttons)
        };
        DialogResult = DialogResult.OK;
        Close();
    }
}
