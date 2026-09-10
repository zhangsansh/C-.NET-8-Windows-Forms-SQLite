namespace BankManagementSystem.Controls;

using BankManagementSystem.Services;
using BankManagementSystem.Utils;

public sealed class ProductListControl : UserControl
{
    private readonly ProductService _svc = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly Button _refresh = new() { Text = "刷新产品", AutoSize = true, MinimumSize = new Size(100, 32) };
    private WidgetConfig _config = new();

    public ProductListControl()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Bg;
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, AutoScroll = true, WrapContents = false };
        UiTheme.StylePrimaryButton(_refresh);
        _refresh.AutoSize = true;
        _refresh.Click += (_, _) => LoadData();
        bar.Controls.Add(_refresh);
        DataExportHelper.AppendExportButtons(bar.Controls, () => _grid, "产品列表控件");
        Controls.Add(UiTheme.WrapGrid(_grid));
        Controls.Add(bar);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        _config = config;
        _refresh.Visible = config.IsEnabled("AllowRefresh");
        LoadData();
    }

    private void LoadData()
    {
        var activeOnly = _config.IsEnabled("ActiveOnly");
        var list = _svc.GetAll(activeOnly);
        var showYield = _config.IsEnabled("ShowYield");
        var showProfit = _config.IsEnabled("ShowProfit") && Session.IsAdmin;

        if (showYield && showProfit)
        {
            _grid.DataSource = list.Select(p => new
            {
                代码 = p.Code,
                名称 = p.Name,
                类别 = p.Category,
                价格 = p.Price.ToString("N2"),
                收益率 = p.YieldRate.ToString("P2"),
                盈利 = p.Profit.ToString("N0"),
                说明 = p.Description
            }).ToList();
        }
        else if (showYield)
        {
            _grid.DataSource = list.Select(p => new
            {
                代码 = p.Code,
                名称 = p.Name,
                类别 = p.Category,
                价格 = p.Price.ToString("N2"),
                收益率 = p.YieldRate.ToString("P2"),
                说明 = p.Description
            }).ToList();
        }
        else if (showProfit)
        {
            _grid.DataSource = list.Select(p => new
            {
                代码 = p.Code,
                名称 = p.Name,
                类别 = p.Category,
                价格 = p.Price.ToString("N2"),
                盈利 = p.Profit.ToString("N0"),
                说明 = p.Description
            }).ToList();
        }
        else
        {
            _grid.DataSource = list.Select(p => new
            {
                代码 = p.Code,
                名称 = p.Name,
                类别 = p.Category,
                价格 = p.Price.ToString("N2"),
                说明 = p.Description
            }).ToList();
        }
    }
}

public sealed class CustomerSummaryControl : UserControl
{
    private readonly CustomerService _svc = new();
    private readonly Label _stats = new() { Dock = DockStyle.Top, Height = 90, Font = new Font("Microsoft YaHei UI", 10F) };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private WidgetConfig _config = new();

    public CustomerSummaryControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Padding = new Padding(8);
        Controls.Add(UiTheme.WrapGrid(_grid));
        Controls.Add(_stats);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        _config = config;
        var all = _svc.GetAll();
        if (Session.IsCustomer && Session.CurrentUser?.CustomerId is int cid)
            all = all.Where(c => c.Id == cid).ToList();

        var lines = new List<string> { $"客户总数：{all.Count}" };
        if (config.IsEnabled("ShowBalanceSum"))
            lines.Add($"账户余额合计：{all.Sum(c => c.Balance):N2} 元");
        if (config.IsEnabled("ShowHighValue"))
            lines.Add($"高净值客户：{all.Count(c => c.ValueAssessment == "高净值")} 人");
        lines.Add($"总账号价值合计：{all.Sum(c => c.TotalValue):N2} 元");
        _stats.Text = string.Join(Environment.NewLine, lines);

