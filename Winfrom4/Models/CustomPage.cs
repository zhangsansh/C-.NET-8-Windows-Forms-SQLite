namespace BankManagementSystem.Models;

public class CustomPage
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PageKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>JSON: [{ "Text":"按钮", "Action":"Message|Refresh|OpenUrl|Custom", "Param":"..." }]</summary>
    public string ButtonsJson { get; set; } = "[]";
    /// <summary>嵌入的业务用户控件键，对应 PageWidgetRegistry（Content）。</summary>
    public string ControlKey { get; set; } = "buttons_only";
    /// <summary>业务用户控件功能配置 JSON。</summary>
    public string ControlConfigJson { get; set; } = "{}";
    /// <summary>嵌入的用户按钮控件键（ButtonBar）。</summary>
    public string ButtonBarKey { get; set; } = "btn_none";
    /// <summary>用户按钮控件功能配置 JSON。</summary>
    public string ButtonBarConfigJson { get; set; } = "{}";
    public bool VisibleToAdmin { get; set; } = true;
    public bool VisibleToCustomer { get; set; }
    public bool VisibleToSuperAdmin { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PageButtonDef
{
    public string Text { get; set; } = string.Empty;
    public string Action { get; set; } = "Message";
    public string Param { get; set; } = string.Empty;
}
