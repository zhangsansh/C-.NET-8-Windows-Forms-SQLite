namespace BankManagementSystem.Forms;

using BankManagementSystem.Data;
using BankManagementSystem.Models;
using BankManagementSystem.Pages;
using BankManagementSystem.Services;
using BankManagementSystem.Utils;

public class MainForm : Form
{
        private readonly Panel _topBar = new() { Dock = DockStyle.Top, Height = 122, BackColor = UiTheme.Primary };
    private readonly Panel _content = new() { Dock = DockStyle.Fill, BackColor = UiTheme.Bg };
    private readonly Panel _status = new() { Dock = DockStyle.Bottom, Height = 36, BackColor = UiTheme.PrimaryDark };
    private readonly Label _statusUser = new() { AutoSize = true, ForeColor = Color.White };
    private readonly Label _statusDate = new() { AutoSize = true, ForeColor = Color.White };
    private readonly Label _statusExtra = new() { AutoSize = true, ForeColor = Color.White };
    private readonly Panel _headerRow = new()
    {
        Dock = DockStyle.Top,
        Height = 48,
        BackColor = UiTheme.PrimaryDark
    };
    private readonly Label _headerBrand = new();
    private readonly Label _headerRole = new();
    private readonly Button _headerLogout = new();
    private readonly FlowLayoutPanel _navButtons = new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(8, 0, 8, 0)
    };
    private readonly Dictionary<string, Button> _navMap = new();
    private readonly Dictionary<string, Func<Control>> _factories = new();
    private readonly CustomPageService _pageSvc = new();
    private readonly LogService _log = new();
    private readonly System.Windows.Forms.Timer _clock = new() { Interval = 1000 };
    private readonly ActivityMessageFilter _activityFilter = new();
    private string _currentKey = "";
    private bool _autoLogoutPrompting;

    // Segoe MDL2 Assets glyphs
    private static class Icons
    {
        public const string Login = "\uE77B";      // Contact
        public const string Home = "\uE80F";       // Home
        public const string Customer = "\uE716";   // People
        public const string Product = "\uE719";    // Shop
        public const string Manage = "\uE713";     // Settings
        public const string Log = "\uE8A5";        // Document
        public const string Pages = "\uE8F1";      // Page
        public const string Custom = "\uE8A1";     // View
    }

    public MainForm()
    {
        Text = "银行管理系统";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1100, 700);
        Font = new Font("Microsoft YaHei UI", 9.5F);
        BackColor = UiTheme.Bg;

        if (int.TryParse(DatabaseHelper.GetSetting("IdleTimeoutSeconds", "300"), out var sec) && sec >= 30)
            Session.IdleTimeoutSeconds = sec;

        BuildChrome();
        BuildNav();
        Navigate("login");

        Application.AddMessageFilter(_activityFilter);
        Session.LoggedIn += OnSessionChanged;
        Session.LoggedOut += OnSessionChanged;

        _clock.Tick += (_, _) =>
        {
            UpdateStatus();
            CheckIdleTimeout();
        };
        _clock.Start();

        FormClosed += (_, _) =>
        {
            _clock.Stop();
            Application.RemoveMessageFilter(_activityFilter);
            Session.LoggedIn -= OnSessionChanged;
            Session.LoggedOut -= OnSessionChanged;
            if (Session.IsLoggedIn)
            {
                _log.Write("退出系统", "认证", Session.CurrentUser!.Username);
                Session.Logout();
            }
        };
    }

    private void OnSessionChanged()
    {
        if (IsDisposed) return;
        if (InvokeRequired)
        {
            BeginInvoke(OnSessionChanged);
            return;
        }
        RefreshHeader();
        BuildNav();
        Navigate(Session.IsLoggedIn ? "home" : "login");
        UpdateStatus();
    }

    private void BuildChrome()
    {
        _headerBrand.ForeColor = Color.White;
        _headerBrand.Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold);
        _headerBrand.AutoSize = true;
        _headerBrand.Location = new Point(16, 12);

        _headerRole.ForeColor = Color.FromArgb(200, 230, 255);
        _headerRole.AutoSize = true;
        _headerRole.Location = new Point(280, 16);

        _headerLogout.Text = "退出登录";
        _headerLogout.AutoSize = true;
        _headerLogout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _headerLogout.MinimumSize = new Size(120, 32);
        _headerLogout.Padding = new Padding(16, 4, 16, 4);
        _headerLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        UiTheme.StylePrimaryButton(_headerLogout);
        _headerLogout.AutoSize = true;
        _headerLogout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _headerLogout.MinimumSize = new Size(120, 32);
        _headerLogout.Padding = new Padding(16, 4, 16, 4);
        _headerLogout.Font = new Font("Microsoft YaHei UI", 9.5F);
        _headerLogout.BackColor = UiTheme.Accent;
        _headerLogout.ForeColor = Color.White;
        _headerRow.Resize += (_, _) => PlaceHeaderControls();
        _headerLogout.SizeChanged += (_, _) => PlaceHeaderControls();
        _headerBrand.SizeChanged += (_, _) => PlaceHeaderControls();
        _headerLogout.Click += (_, _) =>
        {
            if (!Session.IsLoggedIn)
            {
                Navigate("login");
                return;
            }
            _log.Write("手动退出", "认证", Session.CurrentUser!.Username);
            Session.Logout();
        };

        _headerRow.Controls.Add(_headerBrand);
        _headerRow.Controls.Add(_headerRole);
        _headerRow.Controls.Add(_headerLogout);
        RefreshHeader();

        var navRow = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Primary
        };
        navRow.Controls.Add(_navButtons);

        _topBar.Controls.Add(navRow);
        _topBar.Controls.Add(_headerRow);

        var statusInner = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(12, 8, 12, 0)
        };
        statusInner.Controls.Add(_statusUser);
        statusInner.Controls.Add(Spacer());
        statusInner.Controls.Add(_statusDate);
        statusInner.Controls.Add(Spacer());
        statusInner.Controls.Add(_statusExtra);
        _status.Controls.Add(statusInner);
        UpdateStatus();

        Controls.Add(_content);
        Controls.Add(_status);
        Controls.Add(_topBar);
    }

    private void PlaceHeaderControls()
    {
        _headerLogout.Left = Math.Max(8, _headerRow.ClientSize.Width - _headerLogout.Width - 16);
        _headerLogout.Top = Math.Max(4, (_headerRow.ClientSize.Height - _headerLogout.Height) / 2);
        _headerRole.Left = _headerBrand.Right + 16;
        _headerRole.Top = Math.Max(4, (_headerRow.ClientSize.Height - _headerRole.Height) / 2);
        _headerBrand.Top = Math.Max(4, (_headerRow.ClientSize.Height - _headerBrand.Height) / 2);
    }

    private void RefreshHeader()
    {
        if (Session.IsLoggedIn)
        {
            _headerBrand.Text = Session.IsCustomer ? "客户服务中心" : "银行管理控制台";
            _headerRole.Text = RoleText();
            _headerLogout.Text = "退出登录";
        }
        else
        {
            _headerBrand.Text = "银行管理系统";
            _headerRole.Text = "未登录 · 请先登录";
            _headerLogout.Text = "去登录";
        }
        // 文字变化后重新测量宽度，避免「退出登录」被裁切
        _headerLogout.AutoSize = true;
        _headerLogout.PerformLayout();
        PlaceHeaderControls();
    }

    private static Label Spacer() => new()
    {
        Text = "  |  ",
        AutoSize = true,
        ForeColor = Color.FromArgb(160, 180, 200)
    };

    private static string RoleText() => Session.CurrentUser?.Role switch
    {
        UserRole.SuperAdmin => "超级管理员模式",
        UserRole.Admin => "管理员模式",
        UserRole.Customer => "客户模式",
        _ => ""
    };

    private void UpdateStatus()
    {
        if (Session.IsLoggedIn)
        {
            var u = Session.CurrentUser!;
            _statusUser.Text = $"当前用户：{u.DisplayName}（{u.Username} / {u.Role}）";
            _statusExtra.Text = $"空闲剩余 {Session.RemainingIdleSeconds} 秒自动登出 · " +
                (Session.IsCustomer
                    ? "权限：本人账户 · 存取款 · 产品 · 日志"
                    : "权限：管理 · 日志 · 页面扩展");
        }
        else
        {
            _statusUser.Text = "当前用户：未登录";
            _statusExtra.Text = "请点击顶部「登录」图标登录后使用系统功能";
        }
        _statusDate.Text = $"日期：{DateTime.Now:yyyy年MM月dd日 HH:mm:ss dddd}";
    }

    private void CheckIdleTimeout()
    {
        if (!Session.IsIdleTimedOut || _autoLogoutPrompting) return;
        _autoLogoutPrompting = true;
        try
        {
            var name = Session.CurrentUser?.Username ?? "";
            _log.Write("超时自动登出", "认证", $"{name}，超时 {Session.IdleTimeoutSeconds} 秒");
            Session.Logout();
            MessageBox.Show(
                $"已超过 {Session.IdleTimeoutSeconds} 秒无操作，系统已自动登出。\n请重新登录。",
                "自动登出",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        finally
        {
            _autoLogoutPrompting = false;
        }
    }

    private void BuildNav()
    {
        _navButtons.Controls.Clear();
        _navMap.Clear();
        _factories.Clear();

        AddNav("login", Session.IsLoggedIn ? "账号" : "登录", Icons.Login, () =>
        {
            var page = new LoginPage();
            return page;
        });

        AddNav("home", "主页", Icons.Home, () => new HomePage());

        if (!Session.IsLoggedIn)
        {
            HighlightCurrent();
            return;
        }

        if (Session.IsCustomer)
        {
            AddNav("customer", "账户", Icons.Customer, () => new CustomerPage());
            AddNav("product", "产品", Icons.Product, () => new ProductPage());
            AddNav("log", "日志", Icons.Log, () => new LogPage());
        }
        else
        {
            AddNav("customer", "客户", Icons.Customer, () => new CustomerPage());
            AddNav("product", "产品", Icons.Product, () => new ProductPage());
            AddNav("manage", "管理", Icons.Manage, () => new ManagementPage());
            AddNav("log", "日志", Icons.Log, () => new LogPage());
            if (Session.IsAdmin)
            {
                AddNav("pages", "扩展", Icons.Pages, () =>
                {
                    var p = new PageManagerPage();
                    p.PagesChanged += RebuildCustomNav;
                    return p;
                });
            }
        }

        foreach (var page in _pageSvc.GetVisibleFor(Session.CurrentUser!.Role))
        {
            var local = page;
            var title = local.Title.Length > 4 ? local.Title[..4] : local.Title;
            AddNav("custom:" + local.PageKey, title, Icons.Custom, () => new DynamicPageView(local));
        }

        HighlightCurrent();
    }

    private void HighlightCurrent()
    {
        foreach (var kv in _navMap)
            UiTheme.StyleTopNavButton(kv.Value, kv.Key == _currentKey);
    }

    private void RebuildCustomNav()
    {
        var current = _currentKey;
        BuildNav();
        if (_navMap.ContainsKey(current))
            Navigate(current);
        else
            Navigate("home");
    }

    private void AddNav(string key, string text, string iconGlyph, Func<Control> factory)
    {
        var btn = new Button
        {
            Text = text,
            Tag = key
        };
        UiTheme.StyleTopNavButton(btn);
        UiTheme.AttachNavGlyph(btn, iconGlyph);
        btn.Click += (_, _) => Navigate(key);
        _navButtons.Controls.Add(btn);
        _navMap[key] = btn;
        _factories[key] = factory;
    }

    private void Navigate(string key)
    {
        if (!_factories.TryGetValue(key, out var factory))
            return;

        if (!Session.IsLoggedIn && key is not ("login" or "home"))
        {
            MessageBox.Show("请先登录后再访问该功能。", "需要登录", MessageBoxButtons.OK, MessageBoxIcon.Information);
            key = "login";
            factory = _factories[key];
        }

        _currentKey = key;
        HighlightCurrent();

        _content.Controls.Clear();
        var page = factory();
        page.Dock = DockStyle.Fill;
        _content.Controls.Add(page);
        Session.Touch();
        if (Session.IsLoggedIn)
            _log.Write("页面切换", "导航", key);
    }
}