        _grid.Visible = config.IsEnabled("ShowList");
        if (_grid.Visible)
        {
            _grid.DataSource = all.Take(50).Select(c => new
            {
                编号 = c.CustomerNo,
                姓名 = c.Name,
                余额 = c.Balance.ToString("N2"),
                评估 = c.ValueAssessment,
                总价值 = c.TotalValue.ToString("N2")
            }).ToList();
        }
    }
}

public sealed class RateBoardControl : UserControl
{
    private readonly BankService _svc = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly Button _refresh = new() { Text = "刷新利率", AutoSize = true, MinimumSize = new Size(100, 32) };
    private WidgetConfig _config = new();

    public RateBoardControl()
    {
        Dock = DockStyle.Fill;
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, AutoScroll = true, WrapContents = false };
        UiTheme.StylePrimaryButton(_refresh);
        _refresh.AutoSize = true;
        _refresh.Click += (_, _) => LoadData();
        bar.Controls.Add(_refresh);
        DataExportHelper.AppendExportButtons(bar.Controls, () => _grid, "利率看板控件");
        Controls.Add(UiTheme.WrapGrid(_grid));
        Controls.Add(bar);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        _config = config;
        _refresh.Visible = config.IsEnabled("AllowRefresh");
        LoadData();
    }

    private void LoadData()
    {
        var rates = _svc.GetRates();
        var showDesc = _config.IsEnabled("ShowDescription");
        if (showDesc)
        {
            _grid.DataSource = rates.Select(r => new
            {
                名称 = r.Name,
                期限 = r.Period,
                利率 = r.Rate.ToString("P2"),
                说明 = r.Description
            }).ToList();
        }
        else
        {
            _grid.DataSource = rates.Select(r => new
            {
                名称 = r.Name,
                期限 = r.Period,
                利率 = r.Rate.ToString("P2")
            }).ToList();
        }
    }
}

public sealed class NoticeBoardControl : UserControl
{
    private readonly Label _title = new()
    {
        Text = "系统公告",
        Dock = DockStyle.Top,
        Height = 32,
        Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
        ForeColor = UiTheme.Primary
    };
    private readonly TextBox _body = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Both,
        BorderStyle = BorderStyle.FixedSingle,
        Font = new Font("Microsoft YaHei UI", 10F)
    };

    public NoticeBoardControl()
    {
        Dock = DockStyle.Fill;
        Padding = new Padding(4);
        BackColor = Color.White;
        Controls.Add(_body);
        Controls.Add(_title);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        _title.Visible = config.IsEnabled("ShowTitle");
        _body.WordWrap = config.IsEnabled("WordWrap");
        _body.Text = string.IsNullOrWhiteSpace(config.NoticeText)
            ? "暂无公告内容。请在「页面扩展」中编辑该页，填写公告文本。"
            : config.NoticeText.Replace("\\n", Environment.NewLine);
    }
}

public sealed class InterestCalcControl : UserControl
{
    private readonly BankService _bank = new();
    private readonly TextBox _principal = new() { Width = 160, Text = "100000" };
    private readonly TextBox _rate = new() { Width = 120, Text = "0.015" };
    private readonly Label _result = new() { AutoSize = true, Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold), ForeColor = UiTheme.Primary };
    private WidgetConfig _config = new();

    public InterestCalcControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Padding = new Padding(16);
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 160,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        panel.Controls.Add(new Label { Text = "本金（元）", AutoSize = true });
        panel.Controls.Add(_principal);
        panel.Controls.Add(new Label { Text = "年利率（如 0.015 = 1.5%）", AutoSize = true, Margin = new Padding(0, 8, 0, 0) });
        panel.Controls.Add(_rate);
        var calc = new Button { Text = "试算年利息", Width = 120, Margin = new Padding(0, 12, 0, 0) };
        UiTheme.StylePrimaryButton(calc);
        calc.Click += (_, _) => DoCalc();
        panel.Controls.Add(calc);
        panel.Controls.Add(_result);
        Controls.Add(panel);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        _config = config;
        _rate.ReadOnly = !config.IsEnabled("AllowCustomRate");
        if (config.IsEnabled("UseBankRate"))
        {
            var rates = _bank.GetRates();
            var oneYear = rates.FirstOrDefault(r => r.Name.Contains("一年期"));
            if (oneYear != null)
                _rate.Text = oneYear.Rate.ToString("F4");
        }
        DoCalc();
    }

    private void DoCalc()
    {
        if (!decimal.TryParse(_principal.Text, out var p) || !decimal.TryParse(_rate.Text, out var r))
        {
            _result.Text = "请输入有效的本金与利率。";
            return;
        }
        var interest = p * r;
        _result.Text = $"预计年利息：{interest:N2} 元" + Environment.NewLine +
                       $"本息合计：{p + interest:N2} 元";
    }
}

