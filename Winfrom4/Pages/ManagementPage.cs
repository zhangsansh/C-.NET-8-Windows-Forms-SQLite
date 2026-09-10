namespace BankManagementSystem.Pages;

using BankManagementSystem.Models;
using BankManagementSystem.Services;
using BankManagementSystem.Utils;

public class ManagementPage : UserControl
{
    private readonly BankService _bank = new();
    private readonly UserService _users = new();
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };

    public ManagementPage()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Bg;
        Padding = new Padding(8);
        Controls.Add(_tabs);
        BuildBankTab();
        BuildRateTab();
        BuildUserTab();
    }

    private void BuildBankTab()
    {
        var page = new TabPage("银行信息");
        var info = _bank.GetBankInfo() ?? new BankInfo();
        var txtName = new TextBox { Text = info.BankName, Width = 360 };
        var txtAddr = new TextBox { Text = info.Address, Width = 360 };
        var txtPhone = new TextBox { Text = info.Phone, Width = 360 };
        var txtDesc = new TextBox { Text = info.Description, Width = 360, Height = 80, Multiline = true };
        var txtAssets = new TextBox { Text = info.TotalAssets.ToString("F2"), Width = 360 };
        var txtDep = new TextBox { Text = info.TotalDeposits.ToString("F2"), Width = 360 };
        var txtLoan = new TextBox { Text = info.TotalLoans.ToString("F2"), Width = 360 };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(16),
            AutoScroll = true
        };
        void AddRow(string label, Control c)
        {
            panel.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 8, 0, 0) });
            panel.Controls.Add(c);
        }
        AddRow("银行名称", txtName);
        AddRow("地址", txtAddr);
        AddRow("电话", txtPhone);
        AddRow("简介", txtDesc);
        AddRow("总资产", txtAssets);
        AddRow("总存款", txtDep);
        AddRow("总贷款", txtLoan);

        var save = new Button { Text = "保存银行信息", AutoSize = true, MinimumSize = new Size(140, 34), Margin = new Padding(0, 16, 8, 0), Padding = new Padding(12, 2, 12, 2) };
        UiTheme.StylePrimaryButton(save);
        save.AutoSize = true;
        save.Click += (_, _) =>
        {
            if (!decimal.TryParse(txtAssets.Text, out var a) ||
                !decimal.TryParse(txtDep.Text, out var d) ||
                !decimal.TryParse(txtLoan.Text, out var l))
            {
                MessageBox.Show("金额格式错误");
                return;
            }
            info.BankName = txtName.Text.Trim();
            info.Address = txtAddr.Text.Trim();
            info.Phone = txtPhone.Text.Trim();
            info.Description = txtDesc.Text.Trim();
            info.TotalAssets = a;
            info.TotalDeposits = d;
            info.TotalLoans = l;
            _bank.UpdateBankInfo(info);
            _bank.RefreshCustomerCount();
            MessageBox.Show("已保存");
        };

        (string, string)[] Rows() =>
        [
            ("银行名称", txtName.Text),
            ("地址", txtAddr.Text),
            ("电话", txtPhone.Text),
            ("简介", txtDesc.Text),
            ("总资产", txtAssets.Text),
            ("总存款", txtDep.Text),
            ("总贷款", txtLoan.Text),
            ("客户数", info.CustomerCount.ToString())
        ];

        var exportBar = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 12, 0, 0) };
        exportBar.Controls.Add(save);
        exportBar.Controls.Add(DataExportHelper.CreateToolButton("导出CSV", (_, _) =>
            DataExportHelper.ExportKeyValuesToCsv(Rows(), "银行信息"), 100));
        exportBar.Controls.Add(DataExportHelper.CreateToolButton("导出为表格", (_, _) =>
            DataExportHelper.ExportKeyValuesToTable(Rows(), "银行信息"), 110));
        exportBar.Controls.Add(DataExportHelper.CreateToolButton("复制摘要", (_, _) =>
        {
            var text = string.Join(Environment.NewLine, Rows().Select(r => $"{r.Item1}：{r.Item2}"));
            DataExportHelper.CopyText($"【银行信息】\n{text}", "银行信息");
        }, 100));
        exportBar.Controls.Add(DataExportHelper.CreateToolButton("打印预览", (_, _) =>
        {
            var g = new DataGridView();
            g.Columns.Add("项目", "项目");
            g.Columns.Add("内容", "内容");
            foreach (var (k, v) in Rows()) g.Rows.Add(k, v);
            DataExportHelper.ShowPrintPreview(g, "银行信息");
            g.Dispose();
        }, 100));
        exportBar.Controls.Add(DataExportHelper.CreateToolButton("打开目录", (_, _) =>
            DataExportHelper.OpenDirectory(DataExportHelper.ExportFolder), 100));
        panel.Controls.Add(exportBar);
        page.Controls.Add(panel);
        _tabs.TabPages.Add(page);
    }

    private void BuildRateTab()
    {
        var page = new TabPage("利率管理");
        var grid = new DataGridView { Dock = DockStyle.Fill };
        List<InterestRate> data = new();

        void Reload()
        {
            data = _bank.GetRates();
            grid.DataSource = data.Select(r => new
            {
                名称 = r.Name,
                期限 = r.Period,
                利率 = r.Rate.ToString("P4"),
                说明 = r.Description,
                Id = r.Id
            }).ToList();
            if (grid.Columns["Id"] != null) grid.Columns["Id"].Visible = false;
        }

        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, AutoScroll = true, WrapContents = false };
        Button Mk(string t, EventHandler h) => DataExportHelper.CreateToolButton(t, h, 88);
        bar.Controls.Add(Mk("刷新", (_, _) => Reload()));
        bar.Controls.Add(Mk("新增", (_, _) =>
        {
            using var dlg = new RateEditDialog();
            if (dlg.ShowDialog(FindForm()) == DialogResult.OK && dlg.Result != null)
            {
                _bank.AddRate(dlg.Result);
                Reload();
            }
        }));
        bar.Controls.Add(Mk("编辑", (_, _) =>
        {
            if (grid.CurrentRow == null) return;
            var id = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
            var r = data.First(x => x.Id == id);
            using var dlg = new RateEditDialog(r);
            if (dlg.ShowDialog(FindForm()) == DialogResult.OK && dlg.Result != null)
            {
                _bank.UpdateRate(dlg.Result);
                Reload();
            }
        }));
        bar.Controls.Add(Mk("删除", (_, _) =>
        {
            if (grid.CurrentRow == null) return;
            var id = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
            if (MessageBox.Show("确认删除？", "确认", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _bank.DeleteRate(id);
            Reload();
        }));
        DataExportHelper.AppendExportButtons(bar.Controls, () => grid, "利率列表",
            () => DataExportHelper.BuildGridSummary(grid, "利率列表"));
        bar.Controls.Add(new Label
        {
            Text = "可滚动查看全部利率",
            AutoSize = true,
            Margin = new Padding(8, 12, 0, 0),
            ForeColor = UiTheme.TextMuted
        });
        page.Controls.Add(UiTheme.WrapGrid(grid));
        page.Controls.Add(bar);
        _tabs.TabPages.Add(page);
        Reload();
    }

    private void BuildUserTab()
    {
        var page = new TabPage("用户管理");
        var grid = new DataGridView { Dock = DockStyle.Fill };
        List<User> data = new();
        var countLabel = new Label { AutoSize = true, Margin = new Padding(12, 12, 0, 0), ForeColor = UiTheme.TextMuted };

        void Reload()
        {
            data = _users.GetAll();
            if (!Session.IsSuperAdmin)
                data = data.Where(u => u.Role != UserRole.SuperAdmin).ToList();
            grid.DataSource = data.Select(u => new
            {
                编号 = u.Id,
                用户名 = u.Username,
                密码 = u.Password,
                显示名 = u.DisplayName,
                角色 = u.Role switch
                {
                    UserRole.SuperAdmin => "超级管理员",
                    UserRole.Admin => "管理员",
                    _ => "客户"
                },
                客户Id = u.CustomerId?.ToString() ?? "-",
                创建时间 = u.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                状态 = u.IsActive ? "启用" : "停用",
                Id = u.Id
            }).ToList();
            if (grid.Columns["Id"] != null) grid.Columns["Id"].Visible = false;
            var adminCount = data.Count(u => u.Role is UserRole.Admin or UserRole.SuperAdmin);
            var customerCount = data.Count(u => u.Role == UserRole.Customer);
            countLabel.Text = $"共 {data.Count} 个用户（管理员 {adminCount} · 客户 {customerCount}），拖动滚动条查看全部";
        }

        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, AutoScroll = true, WrapContents = false };
        Button Mk(string t, EventHandler h) => DataExportHelper.CreateToolButton(t, h, 88);
        bar.Controls.Add(Mk("刷新", (_, _) => Reload()));
        bar.Controls.Add(Mk("新增", (_, _) =>
        {
            using var dlg = new UserEditDialog();
            if (dlg.ShowDialog(FindForm()) == DialogResult.OK && dlg.Result != null)
            {
                if (dlg.Result.Role == UserRole.SuperAdmin && !Session.IsSuperAdmin)
                {
                    MessageBox.Show("无权创建超级管理员");
                    return;
                }
                _users.Add(dlg.Result);
                Reload();
            }
        }));
        bar.Controls.Add(Mk("编辑", (_, _) =>
        {
            if (grid.CurrentRow == null) return;
            var id = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
            var u = data.First(x => x.Id == id);
            using var dlg = new UserEditDialog(u);
            if (dlg.ShowDialog(FindForm()) == DialogResult.OK && dlg.Result != null)
            {
                _users.Update(dlg.Result);
                Reload();
            }
        }));
        bar.Controls.Add(Mk("删除", (_, _) =>
        {
            if (grid.CurrentRow == null) return;
            var id = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
            if (id == Session.CurrentUser?.Id) { MessageBox.Show("不能删除当前登录用户"); return; }
            if (MessageBox.Show("确认删除？", "确认", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _users.Delete(id);
            Reload();
        }));
        DataExportHelper.AppendExportButtons(bar.Controls, () => grid, "用户列表",
            () => DataExportHelper.BuildGridSummary(grid, "用户列表"));
        bar.Controls.Add(countLabel);
        page.Controls.Add(UiTheme.WrapGrid(grid));
        page.Controls.Add(bar);
        _tabs.TabPages.Add(page);
        Reload();
    }
}

public class RateEditDialog : Form
{
    public InterestRate? Result { get; private set; }
    public RateEditDialog(InterestRate? e = null)
    {
        Text = e == null ? "新增利率" : "编辑利率";
        ClientSize = new Size(380, 260);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        var n = new TextBox { Location = new Point(120, 20), Width = 220, Text = e?.Name ?? "" };
        var p = new TextBox { Location = new Point(120, 60), Width = 220, Text = e?.Period ?? "" };
        var r = new TextBox { Location = new Point(120, 100), Width = 220, Text = e?.Rate.ToString("F4") ?? "0.01" };
        var d = new TextBox { Location = new Point(120, 140), Width = 220, Text = e?.Description ?? "" };
        Controls.Add(new Label { Text = "名称", Location = new Point(20, 22) });
        Controls.Add(new Label { Text = "期限", Location = new Point(20, 62) });
        Controls.Add(new Label { Text = "利率", Location = new Point(20, 102) });
        Controls.Add(new Label { Text = "说明", Location = new Point(20, 142) });
        Controls.AddRange(new Control[] { n, p, r, d });
        var ok = new Button { Text = "保存", Location = new Point(160, 190), Width = 80 };
        UiTheme.StylePrimaryButton(ok);
        ok.Click += (_, _) =>
        {
            if (!decimal.TryParse(r.Text, out var rate)) { MessageBox.Show("利率格式错误"); return; }
            Result = new InterestRate { Id = e?.Id ?? 0, Name = n.Text.Trim(), Period = p.Text.Trim(), Rate = rate, Description = d.Text.Trim() };
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(ok);
    }
}

public class UserEditDialog : Form
{
    public User? Result { get; private set; }
    public UserEditDialog(User? e = null)
    {
        Text = e == null ? "新增用户" : "编辑用户";
        ClientSize = new Size(400, 340);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        var u = new TextBox { Location = new Point(120, 20), Width = 240, Text = e?.Username ?? "" };
        var p = new TextBox { Location = new Point(120, 60), Width = 240, Text = e?.Password ?? "" };
        var d = new TextBox { Location = new Point(120, 100), Width = 240, Text = e?.DisplayName ?? "" };
        var role = new ComboBox { Location = new Point(120, 140), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
        role.Items.AddRange(Session.IsSuperAdmin
            ? new object[] { UserRole.Customer, UserRole.Admin, UserRole.SuperAdmin }
            : new object[] { UserRole.Customer, UserRole.Admin });
        role.SelectedItem = e?.Role ?? UserRole.Customer;
        var cid = new TextBox { Location = new Point(120, 180), Width = 240, Text = e?.CustomerId?.ToString() ?? "" };
        var active = new CheckBox { Text = "启用", Location = new Point(120, 220), Checked = e?.IsActive ?? true };
        Controls.Add(new Label { Text = "用户名", Location = new Point(20, 22) });
        Controls.Add(new Label { Text = "密码", Location = new Point(20, 62) });
        Controls.Add(new Label { Text = "显示名", Location = new Point(20, 102) });
        Controls.Add(new Label { Text = "角色", Location = new Point(20, 142) });
        Controls.Add(new Label { Text = "客户Id", Location = new Point(20, 182) });
        Controls.AddRange(new Control[] { u, p, d, role, cid, active });
        var ok = new Button { Text = "保存", Location = new Point(180, 270), Width = 80 };
        UiTheme.StylePrimaryButton(ok);
        ok.Click += (_, _) =>
        {
            int? customerId = null;
            if (!string.IsNullOrWhiteSpace(cid.Text))
            {
                if (!int.TryParse(cid.Text, out var id)) { MessageBox.Show("客户Id应为整数"); return; }
                customerId = id;
            }
            Result = new User
            {
                Id = e?.Id ?? 0,
                Username = u.Text.Trim(),
                Password = p.Text,
                DisplayName = d.Text.Trim(),
                Role = (UserRole)role.SelectedItem!,
                CustomerId = customerId,
                IsActive = active.Checked
            };
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(ok);
    }
}
