namespace BankManagementSystem.Controls;

using BankManagementSystem.Utils;

internal sealed class PlaceholderControl : UserControl
{
    public PlaceholderControl(string text)
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Controls.Add(new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Microsoft YaHei UI", 11F)
        });
    }
}

/// <summary>仅占位（无业务内容）。</summary>
public sealed class ButtonsOnlyWidget : IPageWidget
{
    public string Key => "buttons_only";
    public string DisplayName => "空白内容区";
    public string Description => "不嵌入业务控件，仅配合按钮栏与自定义按钮使用。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } = Array.Empty<WidgetFeature>();
    public UserControl Create() => new PlaceholderControl("未嵌入业务用户控件，可使用下方用户按钮控件与自定义按钮。");
    public void Apply(UserControl control, WidgetConfig config) { }
}

public sealed class ProductListWidget : IPageWidget
{
    public string Key => "product_list";
    public string DisplayName => "【产品】列表控件";
    public string Description => "产品数据表格，可配置收益率/盈利/启用筛选。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowYield", Label = "显示收益率", DefaultEnabled = true },
        new() { Key = "ShowProfit", Label = "显示盈利（管理员）", DefaultEnabled = true },
        new() { Key = "ActiveOnly", Label = "仅显示启用产品", DefaultEnabled = true },
        new() { Key = "AllowRefresh", Label = "显示刷新按钮", DefaultEnabled = true }
    ];
    public UserControl Create() => new ProductListControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is ProductListControl c) c.ApplyConfig(config);
    }
}

public sealed class ProductCrudWidget : IPageWidget
{
    public string Key => "product_crud";
    public string DisplayName => "【产品】增删改查页";
    public string Description => "产品列表 + 内置增删改查按钮（演示）。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowRefresh", Label = "刷新", DefaultEnabled = true },
        new() { Key = "ShowAdd", Label = "新增", DefaultEnabled = true },
        new() { Key = "ShowEdit", Label = "编辑", DefaultEnabled = true },
        new() { Key = "ShowDelete", Label = "删除", DefaultEnabled = true },
        new() { Key = "ActiveOnly", Label = "仅启用产品", DefaultEnabled = true }
    ];
    public UserControl Create() => new ProductCrudControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is ProductCrudControl c) c.ApplyConfig(config);
    }
}

public sealed class CustomerSummaryWidget : IPageWidget
{
    public string Key => "customer_summary";
    public string DisplayName => "【客户】概览控件";
    public string Description => "客户统计与简表。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowBalanceSum", Label = "显示余额合计", DefaultEnabled = true },
        new() { Key = "ShowHighValue", Label = "显示高净值客户数", DefaultEnabled = true },
        new() { Key = "ShowList", Label = "显示客户简表", DefaultEnabled = true }
    ];
    public UserControl Create() => new CustomerSummaryControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is CustomerSummaryControl c) c.ApplyConfig(config);
    }
}

public sealed class CustomerCrudWidget : IPageWidget
{
    public string Key => "customer_crud";
    public string DisplayName => "【客户】增删改查页";
    public string Description => "客户列表 + 内置增删改查按钮（演示）。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowRefresh", Label = "刷新", DefaultEnabled = true },
        new() { Key = "ShowAdd", Label = "新增", DefaultEnabled = true },
        new() { Key = "ShowEdit", Label = "编辑", DefaultEnabled = true },
        new() { Key = "ShowDelete", Label = "删除", DefaultEnabled = true },
        new() { Key = "HighValueOnly", Label = "仅高净值", DefaultEnabled = false }
    ];
    public UserControl Create() => new CustomerCrudControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is CustomerCrudControl c) c.ApplyConfig(config);
    }
}

public sealed class RateBoardWidget : IPageWidget
{
    public string Key => "rate_board";
    public string DisplayName => "【管理】利率看板";
    public string Description => "展示银行利率表。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowDescription", Label = "显示利率说明", DefaultEnabled = true },
        new() { Key = "AllowRefresh", Label = "显示刷新按钮", DefaultEnabled = true }
    ];
    public UserControl Create() => new RateBoardControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is RateBoardControl c) c.ApplyConfig(config);
    }
}

public sealed class NoticeBoardWidget : IPageWidget
{
    public string Key => "notice_board";
    public string DisplayName => "【通用】公告板";
    public string Description => "显示公告文本。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowTitle", Label = "显示公告标题栏", DefaultEnabled = true },
        new() { Key = "WordWrap", Label = "自动换行", DefaultEnabled = true }
    ];
    public UserControl Create() => new NoticeBoardControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is NoticeBoardControl c) c.ApplyConfig(config);
    }
}

public sealed class InterestCalcWidget : IPageWidget
{
    public string Key => "interest_calc";
    public string DisplayName => "【金融】利息试算";
    public string Description => "本金与利率试算。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "UseBankRate", Label = "默认一年期银行利率", DefaultEnabled = true },
        new() { Key = "AllowCustomRate", Label = "允许自定义利率", DefaultEnabled = true }
    ];
    public UserControl Create() => new InterestCalcControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is InterestCalcControl c) c.ApplyConfig(config);
    }
}

