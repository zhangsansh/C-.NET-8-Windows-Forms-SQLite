namespace BankManagementSystem.Pages;

using BankManagementSystem.Data;
using BankManagementSystem.Models;
using BankManagementSystem.Services;
using BankManagementSystem.Utils;

/// <summary>嵌入主窗体的登录 / 个人中心页面。</summary>
public class LoginPage : UserControl
{
    private readonly AuthService _auth = new();
    private readonly LogService _log = new();
    private readonly CustomerService _customers = new();
    private readonly Panel _host = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = UiTheme.Bg };

    public event Action? LoginSucceeded;
    public event Action? LogoutRequested;

    public LoginPage()
    {
        Dock = DockStyle.Fill;
        Controls.Add(_host);
        Rebuild();
        Session.LoggedIn += Rebuild;
        Session.LoggedOut += Rebuild;
        Disposed += (_, _) =>
        {
            Session.LoggedIn -= Rebuild;
            Session.LoggedOut -= Rebuild;
        };
    }

    private void Rebuild()
    {
        if (IsDisposed) return;
        if (InvokeRequired)
        {
            BeginInvoke(Rebuild);
            return;
        }
        _host.Controls.Clear();
        if (Session.IsLoggedIn)
            BuildProfileView();
        else
            BuildLoginView();
    }

    private void BuildLoginView()
    {
        var tip = new Label
        {
            Text = "请登录后使用完整功能；登录页位于顶部导航「登录 / 账号」。",
            AutoSize = true,
            ForeColor = UiTheme.TextMuted,
            Margin = new Padding(40, 12, 0, 8)
        };

        var card = new Panel
        {
            Width = 480,
            Height = 480,
            BackColor = Color.White,
            Margin = new Padding(40, 8, 40, 40)
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(220, 225, 230));
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = UiTheme.Primary };
        header.Controls.Add(new Label
        {
            Text = "用户登录",
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(32, 16, 32, 16)
        };
        // 标签行加高，避免「用户名/密码」等文字被裁切
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        cmb.Items.AddRange(new object[] { "管理员 / 超级管理员", "客户" });
        cmb.SelectedIndex = 0;
        var txtUser = new TextBox { Dock = DockStyle.Fill, Text = "admin" };
        var txtPwd = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true, Text = "admin123" };

        layout.Controls.Add(Lbl("登录身份提示"), 0, 0);
        layout.Controls.Add(cmb, 0, 1);
        layout.Controls.Add(Lbl("用户名"), 0, 2);
        layout.Controls.Add(txtUser, 0, 3);
        layout.Controls.Add(Lbl("密码"), 0, 4);
        layout.Controls.Add(txtPwd, 0, 5);

        var btn = new Button
        {
            Text = "登 录",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 4)
        };
        UiTheme.StylePrimaryButton(btn);
        btn.Height = 40;
        btn.Click += (_, _) => DoLogin(txtUser.Text, txtPwd.Text);
        layout.Controls.Add(btn, 0, 6);

        layout.Controls.Add(new Label
        {
            Text = "演示账号：admin / admin123　·　zhangsan / 123456　·　user004～user100 / 123456",
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Microsoft YaHei UI", 8.25F),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = true
        }, 0, 7);

        cmb.SelectedIndexChanged += (_, _) =>
        {
            if (cmb.SelectedIndex == 0) { txtUser.Text = "admin"; txtPwd.Text = "admin123"; }
            else { txtUser.Text = "zhangsan"; txtPwd.Text = "123456"; }
        };

        // 先 Fill 后 Top，保证标题栏不被内容区盖住
        card.Controls.Add(layout);
        card.Controls.Add(header);

        var outer = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0)
        };
        outer.Controls.Add(tip);
        outer.Controls.Add(card);
        _host.Controls.Add(outer);

        void CenterCard()
        {
            var left = Math.Max(24, (_host.ClientSize.Width - card.Width) / 2);
            tip.Margin = new Padding(left, 12, 0, 8);
            card.Margin = new Padding(left, 8, 40, 40);
        }
        _host.Resize += (_, _) => CenterCard();
        CenterCard();
    }

    private void BuildProfileView()
    {
        var user = Session.CurrentUser!;
        Customer? customer = null;
        if (user.CustomerId is int cid)
            customer = _customers.GetById(cid);

        // 纵向流式布局：各块按顺序排列，避免 TableLayout 行高/Margin 导致标题被下方盖住
        var stack = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(24, 16, 24, 24)
        };

        int contentWidth = Math.Max(680, _host.ClientSize.Width - 80);
        void SyncWidth()
        {
            contentWidth = Math.Max(680, _host.ClientSize.Width - 80);
            stack.Width = contentWidth + 48;
            foreach (Control c in stack.Controls)
            {
                if (c is Label { AutoSize: true }) continue;
                if (c is Button) continue;
                c.Width = contentWidth;
            }
        }

        // —— 标题（独立一块，不被后续控件遮挡）——
        var title = new Label
        {
            Text = "登录账号信息",
            Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 16)
        };
        stack.Controls.Add(title);

        // —— 头像 + 基本信息 ——
        var top = new TableLayoutPanel
        {
            Width = contentWidth,
            Height = 130,
            ColumnCount = 2,
            Margin = new Padding(0, 0, 0, 16),
            BackColor = Color.Transparent
        };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        top.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var avatar = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0), BackColor = Color.Transparent };
        avatar.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(UiTheme.Accent);
            e.Graphics.FillEllipse(brush, 4, 8, 88, 88);
            var initial = string.IsNullOrEmpty(user.DisplayName) ? "?" : user.DisplayName[..1];
            using var font = new Font("Microsoft YaHei UI", 26F, FontStyle.Bold);
            var size = e.Graphics.MeasureString(initial, font);
            e.Graphics.DrawString(initial, font, Brushes.White,
                4 + (88 - size.Width) / 2, 8 + (88 - size.Height) / 2);
        };
        top.Controls.Add(avatar, 0, 0);

        var infoBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            WordWrap = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Microsoft YaHei UI", 10.5F),
            Text =
                $"用户名（账号）：{user.Username}" + Environment.NewLine +
                $"姓名（显示名）：{user.DisplayName}" + Environment.NewLine +
                $"用户编号：{user.Id}" + Environment.NewLine +
                $"角色：{RoleName(user.Role)}" + Environment.NewLine +
                (customer != null
                    ? $"客户编号：{customer.CustomerNo}" + Environment.NewLine + $"银行账号：{customer.AccountNo}"
                    : "绑定客户：无")
        };
        top.Controls.Add(infoBox, 1, 0);
        stack.Controls.Add(top);

        // —— 详细资料 ——
        var detailCard = new TableLayoutPanel
        {
            Width = contentWidth,
            Height = 220,
            BackColor = Color.White,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10, 8, 10, 8),
            Margin = new Padding(0, 0, 0, 16)
        };
        detailCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        detailCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        detailCard.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        detailCard.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(220, 225, 230));
            e.Graphics.DrawRectangle(pen, 0, 0, detailCard.Width - 1, detailCard.Height - 1);
        };
        detailCard.Controls.Add(new Label
        {
            Text = "详细资料（可滚动，每项换行）",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        var detailText = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            WordWrap = true,
            ScrollBars = ScrollBars.Both,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Microsoft YaHei UI", 9.5F)
        };
        if (customer != null)
        {
            var holdings = _customers.GetHoldings(customer.Id);
            var rate = _customers.GetDepositRate();
            var interest = customer.Balance * rate;
            var productValue = holdings.Sum(h => h.MarketValue);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"住址：{customer.Address}");
            sb.AppendLine($"电话：{customer.Phone}");
            sb.AppendLine($"银行账号：{customer.AccountNo}");
            sb.AppendLine($"账号密码：******");
            sb.AppendLine($"账户余额：{customer.Balance:N2} 元");
            sb.AppendLine($"银行业务：{customer.BusinessType}");
            sb.AppendLine($"价值评估：{customer.ValueAssessment}");
            sb.AppendLine($"预计年利息（{rate:P2}）：{interest:N2} 元");
            sb.AppendLine($"持仓市值：{productValue:N2} 元");
            sb.AppendLine($"总账号价值：{customer.TotalValue:N2} 元");
            sb.AppendLine($"开户时间：{customer.CreatedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"持仓产品数：{holdings.Count}");
            foreach (var h in holdings)
                sb.AppendLine($"  · {h.ProductName}（{h.Category}）市值 {h.MarketValue:N2}");
            detailText.Text = sb.ToString();
        }
        else
        {
            detailText.Text =
                $"创建时间：{user.CreatedAt:yyyy-MM-dd HH:mm}" + Environment.NewLine +
                $"账号状态：{(user.IsActive ? "启用" : "停用")}" + Environment.NewLine +
                "权限范围：系统管理与业务操作";
        }
        detailCard.Controls.Add(detailText, 0, 1);
        stack.Controls.Add(detailCard);

        // —— 自动登出设置 ——
        var timeoutPanel = new TableLayoutPanel
        {
            Width = contentWidth,
            Height = 230,
            BackColor = Color.White,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(12, 10, 12, 10),
            Margin = new Padding(0, 0, 0, 16)
        };
        timeoutPanel.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(220, 225, 230));
            e.Graphics.DrawRectangle(pen, 0, 0, timeoutPanel.Width - 1, timeoutPanel.Height - 1);
        };
        timeoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        timeoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        timeoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        timeoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        timeoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        timeoutPanel.Controls.Add(new Label
        {
            Text = "自动登出设置",
            Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        timeoutPanel.Controls.Add(new Label
        {
            Text = "无操作超时（秒）：在规定时间内不操作窗口将自动登出",
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 1);

        var rowTimeout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight
        };
        var num = new NumericUpDown
        {
            Width = 120,
            Minimum = 30,
            Maximum = 3600,
            Value = Math.Clamp(Session.IdleTimeoutSeconds, 30, 3600),
            Increment = 30,
            Margin = new Padding(0, 4, 12, 0)
        };
        rowTimeout.Controls.Add(new Label
        {
            Text = "超时秒数",
            AutoSize = true,
            Margin = new Padding(0, 10, 8, 0)
        });
        rowTimeout.Controls.Add(num);
        timeoutPanel.Controls.Add(rowTimeout, 0, 2);

        var presets = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight
        };
        foreach (var sec in new[] { 60, 180, 300, 600, 900 })
        {
            var s = sec;
            var b = new Button
            {
                Text = $"{s / 60} 分钟",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(80, 32),
                Margin = new Padding(0, 2, 8, 2),
                Padding = new Padding(10, 2, 10, 2)
            };
            UiTheme.StylePrimaryButton(b);
            b.AutoSize = true;
            b.MinimumSize = new Size(80, 32);
            b.Padding = new Padding(10, 2, 10, 2);
            b.Click += (_, _) => num.Value = s;
            presets.Controls.Add(b);
        }
        timeoutPanel.Controls.Add(presets, 0, 3);

        var bottomRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var save = new Button
        {
            Text = "保存超时设置",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(130, 34),
            Padding = new Padding(12, 2, 12, 2),
            Dock = DockStyle.Left,
            Margin = new Padding(0, 2, 0, 0)
        };
        UiTheme.StylePrimaryButton(save);
        save.AutoSize = true;
        save.MinimumSize = new Size(130, 34);
        save.Click += (_, _) =>
        {
            Session.IdleTimeoutSeconds = (int)num.Value;
            DatabaseHelper.SetSetting("IdleTimeoutSeconds", Session.IdleTimeoutSeconds.ToString());
            Session.Touch();
            _log.Write("更新超时设置", "认证", $"{Session.IdleTimeoutSeconds} 秒");
            MessageBox.Show($"已设置：{Session.IdleTimeoutSeconds} 秒内无操作将自动登出。", "设置成功");
        };
        var remain = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.TextMuted,
            AutoEllipsis = true,
            Margin = new Padding(12, 0, 0, 0)
        };
        void UpdateRemain()
        {
            if (!Session.IsLoggedIn || remain.IsDisposed) return;
            remain.Text = $"登录时间：{Session.LoginTime:yyyy-MM-dd HH:mm:ss}　　剩余空闲：{Session.RemainingIdleSeconds} 秒";
        }
        UpdateRemain();
        var remainTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        remainTimer.Tick += (_, _) =>
        {
            if (!Session.IsLoggedIn || remain.IsDisposed) { remainTimer.Stop(); return; }
            UpdateRemain();
        };
        remainTimer.Start();
        Disposed += (_, _) => remainTimer.Stop();
        bottomRow.Controls.Add(save, 0, 0);
        bottomRow.Controls.Add(remain, 1, 0);
        timeoutPanel.Controls.Add(bottomRow, 0, 4);
        stack.Controls.Add(timeoutPanel);

        // —— 导出目录设置 ——
        var exportPanel = new TableLayoutPanel
        {
            Width = contentWidth,
            Height = 150,
            BackColor = Color.White,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12, 10, 12, 10),
            Margin = new Padding(0, 0, 0, 16)
        };
        exportPanel.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(220, 225, 230));
            e.Graphics.DrawRectangle(pen, 0, 0, exportPanel.Width - 1, exportPanel.Height - 1);
        };
        exportPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        exportPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        exportPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        exportPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        exportPanel.Controls.Add(new Label
        {
            Text = "导出目录设置",
            Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        exportPanel.Controls.Add(new Label
        {
            Text = "客户/产品/日志等页的「导出CSV / 导出为表格」文件将保存到此目录",
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 1);

        var exportPathRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        exportPathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        exportPathRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        var txtExport = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = DataExportHelper.ExportFolder,
            Margin = new Padding(0, 4, 8, 0)
        };
        var btnBrowse = DataExportHelper.CreateToolButton("浏览…", (_, _) =>
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "选择导出文件保存目录",
                SelectedPath = Directory.Exists(txtExport.Text) ? txtExport.Text : DataExportHelper.DefaultExportFolder
            };
            if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                txtExport.Text = dlg.SelectedPath;
        }, 88);
        btnBrowse.Margin = new Padding(0, 2, 0, 0);
        exportPathRow.Controls.Add(txtExport, 0, 0);
        exportPathRow.Controls.Add(btnBrowse, 1, 0);
        exportPanel.Controls.Add(exportPathRow, 0, 2);

        var exportBtnRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight
        };
        var btnSaveExport = DataExportHelper.CreateToolButton("保存导出目录", (_, _) =>
        {
            var path = txtExport.Text.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("请输入导出目录路径");
                return;
            }
            try
            {
                DataExportHelper.SetExportFolder(path);
                txtExport.Text = DataExportHelper.ExportFolder;
                _log.Write("更新导出目录", "账号设置", path);
                MessageBox.Show("导出目录已保存：\n" + DataExportHelper.ExportFolder, "设置成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message);
            }
        }, 130);
        var btnOpenExport = DataExportHelper.CreateToolButton("打开导出目录", (_, _) =>
            DataExportHelper.OpenDirectory(DataExportHelper.ExportFolder), 120);
        var btnResetExport = DataExportHelper.CreateToolButton("恢复默认", (_, _) =>
        {
            txtExport.Text = DataExportHelper.DefaultExportFolder;
            DataExportHelper.SetExportFolder(DataExportHelper.DefaultExportFolder);
            MessageBox.Show("已恢复默认导出目录。", "设置成功");
        }, 100);
        exportBtnRow.Controls.Add(btnSaveExport);
        exportBtnRow.Controls.Add(btnOpenExport);
        exportBtnRow.Controls.Add(btnResetExport);
        exportPanel.Controls.Add(exportBtnRow, 0, 3);
        stack.Controls.Add(exportPanel);

        var logout = new Button
        {
            Text = "退出登录",
            AutoSize = true,
            MinimumSize = new Size(120, 36),
            Padding = new Padding(16, 4, 16, 4),
            Margin = new Padding(0, 4, 0, 24)
        };
        UiTheme.StyleDangerButton(logout);
        logout.AutoSize = true;
        logout.Click += (_, _) =>
        {
            _log.Write("手动退出", "认证", user.Username);
            Session.Logout();
            LogoutRequested?.Invoke();
        };
        stack.Controls.Add(logout);

        _host.Controls.Add(stack);
        _host.Resize += (_, _) => SyncWidth();
        SyncWidth();
    }

    private void DoLogin(string username, string password)
    {
        var user = _auth.Login(username, password);
        if (user == null)
        {
            MessageBox.Show("用户名或密码错误，或账号已停用。", "登录失败",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Session.Login(user);
        _log.Write("登录成功", "认证", $"{user.DisplayName} ({user.Role})");
        LoginSucceeded?.Invoke();
    }

    private static Label Lbl(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        ForeColor = UiTheme.TextMuted,
        TextAlign = ContentAlignment.MiddleLeft,
        AutoSize = false
    };

    private static string RoleName(UserRole role) => role switch
    {
        UserRole.SuperAdmin => "超级管理员",
        UserRole.Admin => "管理员",
        UserRole.Customer => "客户",
        _ => role.ToString()
    };
}
