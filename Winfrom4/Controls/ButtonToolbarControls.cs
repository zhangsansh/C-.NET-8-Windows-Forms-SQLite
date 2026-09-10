namespace BankManagementSystem.Controls;

using BankManagementSystem.Services;
using BankManagementSystem.Utils;

/// <summary>可配置的按钮工具栏基类。</summary>
public abstract class ToolbarControlBase : UserControl, IToolbarHost
{
    protected readonly FlowLayoutPanel Bar = new()
    {
        Dock = DockStyle.Fill,
        WrapContents = true,
        AutoScroll = true,
        Padding = new Padding(4)
    };
    protected WidgetConfig Config = new();
    public event Action<string>? ToolbarAction;

    protected ToolbarControlBase()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(250, 252, 255);
        Height = 48;
        Controls.Add(Bar);
    }

    public virtual void ApplyConfig(WidgetConfig config)
    {
        Config = config;
        Bar.Controls.Clear();
        BuildButtons();
    }

    protected abstract void BuildButtons();

    protected Button AddBtn(string text, string action, bool danger = false, int minWidth = 88)
    {
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(minWidth, 32),
            Margin = new Padding(0, 4, 8, 4),
            Padding = new Padding(10, 2, 10, 2)
        };
        if (danger) UiTheme.StyleDangerButton(b);
        else UiTheme.StylePrimaryButton(b);
        b.AutoSize = true;
        b.MinimumSize = new Size(minWidth, 32);
        b.Padding = new Padding(10, 2, 10, 2);
        var act = action;
        b.Click += (_, _) => ToolbarAction?.Invoke(act);
        Bar.Controls.Add(b);
        return b;
    }
}

/// <summary>增删改查按钮控件。</summary>
public sealed class CrudButtonBarControl : ToolbarControlBase
{
    protected override void BuildButtons()
    {
        if (Config.IsEnabled("ShowRefresh")) AddBtn("刷新", "Refresh");
        if (Config.IsEnabled("ShowAdd") && Session.IsAdmin) AddBtn("新增", "Add");
        if (Config.IsEnabled("ShowEdit") && Session.IsAdmin) AddBtn("编辑", "Edit");
        if (Config.IsEnabled("ShowDelete") && Session.IsAdmin) AddBtn("删除", "Delete", danger: true);
        if (Config.IsEnabled("ShowView")) AddBtn("查看详情", "View", minWidth: 100);
        if (Config.IsEnabled("ShowSave") && Session.IsAdmin) AddBtn("保存", "Save");
    }
}

/// <summary>查询筛选按钮控件。</summary>
public sealed class SearchButtonBarControl : ToolbarControlBase
{
    protected override void BuildButtons()
    {
        if (Config.IsEnabled("ShowSearch")) AddBtn("查询", "Search");
        if (Config.IsEnabled("ShowReset")) AddBtn("重置", "Reset");
        if (Config.IsEnabled("ShowAdvanced")) AddBtn("高级筛选", "AdvancedSearch", minWidth: 100);
        if (Config.IsEnabled("ShowRefresh")) AddBtn("刷新", "Refresh");
    }
}

/// <summary>导出打印按钮控件。</summary>
public sealed class ExportButtonBarControl : ToolbarControlBase
{
    protected override void BuildButtons()
    {
        if (Config.IsEnabled("ShowExport")) AddBtn("导出CSV", "ExportCsv", minWidth: 100);
        if (Config.IsEnabled("ShowExportTable")) AddBtn("导出为表格", "ExportTable", minWidth: 110);
        if (Config.IsEnabled("ShowCopy")) AddBtn("复制摘要", "CopySummary", minWidth: 100);
        if (Config.IsEnabled("ShowPrint")) AddBtn("打印预览", "Print", minWidth: 100);
        if (Config.IsEnabled("ShowOpenFolder")) AddBtn("打开目录", "OpenExportFolder", minWidth: 100);
        if (Config.IsEnabled("ShowOpenLogFolder")) AddBtn("打开日志目录", "OpenLogFolder", minWidth: 120);
    }
}

/// <summary>金融操作按钮控件。</summary>
public sealed class FinanceButtonBarControl : ToolbarControlBase
{
    protected override void BuildButtons()
    {
        if (Config.IsEnabled("ShowDeposit")) AddBtn("存款", "Deposit");
        if (Config.IsEnabled("ShowWithdraw")) AddBtn("取款", "Withdraw");
        if (Config.IsEnabled("ShowTransfer")) AddBtn("转账说明", "Transfer", minWidth: 100);
        if (Config.IsEnabled("ShowInterest")) AddBtn("利息试算", "InterestCalc", minWidth: 100);
        if (Config.IsEnabled("ShowRefresh")) AddBtn("刷新", "Refresh");
    }
}

/// <summary>导航帮助按钮控件。</summary>
public sealed class NavButtonBarControl : ToolbarControlBase
{
    protected override void BuildButtons()
    {
        if (Config.IsEnabled("ShowHome")) AddBtn("返回说明", "HomeHelp", minWidth: 100);
        if (Config.IsEnabled("ShowHelp")) AddBtn("使用帮助", "Help", minWidth: 100);
        if (Config.IsEnabled("ShowAbout")) AddBtn("关于系统", "About", minWidth: 100);
        if (Config.IsEnabled("ShowRefresh")) AddBtn("刷新页面", "Refresh", minWidth: 100);
    }
}

/// <summary>空按钮栏。</summary>
public sealed class NoneButtonBarControl : ToolbarControlBase
{
    protected override void BuildButtons()
    {
        Bar.Controls.Add(new Label
        {
            Text = "未选择用户按钮控件（可在新建/编辑页选择增删改查等按钮栏）",
            AutoSize = true,
            ForeColor = UiTheme.TextMuted,
            Margin = new Padding(4, 10, 0, 0)
        });
    }
}