public sealed class LogViewerWidget : IPageWidget
{
    public string Key => "log_viewer";
    public string DisplayName => "【日志】查看控件";
    public string Description => "操作日志列表与关键词查询。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "AllowSearch", Label = "允许关键词查询", DefaultEnabled = true },
        new() { Key = "Last7Days", Label = "默认近7天（否则近30天）", DefaultEnabled = true }
    ];
    public UserControl Create() => new LogViewerControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is LogViewerControl c) c.ApplyConfig(config);
    }
}

public sealed class BankInfoWidget : IPageWidget
{
    public string Key => "bank_info";
    public string DisplayName => "【管理】银行信息卡";
    public string Description => "展示银行概况卡片。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowAssets", Label = "显示资产负债统计", DefaultEnabled = true },
        new() { Key = "ShowDescription", Label = "显示银行简介", DefaultEnabled = true }
    ];
    public UserControl Create() => new BankInfoControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is BankInfoControl c) c.ApplyConfig(config);
    }
}

public sealed class HoldingListWidget : IPageWidget
{
    public string Key => "holding_list";
    public string DisplayName => "【客户】持仓列表";
    public string Description => "按客户查看持仓明细。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } = Array.Empty<WidgetFeature>();
    public UserControl Create() => new HoldingListControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is HoldingListControl c) c.ApplyConfig(config);
    }
}

public sealed class HomeDashboardWidget : IPageWidget
{
    public string Key => "home_dashboard";
    public string DisplayName => "【主页】看板控件";
    public string Description => "银行/客户/产品/利率摘要看板。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowBank", Label = "银行概况", DefaultEnabled = true },
        new() { Key = "ShowCustomers", Label = "客户合计", DefaultEnabled = true },
        new() { Key = "ShowProducts", Label = "产品盈利", DefaultEnabled = true },
        new() { Key = "ShowRates", Label = "利率摘要", DefaultEnabled = true }
    ];
    public UserControl Create() => new HomeDashboardControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is HomeDashboardControl c) c.ApplyConfig(config);
    }
}

public sealed class UserListWidget : IPageWidget
{
    public string Key => "user_list";
    public string DisplayName => "【管理】用户列表";
    public string Description => "系统用户一览（管理员）。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "AdminsOnly", Label = "仅管理员账号", DefaultEnabled = false },
        new() { Key = "ShowInactive", Label = "显示停用账号", DefaultEnabled = false }
    ];
    public UserControl Create() => new UserListControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is UserListControl c) c.ApplyConfig(config);
    }
}

public sealed class DepositWithdrawWidget : IPageWidget
{
    public string Key => "deposit_panel";
    public string DisplayName => "【金融】存取款面板";
    public string Description => "存取款演示面板。";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowDeposit", Label = "显示存款按钮", DefaultEnabled = true },
        new() { Key = "ShowWithdraw", Label = "显示取款按钮", DefaultEnabled = true }
    ];
    public UserControl Create() => new DepositWithdrawControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is DepositWithdrawControl c) c.ApplyConfig(config);
    }
}

// —— 用户按钮控件 ——

public sealed class NoneButtonBarWidget : IPageWidget
{
    public string Key => "btn_none";
    public string DisplayName => "无按钮栏";
    public string Description => "不显示预置按钮栏。";
    public WidgetKind Kind => WidgetKind.ButtonBar;
    public IReadOnlyList<WidgetFeature> Features { get; } = Array.Empty<WidgetFeature>();
    public UserControl Create() => new NoneButtonBarControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is ToolbarControlBase c) c.ApplyConfig(config);
    }
}

public sealed class CrudButtonBarWidget : IPageWidget
{
    public string Key => "btn_crud";
    public string DisplayName => "【按钮】增删改查栏";
    public string Description => "刷新/新增/编辑/删除/查看/保存。";
    public WidgetKind Kind => WidgetKind.ButtonBar;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowRefresh", Label = "刷新", DefaultEnabled = true },
        new() { Key = "ShowAdd", Label = "新增", DefaultEnabled = true },
        new() { Key = "ShowEdit", Label = "编辑", DefaultEnabled = true },
        new() { Key = "ShowDelete", Label = "删除", DefaultEnabled = true },
        new() { Key = "ShowView", Label = "查看详情", DefaultEnabled = true },
        new() { Key = "ShowSave", Label = "保存", DefaultEnabled = false }
    ];
    public UserControl Create() => new CrudButtonBarControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is ToolbarControlBase c) c.ApplyConfig(config);
    }
}

public sealed class SearchButtonBarWidget : IPageWidget
{
    public string Key => "btn_search";
    public string DisplayName => "【按钮】查询筛选栏";
    public string Description => "查询/重置/高级筛选/刷新。";
    public WidgetKind Kind => WidgetKind.ButtonBar;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowSearch", Label = "查询", DefaultEnabled = true },
        new() { Key = "ShowReset", Label = "重置", DefaultEnabled = true },
        new() { Key = "ShowAdvanced", Label = "高级筛选", DefaultEnabled = true },
        new() { Key = "ShowRefresh", Label = "刷新", DefaultEnabled = true }
    ];
    public UserControl Create() => new SearchButtonBarControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is ToolbarControlBase c) c.ApplyConfig(config);
    }
}