/// <summary>产品增删改查页面控件（内置 CRUD 工具栏）。</summary>
public sealed class ProductCrudControl : UserControl
{
    private readonly ProductService _svc = new();
    private readonly LogService _log = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly FlowLayoutPanel _bar = new() { Dock = DockStyle.Top, Height = 44, WrapContents = false, AutoScroll = true };
    private WidgetConfig _config = new();

    public ProductCrudControl()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Bg;
        Controls.Add(UiTheme.WrapGrid(_grid));
        Controls.Add(_bar);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        _config = config;
        _bar.Controls.Clear();
        void Add(string text, EventHandler h, bool danger = false)
        {
            var b = new Button { Text = text, AutoSize = true, MinimumSize = new Size(80, 32), Margin = new Padding(0, 4, 8, 0), Padding = new Padding(10, 2, 10, 2) };
            if (danger) UiTheme.StyleDangerButton(b); else UiTheme.StylePrimaryButton(b);
            b.AutoSize = true;
            b.Click += h;
            _bar.Controls.Add(b);
        }
        if (config.IsEnabled("ShowRefresh")) Add("刷新", (_, _) => LoadData());
        if (config.IsEnabled("ShowAdd") && Session.IsAdmin) Add("新增产品", (_, _) => MessageBox.Show("请到「产品」页完成正式新增；此处为扩展页演示按钮。", "新增"));
        if (config.IsEnabled("ShowEdit") && Session.IsAdmin) Add("编辑产品", (_, _) =>
        {
            if (_grid.CurrentRow == null) { MessageBox.Show("请选择产品"); return; }
            MessageBox.Show($"演示编辑：{_grid.CurrentRow.Cells[0].Value}", "编辑");
        });
        if (config.IsEnabled("ShowDelete") && Session.IsAdmin) Add("删除产品", (_, _) =>
        {
            if (_grid.CurrentRow == null) { MessageBox.Show("请选择产品"); return; }
            MessageBox.Show($"演示删除：{_grid.CurrentRow.Cells[0].Value}\n正式删除请在「产品」页操作。", "删除");
        }, danger: true);
        DataExportHelper.AppendExportButtons(_bar.Controls, () => _grid, "产品CRUD控件");
        LoadData();
    }

    private void LoadData()
    {
        var list = _svc.GetAll(_config.IsEnabled("ActiveOnly"));
        _grid.DataSource = list.Select(p => new
        {
            代码 = p.Code,
            名称 = p.Name,
            类别 = p.Category,
            价格 = p.Price.ToString("N2"),
            收益率 = p.YieldRate.ToString("P2"),
            启用 = p.IsActive ? "是" : "否"
        }).ToList();
        _log.Write("加载产品CRUD控件", "页面扩展", $"count={list.Count}");
    }
}

/// <summary>客户增删改查页面控件。</summary>
public sealed class CustomerCrudControl : UserControl
{
    private readonly CustomerService _svc = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly FlowLayoutPanel _bar = new() { Dock = DockStyle.Top, Height = 44, AutoScroll = true };
    private WidgetConfig _config = new();

