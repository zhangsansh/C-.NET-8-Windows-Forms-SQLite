namespace BankManagementSystem.Forms;

using BankManagementSystem.Services;
using BankManagementSystem.Utils;

public class LoginForm : Form
{
    private readonly TextBox _txtUser = new();
    private readonly TextBox _txtPwd = new() { UseSystemPasswordChar = true };
    private readonly ComboBox _cmbRoleHint = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly AuthService _auth = new();
    private readonly LogService _log = new();

    public LoginForm()
    {
        Text = "银行管理系统 - 登录";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(480, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = UiTheme.Bg;
        Font = new Font("Microsoft YaHei UI", 10F);

        var header = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = UiTheme.Primary };
        var title = new Label
        {
            Text = "华夏示范银行管理系统",
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold),
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        header.Controls.Add(title);

        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(50, 30, 50, 20) };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        layout.Controls.Add(new Label { Text = "登录身份提示", Dock = DockStyle.Fill, ForeColor = UiTheme.TextMuted }, 0, 0);
        _cmbRoleHint.Items.AddRange(new object[] { "管理员 / 超级管理员", "客户" });
        _cmbRoleHint.SelectedIndex = 0;
        _cmbRoleHint.Dock = DockStyle.Fill;
        layout.Controls.Add(_cmbRoleHint, 0, 1);

        layout.Controls.Add(new Label { Text = "用户名", Dock = DockStyle.Fill, ForeColor = UiTheme.TextMuted }, 0, 2);
        _txtUser.Dock = DockStyle.Fill;
        layout.Controls.Add(_txtUser, 0, 3);

        layout.Controls.Add(new Label { Text = "密码", Dock = DockStyle.Fill, ForeColor = UiTheme.TextMuted }, 0, 4);
        _txtPwd.Dock = DockStyle.Fill;
        layout.Controls.Add(_txtPwd, 0, 5);

        var btnLogin = new Button { Text = "登 录", Dock = DockStyle.Fill };
        UiTheme.StylePrimaryButton(btnLogin);
        btnLogin.Click += BtnLogin_Click;
        layout.Controls.Add(btnLogin, 0, 7);

        var tip = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Microsoft YaHei UI", 8.5F),
            Text = "演示账号：\n超级管理员 superadmin / admin123\n管理员 admin / admin123\n客户 zhangsan / 123456"
        };

        AcceptButton = btnLogin;
        body.Controls.Add(layout);
        Controls.Add(body);
        Controls.Add(tip);
        Controls.Add(header);

        _cmbRoleHint.SelectedIndexChanged += (_, _) =>
        {
            if (_cmbRoleHint.SelectedIndex == 0)
            {
                _txtUser.Text = "admin";
                _txtPwd.Text = "admin123";
            }
            else
            {
                _txtUser.Text = "zhangsan";
                _txtPwd.Text = "123456";
            }
        };
        _txtUser.Text = "admin";
        _txtPwd.Text = "admin123";
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        var user = _auth.Login(_txtUser.Text, _txtPwd.Text);
        if (user == null)
        {
            MessageBox.Show("用户名或密码错误，或账号已停用。", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Session.Login(user);
        _log.Write("登录成功", "认证", $"{user.DisplayName} ({user.Role})");
        DialogResult = DialogResult.OK;
        Close();
    }
}