public sealed class ExportButtonBarWidget : IPageWidget
{
    public string Key => "btn_export";
    public string DisplayName => "【按钮】导出打印栏";
    public string Description => "导出/复制/打印/打开目录。";
    public WidgetKind Kind => WidgetKind.ButtonBar;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowExport", Label = "导出CSV", DefaultEnabled = true },
        new() { Key = "ShowExportTable", Label = "导出为表格", DefaultEnabled = true },
        new() { Key = "ShowCopy", Label = "复制摘要", DefaultEnabled = true },
        new() { Key = "ShowPrint", Label = "打印预览", DefaultEnabled = true },
        new() { Key = "ShowOpenFolder", Label = "打开导出目录", DefaultEnabled = true },
        new() { Key = "ShowOpenLogFolder", Label = "打开日志目录", DefaultEnabled = false }
    ];
    public UserControl Create() => new ExportButtonBarControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is ToolbarControlBase c) c.ApplyConfig(config);
    }
}

public sealed class FinanceButtonBarWidget : IPageWidget
{
    public string Key => "btn_finance";
    public string DisplayName => "【按钮】金融操作栏";
    public string Description => "存款/取款/转账/利息/刷新。";
    public WidgetKind Kind => WidgetKind.ButtonBar;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowDeposit", Label = "存款", DefaultEnabled = true },
        new() { Key = "ShowWithdraw", Label = "取款", DefaultEnabled = true },
        new() { Key = "ShowTransfer", Label = "转账说明", DefaultEnabled = true },
        new() { Key = "ShowInterest", Label = "利息试算", DefaultEnabled = true },
        new() { Key = "ShowRefresh", Label = "刷新", DefaultEnabled = true }
    ];
    public UserControl Create() => new FinanceButtonBarControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is ToolbarControlBase c) c.ApplyConfig(config);
    }
}

public sealed class NavButtonBarWidget : IPageWidget
{
    public string Key => "btn_nav";
    public string DisplayName => "【按钮】导航帮助栏";
    public string Description => "说明/帮助/关于/刷新。";
    public WidgetKind Kind => WidgetKind.ButtonBar;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowHome", Label = "返回说明", DefaultEnabled = true },
        new() { Key = "ShowHelp", Label = "使用帮助", DefaultEnabled = true },
        new() { Key = "ShowAbout", Label = "关于系统", DefaultEnabled = true },
        new() { Key = "ShowRefresh", Label = "刷新页面", DefaultEnabled = true }
    ];
    public UserControl Create() => new NavButtonBarControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is ToolbarControlBase c) c.ApplyConfig(config);
    }
}

public static class PageWidgetRegistry
{
    private static readonly Dictionary<string, IPageWidget> Map;

    static PageWidgetRegistry()
    {
        IPageWidget[] widgets =
        [
            new ButtonsOnlyWidget(),
            new ProductListWidget(),
            new ProductCrudWidget(),
            new CustomerSummaryWidget(),
            new CustomerCrudWidget(),
            new RateBoardWidget(),
            new NoticeBoardWidget(),
            new InterestCalcWidget(),
            new LogViewerWidget(),
            new BankInfoWidget(),
            new HoldingListWidget(),
            new HomeDashboardWidget(),
            new UserListWidget(),
            new DepositWithdrawWidget(),
            new NoneButtonBarWidget(),
            new CrudButtonBarWidget(),
            new SearchButtonBarWidget(),
            new ExportButtonBarWidget(),
            new FinanceButtonBarWidget(),
            new NavButtonBarWidget()
        ];
        Map = widgets.ToDictionary(w => w.Key, StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<IPageWidget> All => Map.Values.OrderBy(w => w.Kind).ThenBy(w => w.DisplayName).ToList();
    public static IReadOnlyList<IPageWidget> ContentWidgets =>
        Map.Values.Where(w => w.Kind == WidgetKind.Content).OrderBy(w => w.DisplayName).ToList();
    public static IReadOnlyList<IPageWidget> ButtonBarWidgets =>
        Map.Values.Where(w => w.Kind == WidgetKind.ButtonBar).OrderBy(w => w.DisplayName).ToList();

    public static IPageWidget Get(string? key)
        => Map.TryGetValue(key ?? "buttons_only", out var w) ? w : Map["buttons_only"];

    public static IPageWidget GetButtonBar(string? key)
        => Map.TryGetValue(key ?? "btn_none", out var w) && w.Kind == WidgetKind.ButtonBar
            ? w
            : Map["btn_none"];

    public static WidgetConfig ParseConfig(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new WidgetConfig();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<WidgetConfig>(json) ?? new WidgetConfig();
        }
        catch { return new WidgetConfig(); }
    }

    public static string SerializeConfig(WidgetConfig config)
        => System.Text.Json.JsonSerializer.Serialize(config);
}