    public CustomerCrudControl()
    {
        Dock = DockStyle.Fill;
        Controls.Add(UiTheme.WrapGrid(_grid));
        Controls.Add(_bar);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        _config = config;
        _bar.Controls.Clear();
        void Add(string text, EventHandler h)
        {
            var b = new Button { Text = text, AutoSize = true, MinimumSize = new Size(88, 32), Margin = new Padding(0, 4, 8, 0), Padding = new Padding(10, 2, 10, 2) };
            UiTheme.StylePrimaryButton(b);
            b.AutoSize = true;
            b.Click += h;
            _bar.Controls.Add(b);
        }
        if (config.IsEnabled("ShowRefresh")) Add("刷新", (_, _) => LoadData());
        if (config.IsEnabled("ShowAdd") && Session.IsAdmin) Add("新增客户", (_, _) => MessageBox.Show("正式新增请到「客户」页。", "新增"));
        if (config.IsEnabled("ShowEdit") && Session.IsAdmin) Add("编辑客户", (_, _) => MessageBox.Show("正式编辑请到「客户」页。", "编辑"));
        if (config.IsEnabled("ShowDelete") && Session.IsAdmin)
        {
            var del = new Button { Text = "删除客户", AutoSize = true, MinimumSize = new Size(88, 32), Margin = new Padding(0, 4, 8, 0), Padding = new Padding(10, 2, 10, 2) };
            UiTheme.StyleDangerButton(del);
            del.AutoSize = true;
            del.Click += (_, _) => MessageBox.Show("正式删除请到「客户」页。", "删除");
            _bar.Controls.Add(del);
        }
        DataExportHelper.AppendExportButtons(_bar.Controls, () => _grid, "客户CRUD控件");
        LoadData();
    }

    private void LoadData()
    {
        var all = _svc.GetAll();
        if (Session.IsCustomer && Session.CurrentUser?.CustomerId is int cid)
            all = all.Where(c => c.Id == cid).ToList();
        if (_config.IsEnabled("HighValueOnly"))
            all = all.Where(c => c.ValueAssessment == "高净值").ToList();
        _grid.DataSource = all.Select(c => new
        {
            编号 = c.CustomerNo,
            姓名 = c.Name,
            电话 = c.Phone,
            余额 = c.Balance.ToString("N2"),
            评估 = c.ValueAssessment,
            总价值 = c.TotalValue.ToString("N2")
        }).ToList();
    }
}

/// <summary>日志查看页面控件。</summary>
public sealed class LogViewerControl : UserControl
{
    private readonly LogService _log = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly TextBox _kw = new() { Width = 160 };
    private readonly Button _search = new() { Text = "查询", AutoSize = true, MinimumSize = new Size(72, 32) };
    private WidgetConfig _config = new();

    public LogViewerControl()
    {
        Dock = DockStyle.Fill;
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, AutoScroll = true };
        bar.Controls.Add(new Label { Text = "关键词", AutoSize = true, Margin = new Padding(0, 10, 4, 0) });
        bar.Controls.Add(_kw);
        UiTheme.StylePrimaryButton(_search);
        _search.AutoSize = true;
        _search.Click += (_, _) => LoadData();
        bar.Controls.Add(_search);
        DataExportHelper.AppendExportButtons(bar.Controls, () => _grid, "日志查看控件",
            openFolderPath: Data.DatabaseHelper.LogFolder);
        Controls.Add(UiTheme.WrapGrid(_grid));
        Controls.Add(bar);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        _config = config;
        _search.Visible = config.IsEnabled("AllowSearch");
        LoadData();
    }

    private void LoadData()
    {
        var days = _config.IsEnabled("Last7Days") ? 7 : 30;
        var from = DateTime.Today.AddDays(-days);
        var list = _log.GetLogs(from, DateTime.Now, _config.IsEnabled("AllowSearch") ? _kw.Text : null);
        if (Session.IsCustomer)
            list = list.Where(l => l.Username == Session.CurrentUser?.Username).ToList();
        _grid.DataSource = list.Select(l => new
        {
            时间 = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            用户 = l.Username,
            模块 = l.Module,
            操作 = l.Action,
            详情 = l.Detail
        }).ToList();
    }
}

