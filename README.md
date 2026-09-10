# 华夏示范银行管理系统 — 帮助文档（README）

> 基于 **C# / .NET 8 + Windows Forms + SQLite** 的桌面银行业务演示系统。  
> 分层架构：`Forms / Pages` → `Controls`（内容用户控件 + 用户按钮控件）→ `Services` → `Data / Models / Utils`。  
> 支持超级管理员 / 管理员 / 客户；内嵌登录、顶部图标导航、客户与产品管理、持仓价值、操作日志、空闲自动登出、搜索与导出；  
> **页面扩展**可分别选择「内容用户控件」与「用户按钮控件」，并独立配置功能开关。

---

## 目录

1. [项目简介](#1-项目简介)
2. [使用技术](#2-使用技术)
3. [环境要求与运行方法](#3-环境要求与运行方法)
4. [演示账号](#4-演示账号)
5. [功能说明与使用方法](#5-功能说明与使用方法)
6. [全部用户控件详解（介绍 / 使用方式 / 代码）](#6-全部用户控件详解介绍--使用方式--代码)
7. [项目结构](#7-项目结构)
8. [代码书写全过程与步骤](#8-代码书写全过程与步骤)
9. [数据库设计](#9-数据库设计)
10. [核心业务说明](#10-核心业务说明)
11. [常见问题](#11-常见问题)

---

## 1. 项目简介

| 模块        | 说明                                                         |
| ----------- | ------------------------------------------------------------ |
| 登录 / 账号 | 登录嵌在主窗体；可设置无操作超时自动登出；可配置导出目录     |
| 主页        | 银行概况、利率、产品盈利（客户见本人资产）                   |
| 客户 / 账户 | 约 100 名客户；搜索；详情换行；持仓；存取款与总账号价值；导出/打印 |
| 产品        | PROD001～PROD100；搜索；增删改查；导出/打印                  |
| 系统管理    | 银行信息、利率、用户                                         |
| 操作日志    | SQLite + 按日文件双写；关键词与日期筛选；导出                |
| 页面扩展    | 新建自定义导航页，嵌入**内容控件 + 按钮控件**并配置功能      |

**数据与文件位置：**

| 类型           | 路径                                                         |
| -------------- | ------------------------------------------------------------ |
| 数据库         | `%LocalAppData%\BankManagementSystem\bank.db`                |
| 日志           | `%LocalAppData%\BankManagementSystem\Logs\log_yyyyMMdd.txt`（可在设置中改） |
| 导出（默认）   | **程序运行目录下的 `Exports`**（如 `bin\Debug\net8.0-windows\Exports`） |
| 导出（自定义） | 账号页「导出目录设置」保存后的路径                           |

---

## 2. 使用技术

| 技术                            | 用途                                                         |
| ------------------------------- | ------------------------------------------------------------ |
| C# / .NET 8（`net8.0-windows`） | 主语言与运行时                                               |
| Windows Forms                   | `Form`、`UserControl`、`DataGridView`、`TableLayoutPanel`、`ComboBox`、`CheckedListBox` 等 |
| Microsoft.Data.Sqlite           | SQLite 数据访问                                              |
| System.Text.Json                | 按钮 JSON、控件配置 JSON                                     |
| Segoe MDL2 Assets               | 顶部导航字形（`Paint` 绘制）                                 |
| IMessageFilter                  | 键鼠活动 → 重置空闲计时                                      |
| Timer                           | 状态栏时钟与空闲检测                                         |
| System.Drawing.Printing         | 打印预览                                                     |

### 分层架构

```
Forms（主窗体） / Pages（业务页、扩展编辑器）
        ↓
Controls（IPageWidget 注册表 + 内容控件 + 按钮栏控件）
        ↓
Services（登录、客户、产品、银行、用户、自定义页、日志）
        ↓
Data（DatabaseHelper） / Models / Utils（Session、UiTheme、DataExportHelper）
```

自定义页字段：

| 字段                                   | 含义                                               |
| -------------------------------------- | -------------------------------------------------- |
| `ControlKey` / `ControlConfigJson`     | 内容用户控件及其功能                               |
| `ButtonBarKey` / `ButtonBarConfigJson` | 用户按钮控件及其功能                               |
| `ButtonsJson`                          | 额外自由定义按钮（Message / Refresh / OpenUrl 等） |

---

## 3. 环境要求与运行方法

### 3.1 环境

- Windows 10 / 11  
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)  
- 可选：Visual Studio 2022 / Rider / VS Code + C#

### 3.2 命令行

```bash
cd "c:\Users\zjl15\Desktop\C#\Winfrom4"
dotnet restore
dotnet build
dotnet run --project BankManagementSystem.csproj
```

> 请勿对 `_diag` 诊断工程执行 `dotnet run`（项目已 `Compile Remove="_diag/**"`）。

### 3.3 Visual Studio

打开 `BankManagementSystem.csproj` → 确认目标框架为 `net8.0-windows` → **F5** 运行。

### 3.4 项目文件要点

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.11" />
  </ItemGroup>
</Project>
```

---

## 4. 演示账号

| 角色       | 用户名                                                       | 密码       | 说明                     |
| ---------- | ------------------------------------------------------------ | ---------- | ------------------------ |
| 超级管理员 | `superadmin` / `superadmin2` / `superadmin3`                 | `admin123` | 全部功能 + 页面扩展      |
| 管理员     | `admin`～`admin8`、`manager1`～`3`、`auditor`、`teller1`/`2` | `admin123` | 管理业务与日志           |
| 客户       | `zhangsan` / `lisi` / `wangwu`                               | `123456`   | 本人账户、产品浏览、日志 |
| 客户       | `user004`～`user100`                                         | `123456`   | 其余演示客户             |

角色枚举：`Customer = 0`，`Admin = 1`，`SuperAdmin = 2`。

---

## 5. 功能说明与使用方法

### 5.1 登录与账号页

1. 启动后进入嵌入式登录页，输入用户名/密码登录。  
2. 登录后顶部导航切换业务页；右上可进入「账号」查看资料。  
3. **空闲超时**：账号页设置秒数（默认 300），无键鼠操作超时自动登出。  
4. **导出目录**：浏览 / 保存 / 打开 / **恢复默认**（默认 = 程序运行目录 `\Exports`）。

### 5.2 主页

- 管理员：银行概况、客户合计、产品盈利、利率摘要。  
- 客户：本人余额、持仓与资产相关摘要。  
- 工具栏可导出/复制/打印相关汇总表（若页内含表格）。

### 5.3 客户页

- **搜索**：按姓名、客户号、账号、电话等过滤；Enter 搜索；「重置」清空。  
- 管理员：新增 / 编辑 / 删除客户。  
- 存取款：更新余额并记日志；详情区换行展示；下方持仓表。  
- **导出 CSV / 导出为表格 / 复制摘要 / 打印预览 / 打开目录**。

### 5.4 产品页

- **搜索**产品代码、名称、类别等。  
- 管理员增删改查；客户只读浏览。  
- 同样支持导出与打印工具栏。

### 5.5 系统管理

- 银行信息、利率表、系统用户列表（管理员）。

### 5.6 操作日志

- 关键词 + 日期范围查询；重置；导出。  
- 日志同时写入 SQLite `OperationLogs` 与按日文本文件。

### 5.7 页面扩展（管理员 / 超管）

1. 打开「页面扩展」→ **新建**或**编辑**。  
2. 填写标题、PageKey、说明、可见角色、排序。  
3. 选择 **内容用户控件** + 勾选功能；选择 **用户按钮控件** + 勾选功能。  
4. （可选）添加自定义按钮：`Message` / `Refresh` / `OpenUrl` / `Custom`。  
5. 保存后顶部导航出现新页；打开即运行 `DynamicPageView` 组合加载控件。

**推荐组合示例：**

| 场景          | 内容控件        | 按钮栏        |
| ------------- | --------------- | ------------- |
| 产品浏览+导出 | `product_list`  | `btn_export`  |
| 客户维护演示  | `customer_crud` | `btn_crud`    |
| 利率+帮助     | `rate_board`    | `btn_nav`     |
| 仅公告        | `notice_board`  | `btn_none`    |
| 存取款演示    | `deposit_panel` | `btn_finance` |

---

## 6. 全部用户控件详解（介绍 / 使用方式 / 代码）

控件通过 `IPageWidget` + `PageWidgetRegistry` 注册。自定义页只存 **Key** 与 **Config JSON**，运行时 `Create()` + `Apply()`。

### 6.1 契约与注册表

**接口（`Controls/IPageWidget.cs`）：**

```csharp
public enum WidgetKind { Content = 0, ButtonBar = 1 }

public sealed class WidgetFeature
{
    public string Key { get; init; } = "";
    public string Label { get; init; } = "";
    public bool DefaultEnabled { get; init; } = true;
}

public sealed class WidgetConfig
{
    public Dictionary<string, bool> Features { get; set; } = new();
    public string NoticeText { get; set; } = "";
    public string ExtraParam { get; set; } = "";
    public bool IsEnabled(string featureKey, bool defaultValue = true)
        => Features.TryGetValue(featureKey, out var v) ? v : defaultValue;
}

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

public interface IToolbarHost
{
    event Action<string>? ToolbarAction;
}
```

**注册与解析（节选）：**

```csharp
public static class PageWidgetRegistry
{
    public static IPageWidget Get(string? key)
        => Map.TryGetValue(key ?? "buttons_only", out var w) ? w : Map["buttons_only"];

    public static IPageWidget GetButtonBar(string? key)
        => Map.TryGetValue(key ?? "btn_none", out var w) && w.Kind == WidgetKind.ButtonBar
            ? w : Map["btn_none"];

    public static WidgetConfig ParseConfig(string? json) { /* JsonSerializer */ }
    public static string SerializeConfig(WidgetConfig config) => JsonSerializer.Serialize(config);
}
```

**页面中嵌入用法：**

```csharp
var contentWidget = PageWidgetRegistry.Get(page.ControlKey);
var control = contentWidget.Create();
contentWidget.Apply(control, PageWidgetRegistry.ParseConfig(page.ControlConfigJson));

var barWidget = PageWidgetRegistry.GetButtonBar(page.ButtonBarKey);
var bar = barWidget.Create();
barWidget.Apply(bar, PageWidgetRegistry.ParseConfig(page.ButtonBarConfigJson));
if (bar is IToolbarHost toolbar)
    toolbar.ToolbarAction += OnToolbarAction;
```

**配置 JSON 示例：**

```json
{
  "Features": { "ShowYield": true, "ActiveOnly": true, "AllowRefresh": false },
  "NoticeText": "本周理财产品促销",
  "ExtraParam": ""
}
```

---

### 6.2 内容用户控件一览

| Key                | 显示名             | UserControl              | 可配置功能                                          |
| ------------------ | ------------------ | ------------------------ | --------------------------------------------------- |
| `buttons_only`     | 空白内容区         | `PlaceholderControl`     | 无                                                  |
| `product_list`     | 【产品】列表控件   | `ProductListControl`     | ShowYield / ShowProfit / ActiveOnly / AllowRefresh  |
| `product_crud`     | 【产品】增删改查页 | `ProductCrudControl`     | ShowRefresh/Add/Edit/Delete / ActiveOnly            |
| `customer_summary` | 【客户】概览控件   | `CustomerSummaryControl` | ShowBalanceSum / ShowHighValue / ShowList           |
| `customer_crud`    | 【客户】增删改查页 | `CustomerCrudControl`    | ShowRefresh/Add/Edit/Delete / HighValueOnly         |
| `rate_board`       | 【管理】利率看板   | `RateBoardControl`       | ShowDescription / AllowRefresh                      |
| `notice_board`     | 【通用】公告板     | `NoticeBoardControl`     | ShowTitle / WordWrap（文案用 NoticeText）           |
| `interest_calc`    | 【金融】利息试算   | `InterestCalcControl`    | UseBankRate / AllowCustomRate                       |
| `log_viewer`       | 【日志】查看控件   | `LogViewerControl`       | AllowSearch / Last7Days                             |
| `bank_info`        | 【管理】银行信息卡 | `BankInfoControl`        | ShowAssets / ShowDescription                        |
| `holding_list`     | 【客户】持仓列表   | `HoldingListControl`     | 无                                                  |
| `home_dashboard`   | 【主页】看板控件   | `HomeDashboardControl`   | ShowBank / ShowCustomers / ShowProducts / ShowRates |
| `user_list`        | 【管理】用户列表   | `UserListControl`        | AdminsOnly / ShowInactive                           |
| `deposit_panel`    | 【金融】存取款面板 | `DepositWithdrawControl` | ShowDeposit / ShowWithdraw                          |

#### 6.2.1 `buttons_only` — 空白内容区

- **介绍**：不加载业务表格，只配合按钮栏与自定义按钮。  
- **使用**：新建页时选「空白内容区」。

```csharp
public sealed class ButtonsOnlyWidget : IPageWidget
{
    public string Key => "buttons_only";
    public string DisplayName => "空白内容区";
    public WidgetKind Kind => WidgetKind.Content;
    public UserControl Create() => new PlaceholderControl("未嵌入业务用户控件…");
    public void Apply(UserControl control, WidgetConfig config) { }
}
```

#### 6.2.2 `product_list` — 产品列表

- **介绍**：产品 DataGridView；可隐藏收益率/盈利列；仅启用产品；带刷新与导出按钮。  
- **使用**：扩展页选「【产品】列表控件」，勾选所需功能。

```csharp
public sealed class ProductListControl : UserControl
{
    private readonly ProductService _svc = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private WidgetConfig _config = new();

    public ProductListControl()
    {
        Dock = DockStyle.Fill;
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44 };
        var refresh = new Button { Text = "刷新产品" };
        refresh.Click += (_, _) => LoadData();
        bar.Controls.Add(refresh);
        DataExportHelper.AppendExportButtons(bar.Controls, () => _grid, "产品列表控件");
        Controls.Add(UiTheme.WrapGrid(_grid));
        Controls.Add(bar);
    }

    public void ApplyConfig(WidgetConfig config)
    {
        _config = config;
        LoadData(); // 按 ShowYield / ShowProfit / ActiveOnly 绑定列
    }
}
```

对应 Widget：

```csharp
public sealed class ProductListWidget : IPageWidget
{
    public string Key => "product_list";
    public UserControl Create() => new ProductListControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is ProductListControl c) c.ApplyConfig(config);
    }
}
```

#### 6.2.3 `product_crud` / `customer_crud`

- **介绍**：内置增删改查工具条的演示页（管理员可见写操作）。  
- **使用**：需要「内容区自带 CRUD」时选用；也可外加 `btn_crud` 形成双栏按钮。

```csharp
// ProductCrudWidget Features 示例
new() { Key = "ShowAdd", Label = "新增", DefaultEnabled = true },
new() { Key = "ShowEdit", Label = "编辑", DefaultEnabled = true },
new() { Key = "ShowDelete", Label = "删除", DefaultEnabled = true },
```

#### 6.2.4 `customer_summary` — 客户概览

- **介绍**：统计标签 + 客户简表（余额合计、高净值人数等可开关）。  
- **使用**：仪表盘类自定义页。

#### 6.2.5 `rate_board` — 利率看板

- **介绍**：读取 `InterestRates` 展示；可显示说明与刷新。

#### 6.2.6 `notice_board` — 公告板

- **介绍**：展示 `WidgetConfig.NoticeText`；可开关标题栏与换行。  
- **使用**：在编辑页「公告文本」框填写内容后保存。

```csharp
public void ApplyConfig(WidgetConfig config)
{
    // ShowTitle / WordWrap；正文 = config.NoticeText
}
```

#### 6.2.7 `interest_calc` — 利息试算

- **介绍**：本金 × 利率 × 期限试算；可默认银行一年期利率或允许自定义利率。

#### 6.2.8 `log_viewer` — 日志查看

- **介绍**：查询 `OperationLogs`；可开关关键词框；默认近 7 天或近 30 天。

#### 6.2.9 `bank_info` — 银行信息卡

- **介绍**：展示 `BankInfo`；可开关资产负债与简介。

#### 6.2.10 `holding_list` — 持仓列表

- **介绍**：按当前会话权限展示持仓明细（客户看本人，管理员可看汇总/选择）。

#### 6.2.11 `home_dashboard` — 主页看板

- **介绍**：四块摘要（银行/客户/产品/利率）可单独开关，适合做成「迷你主页」扩展页。

#### 6.2.12 `user_list` — 用户列表

- **介绍**：系统用户一览；可仅管理员、是否显示停用账号。

#### 6.2.13 `deposit_panel` — 存取款面板

- **介绍**：演示存取款操作面板；可单独隐藏存款或取款按钮。

---

### 6.3 用户按钮控件一览

| Key           | 显示名             | UserControl               | 可配置功能 / 动作名                                          |
| ------------- | ------------------ | ------------------------- | ------------------------------------------------------------ |
| `btn_none`    | 无按钮栏           | `NoneButtonBarControl`    | 无（提示文案）                                               |
| `btn_crud`    | 【按钮】增删改查栏 | `CrudButtonBarControl`    | Refresh/Add/Edit/Delete/View/Save                            |
| `btn_search`  | 【按钮】查询筛选栏 | `SearchButtonBarControl`  | Search/Reset/AdvancedSearch/Refresh                          |
| `btn_export`  | 【按钮】导出打印栏 | `ExportButtonBarControl`  | ExportCsv/ExportTable/CopySummary/Print/OpenExportFolder/OpenLogFolder |
| `btn_finance` | 【按钮】金融操作栏 | `FinanceButtonBarControl` | Deposit/Withdraw/Transfer/InterestCalc/Refresh               |
| `btn_nav`     | 【按钮】导航帮助栏 | `NavButtonBarControl`     | HomeHelp/Help/About/Refresh                                  |

#### 6.3.1 按钮栏基类

```csharp
public abstract class ToolbarControlBase : UserControl, IToolbarHost
{
    protected readonly FlowLayoutPanel Bar = new() { Dock = DockStyle.Fill, WrapContents = true };
    public event Action<string>? ToolbarAction;

    protected Button AddBtn(string text, string action, bool danger = false, int minWidth = 88)
    {
        var b = new Button { Text = text, AutoSize = true, MinimumSize = new Size(minWidth, 32) };
        UiTheme.StylePrimaryButton(b);
        b.Click += (_, _) => ToolbarAction?.Invoke(action);
        Bar.Controls.Add(b);
        return b;
    }

    public virtual void ApplyConfig(WidgetConfig config)
    {
        Config = config;
        Bar.Controls.Clear();
        BuildButtons();
    }

    protected abstract void BuildButtons();
}
```

#### 6.3.2 `btn_crud` 示例

```csharp
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
```

#### 6.3.3 `btn_export` 与真实导出

`DynamicPageView` 收到 `ExportCsv` / `ExportTable` 等动作后，从内容区查找 `DataGridView` 并调用 `DataExportHelper`（非演示弹窗）。

```csharp
// DataExportHelper 默认目录 = 程序运行目录\Exports
public static string DefaultExportFolder
{
    get
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "Exports");
        Directory.CreateDirectory(dir);
        return dir;
    }
}
```

#### 6.3.4 如何新增一个用户控件（开发步骤）

1. 在 `WidgetControls.cs` 或 `ButtonToolbarControls.cs` 写 `UserControl`，实现 `ApplyConfig`。  
2. 在 `PageWidgetRegistry.cs` 增加 `IPageWidget` 实现（Key、Features、Create、Apply）。  
3. 把实例加入 `PageWidgetRegistry` 静态构造函数数组。  
4. 重新运行后，页面扩展下拉框自动出现新控件。

```csharp
public sealed class MyWidget : IPageWidget
{
    public string Key => "my_widget";
    public string DisplayName => "【自定义】我的控件";
    public string Description => "说明文字";
    public WidgetKind Kind => WidgetKind.Content;
    public IReadOnlyList<WidgetFeature> Features { get; } =
    [
        new() { Key = "ShowX", Label = "显示 X", DefaultEnabled = true }
    ];
    public UserControl Create() => new MyControl();
    public void Apply(UserControl control, WidgetConfig config)
    {
        if (control is MyControl c) c.ApplyConfig(config);
    }
}
// 注册：new MyWidget(),
```

---

## 7. 项目结构

```
Winfrom4/
├── BankManagementSystem.csproj
├── Program.cs                 # 入口：初始化 DB + MainForm
├── Forms/
│   ├── MainForm.cs            # 顶栏导航、状态栏、空闲登出、页面切换
│   └── LoginForm.cs           #（如保留）独立登录窗体备用
├── Pages/
│   ├── LoginPage.cs           # 登录 + 账号设置（超时/导出目录）
│   ├── HomePage.cs
│   ├── CustomerPage.cs        # 搜索 + CRUD + 存取款 + 导出
│   ├── ProductPage.cs         # 搜索 + CRUD + 导出
│   ├── ManagementPage.cs
│   ├── LogPage.cs
│   └── DynamicPage.cs         # DynamicPageView + PageManager + 编辑对话框
├── Controls/
│   ├── IPageWidget.cs
│   ├── PageWidgetRegistry.cs
│   ├── WidgetControls.cs      # 内容 UserControl
│   └── ButtonToolbarControls.cs
├── Services/                  # Auth / Customer / Product / Bank / CustomPage / Log …
├── Models/                    # User, Customer, Product, Holding, CustomPage …
├── Data/
│   └── DatabaseHelper.cs      # 建库、升级、种子数据
└── Utils/
    ├── Session.cs
    ├── UiTheme.cs
    ├── ActivityMessageFilter.cs
    └── DataExportHelper.cs    # CSV / xls / 复制 / 打印 / 打开目录
```

---

## 8. 代码书写全过程与步骤

以下按**从零搭建到功能完备**的推荐顺序说明；每步给出关键代码骨架（与仓库实现一致）。

### 步骤 1：创建 WinForms 项目并引用 SQLite

```bash
dotnet new winforms -n BankManagementSystem -f net8.0
dotnet add package Microsoft.Data.Sqlite
```

设置 `UseWindowsForms`、`net8.0-windows`（见第 3.4 节）。

---

### 步骤 2：定义 Models

```csharp
// Models/User.cs
public enum UserRole { Customer = 0, Admin = 1, SuperAdmin = 2 }

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public UserRole Role { get; set; }
    public string DisplayName { get; set; } = "";
    public int? CustomerId { get; set; }
    public bool IsActive { get; set; } = true;
}

// Models/CustomPage.cs（扩展页核心字段）
public class CustomPage
{
    public string Title { get; set; } = "";
    public string PageKey { get; set; } = "";
    public string ControlKey { get; set; } = "buttons_only";
    public string ControlConfigJson { get; set; } = "{}";
    public string ButtonBarKey { get; set; } = "btn_none";
    public string ButtonBarConfigJson { get; set; } = "{}";
    public string ButtonsJson { get; set; } = "[]";
    public bool VisibleToAdmin { get; set; } = true;
    public bool VisibleToCustomer { get; set; }
    public bool VisibleToSuperAdmin { get; set; } = true;
}
```

同步建立 `Customer`、`Product`、`Holding`、`InterestRate`、`BankInfo`、`OperationLog` 等实体。

---

### 步骤 3：DatabaseHelper — 建库、升级、种子

```csharp
public static class DatabaseHelper
{
    private static readonly string DbFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BankManagementSystem");
    public static string DbPath => Path.Combine(DbFolder, "bank.db");

    public static void Initialize()
    {
        Directory.CreateDirectory(DbFolder);
        using var conn = CreateConnection();
        conn.Open();
        // CREATE TABLE IF NOT EXISTS Users / Customers / Products / Holdings /
        // InterestRates / BankInfo / OperationLogs / CustomPages / AppSettings
        EnsureSchemaUpgrades(conn); // 为旧库 ADD COLUMN ControlKey 等
        SeedIfEmpty(conn);          // 银行信息、管理员、100 客户、100 产品、利率
        EnsureLargeDemoData(conn);  // 旧库补齐演示数据
    }
}
```

种子管理员示例：

```csharp
("superadmin", "admin123", 2, "超级管理员"),
("admin", "admin123", 1, "系统管理员"),
// 客户：zhangsan / 123456；user004～user100 / 123456
```

---

### 步骤 4：Session 与 UiTheme

```csharp
public static class Session
{
    public static User? CurrentUser { get; private set; }
    public static bool IsLoggedIn => CurrentUser != null;
    public static bool IsAdmin => CurrentUser?.Role is UserRole.Admin or UserRole.SuperAdmin;
    public static bool IsCustomer => CurrentUser?.Role == UserRole.Customer;
    public static int IdleTimeoutSeconds { get; set; } = 300;
    public static DateTime LastActivityUtc { get; set; } = DateTime.UtcNow;

    public static void Login(User user) { CurrentUser = user; Touch(); }
    public static void Logout() { CurrentUser = null; }
    public static void Touch() => LastActivityUtc = DateTime.UtcNow;
    public static bool IsIdleTimedOut =>
        IsLoggedIn && (DateTime.UtcNow - LastActivityUtc).TotalSeconds >= IdleTimeoutSeconds;
}
```

`UiTheme`：统一主色、危险按钮、表格包装 `WrapGrid` 等。

---

### 步骤 5：Services 层

```csharp
public class AuthService
{
    public User? Login(string username, string password)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        // SELECT ... WHERE Username=$u AND Password=$p AND IsActive=1
        // 成功则返回 User
    }
}

public class LogService
{
    public void Write(string action, string module, string detail) { /* DB + 文件 */ }
    public List<OperationLog> GetLogs(DateTime from, DateTime to, string? keyword) { ... }
}

// CustomerService / ProductService / BankService / CustomPageService 同理
```

---

### 步骤 6：Program 入口

```csharp
[STAThread]
static void Main()
{
    ApplicationConfiguration.Initialize();
    try { DatabaseHelper.Initialize(); }
    catch (Exception ex)
    {
        MessageBox.Show("数据库初始化失败：\n" + ex.Message);
        return;
    }
    Application.Run(new MainForm());
}
```

---

### 步骤 7：MainForm — 导航与空闲登出

```csharp
public class MainForm : Form
{
    private readonly Panel _content = new() { Dock = DockStyle.Fill };
    private readonly FlowLayoutPanel _nav = new() { Dock = DockStyle.Top, Height = 72 };

    public MainForm()
    {
        // 顶部品牌 + 导航 + 退出登录
        // Application.AddMessageFilter(new ActivityMessageFilter(...)) 重置空闲
        // Timer：刷新状态栏时钟 + CheckIdleTimeout()
        Navigate("login");
    }

    private void RebuildNav()
    {
        _nav.Controls.Clear();
        if (!Session.IsLoggedIn) return;
        AddNav("home", "主页", () => new HomePage());
        if (Session.IsAdmin)
        {
            AddNav("customer", "客户", () => new CustomerPage());
            AddNav("product", "产品", () => new ProductPage());
            AddNav("mgmt", "管理", () => new ManagementPage());
            AddNav("log", "日志", () => new LogPage());
            AddNav("pages", "页面扩展", () => new PageManagerPage());
        }
        else
        {
            AddNav("product", "产品", () => new ProductPage());
            // 客户可见页…
        }
        // 加载 CustomPages → AddNav("custom:"+key, title, () => new DynamicPageView(p))
        AddNav("account", "账号", () => new LoginPage(/* profile mode */));
    }

    private void Navigate(string key)
    {
        _content.Controls.Clear();
        UserControl page = key switch
        {
            "login" => new LoginPage(),
            "home" => new HomePage(),
            // ...
            _ => ResolveCustom(key)
        };
        page.Dock = DockStyle.Fill;
        _content.Controls.Add(page);
    }
}
```

---

### 步骤 8：业务页（以客户页为例）

```csharp
public class CustomerPage : UserControl
{
    private readonly TextBox _searchBox = new() { Width = 180 };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };

    private void BuildUi()
    {
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 92, WrapContents = true };
        toolbar.Controls.Add(new Label { Text = "搜索" });
        toolbar.Controls.Add(_searchBox);
        toolbar.Controls.Add(MakeBtn("搜索", (_, _) => ApplyFilter()));
        toolbar.Controls.Add(MakeBtn("重置", (_, _) => { _searchBox.Clear(); ApplyFilter(); }));
        _searchBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { ApplyFilter(); e.SuppressKeyPress = true; }
        };
        // 刷新 / 新增 / 编辑 / 删除 / 存取款 …
        DataExportHelper.AppendExportButtons(toolbar.Controls, () => _grid, "客户列表");
        // 下方：客户表 + 详情 + 持仓表
    }

    private void ApplyFilter()
    {
        var kw = _searchBox.Text.Trim();
        // 按 Name / CustomerNo / AccountNo / Phone 过滤后绑定 _grid
    }
}
```

产品页、日志页按同样模式增加搜索与 `AppendExportButtons`。

---

### 步骤 9：导出工具类

```csharp
public static class DataExportHelper
{
    public static string AppRunFolder =>
        AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    public static string DefaultExportFolder
    {
        get
        {
            var dir = Path.Combine(AppRunFolder, "Exports");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string ExportFolder
    {
        get
        {
            var custom = DatabaseHelper.GetSetting("ExportFolder");
            if (!string.IsNullOrWhiteSpace(custom))
            {
                Directory.CreateDirectory(custom);
                return custom;
            }
            return DefaultExportFolder;
        }
    }

    public static void AppendExportButtons(
        Control.ControlCollection host,
        Func<DataGridView?> getGrid,
        string title,
        Func<string>? getSummary = null,
        string? openFolderPath = null)
    {
        host.Add(CreateToolButton("导出CSV", (_, _) => ExportCsv(getGrid(), title)));
        host.Add(CreateToolButton("导出为表格", (_, _) => ExportExcelHtml(getGrid(), title)));
        host.Add(CreateToolButton("复制摘要", (_, _) => CopySummary(getGrid(), getSummary)));
        host.Add(CreateToolButton("打印预览", (_, _) => PrintPreview(getGrid(), title)));
        host.Add(CreateToolButton("打开目录", (_, _) => OpenDirectory(openFolderPath ?? ExportFolder)));
    }
}
```

账号页保存：`DataExportHelper.SetExportFolder(path)` → 写入 `AppSettings`。

---

### 步骤 10：用户控件插件体系

1. 写 `IPageWidget` / `WidgetConfig`（步骤见第 6 节）。  
2. 实现各 `*Control` + `*Widget`。  
3. `PageWidgetRegistry` 集中注册。  
4. `CustomPageEditDialog`：两个 `ComboBox`（内容 / 按钮栏）+ 两个 `CheckedListBox`（功能）+ 公告文本。  
5. `DynamicPageView`：按 Key 创建并 `Apply`，订阅 `ToolbarAction`。

编辑对话框保存片段：

```csharp
page.ControlKey = widget.Key;
page.ControlConfigJson = PageWidgetRegistry.SerializeConfig(BuildConfigFromChecks(_contentFeatures, noticeText));
page.ButtonBarKey = buttonBar.Key;
page.ButtonBarConfigJson = PageWidgetRegistry.SerializeConfig(BuildConfigFromChecks(_barFeatures));
CustomPageService.Save(page);
```

---

### 步骤 11：空闲检测

```csharp
// Utils/ActivityMessageFilter.cs
public class ActivityMessageFilter : IMessageFilter
{
    public bool PreFilterMessage(ref Message m)
    {
        // 鼠标/键盘消息 → Session.Touch()
        return false;
    }
}
```

主窗体 Timer 调用 `CheckIdleTimeout()`，超时则 `Session.Logout()` 并 `Navigate("login")`。

---

### 步骤 12：联调与验证清单

1. `dotnet build` 无错误。  
2. `admin` / `admin123` 登录 → 客户/产品搜索与导出。  
3. 账号页「恢复默认」→ 导出路径为运行目录 `\Exports`。  
4. 页面扩展：新建页选 `product_list` + `btn_export`，确认导航出现且导出真实表格。  
5. `zhangsan` / `123456` 登录 → 仅见客户权限菜单。  
6. 缩短空闲秒数验证自动登出。

---

## 9. 数据库设计

| 表              | 用途                                    |
| --------------- | --------------------------------------- |
| `Users`         | 登录账号、角色、关联 CustomerId         |
| `Customers`     | 客户资料、账号、余额、价值评估          |
| `Products`      | 产品代码/名称/类别/价格/收益率/盈利     |
| `Holdings`      | 客户持仓（数量、成本价）                |
| `InterestRates` | 利率看板                                |
| `BankInfo`      | 银行概况                                |
| `OperationLogs` | 操作审计                                |
| `CustomPages`   | 自定义页 + 控件 Key/Config              |
| `AppSettings`   | IdleTimeout、ExportFolder、LogFolder 等 |

`CustomPages` 关键字段：

```sql
ControlKey TEXT NOT NULL DEFAULT 'buttons_only',
ControlConfigJson TEXT NOT NULL DEFAULT '{}',
ButtonBarKey TEXT NOT NULL DEFAULT 'btn_none',
ButtonBarConfigJson TEXT NOT NULL DEFAULT '{}'
```

旧库通过 `EnsureSchemaUpgrades` 自动 `ALTER TABLE` 补列。

---

## 10. 核心业务说明

| 业务     | 规则                                                    |
| -------- | ------------------------------------------------------- |
| 权限     | 客户只看本人数据；管理员/超管管理全库；超管含页面扩展   |
| 持仓价值 | 数量 × 现价等汇总（客户详情/主页）                      |
| 存取款   | 更新 `Customers.Balance` 并写操作日志                   |
| 日志双写 | SQLite + `%LocalAppData%\...\Logs\log_日期.txt`         |
| 导出     | UTF-8 BOM CSV；HTML 表 `.xls`；目录默认程序旁 `Exports` |
| 扩展页   | 内容控件 + 按钮栏 + 可选自定义按钮三层组合              |

---

## 11. 常见问题

**Q：导出文件在哪？**  
A：默认在程序运行目录下的 `Exports`。可在「账号」页查看/修改；「恢复默认」回到运行目录 `\Exports`。

**Q：数据库在哪？删库重来？**  
A：`%LocalAppData%\BankManagementSystem\bank.db`。关闭程序后删除该文件（或整个文件夹）再启动会重建并重新种子数据。

**Q：自定义页控件不显示？**  
A：确认 `ControlKey`/`ButtonBarKey` 已在 `PageWidgetRegistry` 注册；查看页眉是否显示控件显示名；看是否加载异常提示。

**Q：客户看不到「页面扩展」？**  
A：仅管理员/超管可见管理入口；客户仅能看到对其 `VisibleToCustomer=1` 的自定义页。

**Q：按钮栏点了没反应？**  
A：`DynamicPageView.OnToolbarAction` 需能找到内容区 `DataGridView`（导出类）或对应演示处理；纯 `buttons_only` 时部分动作会提示无可导出表格。

**Q：如何改默认空闲时间？**  
A：账号页修改，或改 `AppSettings.IdleTimeoutSeconds`（秒，建议 ≥ 30）。

---

## 附录：控件 Key 速查

**内容：**  
`buttons_only` · `product_list` · `product_crud` · `customer_summary` · `customer_crud` · `rate_board` · `notice_board` · `interest_calc` · `log_viewer` · `bank_info` · `holding_list` · `home_dashboard` · `user_list` · `deposit_panel`

**按钮栏：**  
`btn_none` · `btn_crud` · `btn_search` · `btn_export` · `btn_finance` · `btn_nav`

---

*文档与当前仓库源码同步：含搜索、导出目录默认程序运行文件夹、内容/按钮双控件扩展体系。*

<img width="2559" height="1532" alt="屏幕截图 2026-09-10 222550" src="https://github.com/user-attachments/assets/76911f2f-6fd4-4c6c-a09c-df50ba73d289" />
<img width="2559" height="1531" alt="屏幕截图 2026-09-10 222557" src="https://github.com/user-attachments/assets/16001058-53e3-4b91-9d96-7b71790f153e" />
<img width="2559" height="1529" alt="屏幕截图 2026-09-10 222602" src="https://github.com/user-attachments/assets/45f95b42-5722-4438-8f8e-cce54f05102c" />
<img width="2559" height="1535" alt="屏幕截图 2026-09-10 222608" src="https://github.com/user-attachments/assets/9dff51ce-4a8e-46ec-ae10-6b5884ccb4b1" />
<img width="2559" height="1528" alt="屏幕截图 2026-09-10 222612" src="https://github.com/user-attachments/assets/939bebe5-573d-4b26-ab90-0d88ae16d95f" />
<img width="2558" height="1513" alt="屏幕截图 2026-09-10 222616" src="https://github.com/user-attachments/assets/47890437-cf1e-4d00-835d-de56fcd0b0fc" />
<img width="2556" height="1525" alt="屏幕截图 2026-09-10 222620" src="https://github.com/user-attachments/assets/4c244385-bbd7-4cc5-bc61-617cff91412e" />
<img width="2554" height="1531" alt="屏幕截图 2026-09-10 222624" src="https://github.com/user-attachments/assets/47cf566d-c3e2-40a8-bd22-a4f6e7879f97" />
<img width="2558" height="1524" alt="屏幕截图 2026-09-10 222629" src="https://github.com/user-attachments/assets/e8e3b9c0-9aad-4828-872d-e5938729499b" />
<img width="2558" height="1521" alt="屏幕截图 2026-09-10 222634" src="https://github.com/user-attachments/assets/20ebe29c-e47a-496c-b9ff-df65797842e9" />



