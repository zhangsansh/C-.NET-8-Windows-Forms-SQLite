namespace BankManagementSystem.Controls;

/// <summary>用户控件分类：业务内容区 / 按钮工具栏。</summary>
public enum WidgetKind
{
    Content = 0,
    ButtonBar = 1
}

/// <summary>用户控件可配置功能项。</summary>
public sealed class WidgetFeature
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool DefaultEnabled { get; init; } = true;
    public override string ToString() => Label;
}

/// <summary>用户控件运行时配置。</summary>
public sealed class WidgetConfig
{
    public Dictionary<string, bool> Features { get; set; } = new();
    public string NoticeText { get; set; } = string.Empty;
    public string ExtraParam { get; set; } = string.Empty;

    public bool IsEnabled(string featureKey, bool defaultValue = true)
        => Features.TryGetValue(featureKey, out var v) ? v : defaultValue;
}

/// <summary>可嵌入自定义页面的用户控件契约（表现层插件）。</summary>
public interface IPageWidget
{
    string Key { get; }
    string DisplayName { get; }
    string Description { get; }
    WidgetKind Kind { get; }
    IReadOnlyList<WidgetFeature> Features { get; }

    UserControl Create();
    void Apply(UserControl control, WidgetConfig config);
}

/// <summary>按钮工具栏控件可向外报告动作（供自定义页统一处理）。</summary>
public interface IToolbarHost
{
    event Action<string>? ToolbarAction;
}