/// <summary>银行信息卡片控件。</summary>
public sealed class BankInfoControl : UserControl
{
    private readonly BankService _svc = new();
    private readonly Label _body = new()
    {
        Dock = DockStyle.Fill,
        Font = new Font("Microsoft YaHei UI", 10.5F),
        Padding = new Padding(12)
    };

    public BankInfoControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Controls.Add(_body);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        var b = _svc.GetBankInfo();
        if (b == null)
        {
            _body.Text = "暂无银行信息。";
            return;
        }
        var lines = new List<string> { $"银行名称：{b.BankName}", $"地址：{b.Address}", $"电话：{b.Phone}" };
        if (config.IsEnabled("ShowAssets"))
        {
            lines.Add($"总资产：{b.TotalAssets:N0}");
            lines.Add($"总存款：{b.TotalDeposits:N0}");
            lines.Add($"总贷款：{b.TotalLoans:N0}");
            lines.Add($"客户数：{b.CustomerCount}");
        }
        if (config.IsEnabled("ShowDescription"))
            lines.Add($"简介：{b.Description}");
        _body.Text = string.Join(Environment.NewLine + Environment.NewLine, lines);
    }
}

/// <summary>持仓列表面板控件。</summary>
public sealed class HoldingListControl : UserControl
{
    private readonly CustomerService _svc = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _customers = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };

    public HoldingListControl()
    {
        Dock = DockStyle.Fill;
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42 };
        bar.Controls.Add(new Label { Text = "客户", AutoSize = true, Margin = new Padding(0, 10, 6, 0) });
        bar.Controls.Add(_customers);
        var refresh = new Button { Text = "刷新", AutoSize = true, MinimumSize = new Size(72, 32), Margin = new Padding(8, 4, 0, 0) };
        UiTheme.StylePrimaryButton(refresh);
        refresh.AutoSize = true;
        refresh.Click += (_, _) => LoadHoldings();
        bar.Controls.Add(refresh);
        _customers.SelectedIndexChanged += (_, _) => LoadHoldings();
        Controls.Add(UiTheme.WrapGrid(_grid));
        Controls.Add(bar);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        _customers.Items.Clear();
        var list = _svc.GetAll();
        if (Session.IsCustomer && Session.CurrentUser?.CustomerId is int cid)
            list = list.Where(c => c.Id == cid).ToList();
        foreach (var c in list)
            _customers.Items.Add($"{c.Id}|{c.CustomerNo} {c.Name}");
        if (_customers.Items.Count > 0) _customers.SelectedIndex = 0;
        else LoadHoldings();
        _ = config;
    }

    private void LoadHoldings()
    {
        if (_customers.SelectedItem == null)
        {
            _grid.DataSource = null;
            return;
        }
        var id = int.Parse(_customers.SelectedItem.ToString()!.Split('|')[0]);
        var holdings = _svc.GetHoldings(id);
        _grid.DataSource = holdings.Select(h => new
        {
            产品 = h.ProductName,
            类别 = h.Category,
            数量 = h.Quantity.ToString("N2"),
            成本 = h.CostPrice.ToString("N2"),
            现价 = h.CurrentPrice.ToString("N2"),
            市值 = h.MarketValue.ToString("N2")
        }).ToList();
    }
}

/// <summary>主页看板控件。</summary>
public sealed class HomeDashboardControl : UserControl
{
    private readonly BankService _bank = new();
    private readonly ProductService _products = new();
    private readonly CustomerService _customers = new();
    private readonly Label _body = new() { Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei UI", 11F), Padding = new Padding(16) };

    public HomeDashboardControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Controls.Add(_body);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        var lines = new List<string> { "【首页看板】" };
        if (config.IsEnabled("ShowBank") && _bank.GetBankInfo() is { } b)
            lines.Add($"银行：{b.BankName}｜客户数 {b.CustomerCount}｜总资产 {b.TotalAssets:N0}");
        if (config.IsEnabled("ShowCustomers"))
            lines.Add($"客户合计：{_customers.GetAll().Count} 人");
        if (config.IsEnabled("ShowProducts"))
        {
            var ps = _products.GetAll(true);
            lines.Add($"启用产品：{ps.Count} 种｜盈利合计 {ps.Sum(p => p.Profit):N0}");
        }
        if (config.IsEnabled("ShowRates"))
        {
            var rates = _bank.GetRates().Take(4);
            lines.Add("利率：" + string.Join("；", rates.Select(r => $"{r.Name} {r.Rate:P2}")));
        }
        _body.Text = string.Join(Environment.NewLine + Environment.NewLine, lines);
    }
}

/// <summary>用户列表控件（管理类页面）。</summary>
public sealed class UserListControl : UserControl
{
    private readonly UserService _svc = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };

    public UserListControl()
    {
        Dock = DockStyle.Fill;
        Controls.Add(UiTheme.WrapGrid(_grid));
    }

    public void ApplyConfig(WidgetConfig config)
    {
        if (!Session.IsAdmin)
        {
            _grid.DataSource = new[] { new { 提示 = "仅管理员可查看用户列表" } }.ToList();
            return;
        }
        var users = _svc.GetAll();
        if (config.IsEnabled("AdminsOnly"))
            users = users.Where(u => u.Role != Models.UserRole.Customer).ToList();
        if (!config.IsEnabled("ShowInactive"))
            users = users.Where(u => u.IsActive).ToList();
        _grid.DataSource = users.Select(u => new
        {
            用户名 = u.Username,
            姓名 = u.DisplayName,
            角色 = u.Role.ToString(),
            启用 = u.IsActive ? "是" : "否"
        }).ToList();
    }
}

/// <summary>存取款演示面板控件。</summary>
public sealed class DepositWithdrawControl : UserControl
{
    private readonly TextBox _amount = new() { Width = 160, Text = "1000" };
    private readonly Label _tip = new() { AutoSize = true, ForeColor = UiTheme.TextMuted };

    public DepositWithdrawControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Padding = new Padding(16);
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 140, FlowDirection = FlowDirection.TopDown };
        panel.Controls.Add(new Label { Text = "金额（元）", AutoSize = true });
        panel.Controls.Add(_amount);
        panel.Controls.Add(_tip);
        Controls.Add(panel);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        // 重建，避免重复添加按钮
        Controls.Clear();
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Padding = new Padding(16);

        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 140, FlowDirection = FlowDirection.TopDown };
        panel.Controls.Add(new Label { Text = "金额（元）", AutoSize = true });
        panel.Controls.Add(_amount);
        _tip.Text = "配合底部「金融操作」按钮控件使用；正式存取款请到「客户」页。";
        panel.Controls.Add(_tip);

        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48 };
        if (config.IsEnabled("ShowDeposit"))
        {
            var d = new Button { Text = "模拟存款", AutoSize = true, MinimumSize = new Size(100, 32), Margin = new Padding(0, 4, 8, 0), Padding = new Padding(12, 2, 12, 2) };
            UiTheme.StylePrimaryButton(d);
            d.AutoSize = true;
            d.Click += (_, _) => MessageBox.Show($"模拟存款 {_amount.Text} 元（演示）。", "存款");
            bar.Controls.Add(d);
        }
        if (config.IsEnabled("ShowWithdraw"))
        {
            var w = new Button { Text = "模拟取款", AutoSize = true, MinimumSize = new Size(100, 32), Margin = new Padding(0, 4, 8, 0), Padding = new Padding(12, 2, 12, 2) };
            UiTheme.StylePrimaryButton(w);
            w.AutoSize = true;
            w.Click += (_, _) => MessageBox.Show($"模拟取款 {_amount.Text} 元（演示）。", "取款");
            bar.Controls.Add(w);
        }
        Controls.Add(panel);
        Controls.Add(bar);
        bar.BringToFront();
    }
}