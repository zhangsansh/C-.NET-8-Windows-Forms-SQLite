namespace BankManagementSystem.Pages;

using System.Text;
using BankManagementSystem.Models;
using BankManagementSystem.Services;
using BankManagementSystem.Utils;

public class CustomerPage : UserControl
{
    private readonly CustomerService _svc = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly DataGridView _holdingGrid = new() { Dock = DockStyle.Fill };
    private readonly TextBox _detail = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        WordWrap = true,
        ScrollBars = ScrollBars.Both,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.White,
        Font = new Font("Microsoft YaHei UI", 10F),
        AcceptsReturn = true
    };
    private readonly Label _countLabel = new()
    {
        AutoSize = true,
        Margin = new Padding(8, 10, 0, 0),
        ForeColor = UiTheme.TextMuted
    };
    private readonly Label _detailTitle = new()
    {
        Text = "客户信息展示栏（可滚动，每项信息单独换行）",
        Dock = DockStyle.Top,
        Height = 30,
        Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
        ForeColor = UiTheme.Primary,
        BackColor = Color.White,
        Padding = new Padding(8, 6, 0, 0)
    };

    private readonly TextBox _kw = SearchHelper.CreateBox(320, "关键词，空格=并且，|=或者");
    private readonly ComboBox _fieldScope = SearchHelper.CreateCombo(130,
        "全部字段", "姓名", "客户编号", "电话", "住址", "银行账号", "银行业务");
    private readonly ComboBox _matchMode = SearchHelper.CreateCombo(120, "模糊包含", "精确匹配", "前缀匹配");
    private readonly ComboBox _grade = SearchHelper.CreateCombo(120, "全部等级", "普通", "优质", "高净值");
    private readonly ComboBox _biz = SearchHelper.CreateCombo(150,
        "全部业务", "储蓄", "储蓄+理财", "储蓄+基金", "综合理财", "股票投资", "期货交易", "存款+保险");
    private readonly TextBox _balMin = SearchHelper.CreateBox(120, "余额下限");
    private readonly TextBox _balMax = SearchHelper.CreateBox(120, "余额上限");
    private readonly DateTimePicker _dateFrom = SearchHelper.CreateDatePicker();
    private readonly DateTimePicker _dateTo = SearchHelper.CreateDatePicker();

    private List<Customer> _all = new();
    private List<Customer> _data = new();

    public CustomerPage()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Bg;
        Padding = new Padding(12);
        BuildUi();
        LoadData();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            Padding = new Padding(0)
        };
        // AutoSize：筛选区与操作栏按内容完整增高，避免固定高度裁切
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 58));

        root.Controls.Add(BuildSearchPanel(), 0, 0);
        root.Controls.Add(BuildActionBar(), 0, 1);

        _grid.SelectionChanged += (_, _) => ShowSelectedDetail();
        root.Controls.Add(UiTheme.WrapGrid(_grid), 0, 2);
        root.Controls.Add(BuildBottomSplit(), 0, 3);
        Controls.Add(root);
    }

    private Control BuildSearchPanel()
    {
        var stack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Padding = new Padding(2)
        };

        stack.Controls.Add(new Label
        {
            Text = "联合查询 / 模糊搜索 / 分级筛选 / 时间范围（条件同时生效，控件完整可见）",
            AutoSize = true,
            Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            Margin = new Padding(0, 0, 0, 6)
        });

        var row1 = SearchHelper.CreateFilterRow();
        row1.Controls.Add(SearchHelper.CreateLabel("关键词"));
        row1.Controls.Add(_kw);
        row1.Controls.Add(SearchHelper.CreateLabel("搜索范围"));
        row1.Controls.Add(_fieldScope);
        row1.Controls.Add(SearchHelper.CreateLabel("匹配方式"));
        row1.Controls.Add(_matchMode);
        row1.Controls.Add(SearchHelper.CreateLabel("价值分级"));
        row1.Controls.Add(_grade);
        row1.Controls.Add(SearchHelper.CreateLabel("银行业务"));
        row1.Controls.Add(_biz);

        var row2 = SearchHelper.CreateFilterRow();
        row2.Controls.Add(SearchHelper.CreateLabel("余额区间"));
        row2.Controls.Add(_balMin);
        row2.Controls.Add(SearchHelper.CreateLabel("~"));
        row2.Controls.Add(_balMax);
        row2.Controls.Add(SearchHelper.CreateLabel("开户时间"));
        row2.Controls.Add(_dateFrom);
        row2.Controls.Add(SearchHelper.CreateLabel("至"));
        row2.Controls.Add(_dateTo);
        row2.Controls.Add(MakeBtn("查询", (_, _) => ApplyFilter()));
        row2.Controls.Add(MakeBtn("重置条件", (_, _) => ResetFilters()));

        _kw.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { ApplyFilter(); e.SuppressKeyPress = true; }
        };
        _balMin.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { ApplyFilter(); e.SuppressKeyPress = true; } };
        _balMax.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { ApplyFilter(); e.SuppressKeyPress = true; } };

        stack.Controls.Add(row1);
        stack.Controls.Add(row2);
        return SearchHelper.CreateFilterPanel(stack);
    }

    private Control BuildActionBar()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            AutoScroll = false,
            Padding = new Padding(0, 6, 0, 4),
            MinimumSize = new Size(0, 48)
        };
        toolbar.Controls.Add(MakeBtn("刷新", (_, _) => LoadData()));
        if (Session.IsAdmin)
        {
            toolbar.Controls.Add(MakeBtn("新增", BtnAdd));
            toolbar.Controls.Add(MakeBtn("编辑", BtnEdit));
            toolbar.Controls.Add(MakeBtn("删除", BtnDelete));
        }
        toolbar.Controls.Add(MakeBtn("存款", BtnDeposit));
        toolbar.Controls.Add(MakeBtn("取款", BtnWithdraw));
        toolbar.Controls.Add(MakeBtn("刷新详情", (_, _) => ShowSelectedDetail()));
        DataExportHelper.AppendExportButtons(
            toolbar.Controls,
            () => _grid,
            "客户列表",
            () =>
            {
                var sb = new StringBuilder();
                sb.AppendLine(DataExportHelper.BuildGridSummary(_grid, "客户列表"));
                if (!string.IsNullOrWhiteSpace(_detail.Text))
                {
                    sb.AppendLine();
                    sb.AppendLine("—— 当前客户详情 ——");
                    sb.AppendLine(_detail.Text.Length > 800 ? _detail.Text[..800] + "…" : _detail.Text);
                }
                return sb.ToString();
            });
        toolbar.Controls.Add(_countLabel);
        // 随父容器变宽，换行后高度正确参与 AutoSize 行计算
        toolbar.ParentChanged += (_, _) =>
        {
            if (toolbar.Parent == null) return;
            void Sync() => toolbar.Width = Math.Max(toolbar.Parent.ClientSize.Width - 4, 640);
            toolbar.Parent.Resize += (_, _) => Sync();
            Sync();
        };
        return toolbar;
    }

    private Control BuildBottomSplit()
    {
        var bottom = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            Panel1MinSize = 120,
            Panel2MinSize = 120
        };
        void SafeSplit()
        {
            try
            {
                if (!bottom.IsHandleCreated || bottom.Width <= 260) return;
                var target = Math.Max(bottom.Panel1MinSize, (int)(bottom.Width * 0.48));
                var max = bottom.Width - bottom.Panel2MinSize - bottom.SplitterWidth;
                if (max > bottom.Panel1MinSize)
                    bottom.SplitterDistance = Math.Clamp(target, bottom.Panel1MinSize, max);
            }
            catch { /* 布局未完成 */ }
        }
        bottom.HandleCreated += (_, _) => SafeSplit();
        bottom.SizeChanged += (_, _) => SafeSplit();
        Load += (_, _) => SafeSplit();

        var detailPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8) };
        detailPanel.Controls.Add(_detail);
        detailPanel.Controls.Add(_detailTitle);
        bottom.Panel1.Controls.Add(detailPanel);

        var holdingWrap = UiTheme.WrapGrid(_holdingGrid);
        bottom.Panel2.Controls.Add(holdingWrap);
        bottom.Panel2.Controls.Add(new Label
        {
            Text = "持有产品明细（横向/纵向滚动查看全部）",
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            BackColor = Color.White,
            Padding = new Padding(8, 6, 0, 0)
        });
        return bottom;
    }

    private static Button MakeBtn(string text, EventHandler handler)
        => DataExportHelper.CreateToolButton(text, handler, text.Length > 4 ? 100 : 88);

    private void ResetFilters()
    {
        _kw.Clear();
        _fieldScope.SelectedIndex = 0;
        _matchMode.SelectedIndex = 0;
        _grade.SelectedIndex = 0;
        _biz.SelectedIndex = 0;
        _balMin.Clear();
        _balMax.Clear();
        _dateFrom.Checked = false;
        _dateTo.Checked = false;
        ApplyFilter();
    }

    private void LoadData()
    {
        _all = _svc.GetAll();
        if (Session.IsCustomer && Session.CurrentUser?.CustomerId is int cid)
            _all = _all.Where(c => c.Id == cid).ToList();
        ApplyFilter();
    }

    private SearchHelper.MatchMode CurrentMatchMode() => _matchMode.SelectedIndex switch
    {
        1 => SearchHelper.MatchMode.Exact,
        2 => SearchHelper.MatchMode.Prefix,
        _ => SearchHelper.MatchMode.Fuzzy
    };

    private void ApplyFilter()
    {
        var kw = _kw.Text.Trim();
        var mode = CurrentMatchMode();
        var scope = _fieldScope.SelectedItem?.ToString() ?? "全部字段";
        var grade = _grade.SelectedIndex <= 0 ? null : _grade.SelectedItem?.ToString();
        var biz = _biz.SelectedIndex <= 0 ? null : _biz.SelectedItem?.ToString();
        var balMin = SearchHelper.TryParseDecimal(_balMin.Text);
        var balMax = SearchHelper.TryParseDecimal(_balMax.Text);
        DateTime? from = _dateFrom.Checked ? _dateFrom.Value.Date : null;
        DateTime? to = _dateTo.Checked ? _dateTo.Value.Date : null;

        if (balMin.HasValue && balMax.HasValue && balMin > balMax)
        {
            MessageBox.Show("余额下限不能大于上限。");
            return;
        }
        if (from.HasValue && to.HasValue && from > to)
        {
            MessageBox.Show("开户开始日期不能晚于结束日期。");
            return;
        }

        _data = _all.Where(c =>
        {
            // 分级
            if (grade != null && !string.Equals(c.ValueAssessment, grade, StringComparison.OrdinalIgnoreCase))
                return false;
            // 业务
            if (biz != null && !string.Equals(c.BusinessType, biz, StringComparison.OrdinalIgnoreCase))
                return false;
            // 余额区间
            if (!SearchHelper.InDecimalRange(c.Balance, balMin, balMax))
                return false;
            // 时间
            if (!SearchHelper.InDateRange(c.CreatedAt, from, to))
                return false;
            // 关键词（按范围模糊/精确/前缀）
            if (string.IsNullOrEmpty(kw)) return true;
            return scope switch
            {
                "姓名" => SearchHelper.MatchText(c.Name, kw, mode),
                "客户编号" => SearchHelper.MatchText(c.CustomerNo, kw, mode),
                "电话" => SearchHelper.MatchText(c.Phone, kw, mode),
                "住址" => SearchHelper.MatchText(c.Address, kw, mode),
                "银行账号" => SearchHelper.MatchText(c.AccountNo, kw, mode),
                "银行业务" => SearchHelper.MatchText(c.BusinessType, kw, mode),
                _ => SearchHelper.MatchText(c.CustomerNo, kw, mode)
                    || SearchHelper.MatchText(c.Name, kw, mode)
                    || SearchHelper.MatchText(c.Phone, kw, mode)
                    || SearchHelper.MatchText(c.Address, kw, mode)
                    || SearchHelper.MatchText(c.AccountNo, kw, mode)
                    || SearchHelper.MatchText(c.BusinessType, kw, mode)
                    || SearchHelper.MatchText(c.ValueAssessment, kw, mode)
                    || SearchHelper.MatchText(c.Balance.ToString("N2"), kw, mode)
                    || SearchHelper.MatchText(c.TotalValue.ToString("N2"), kw, mode)
                    || SearchHelper.MatchText(c.CreatedAt.ToString("yyyy-MM-dd"), kw, mode)
            };
        }).ToList();

        _grid.DataSource = _data.Select(c => new
        {
            内部编号 = c.Id,
            客户编号 = c.CustomerNo,
            姓名 = c.Name,
            住址 = c.Address,
            电话 = c.Phone,
            银行账号 = MaskAccount(c.AccountNo),
            账号密码 = Session.IsAdmin ? c.AccountPassword : "******",
            账户余额 = c.Balance.ToString("N2"),
            银行业务 = c.BusinessType,
            价值分级 = c.ValueAssessment,
            总账号价值 = c.TotalValue.ToString("N2"),
            开户时间 = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            Id = c.Id
        }).ToList();

        if (_grid.Columns["Id"] != null)
            _grid.Columns["Id"].Visible = false;
        // 保证长文本列完整可见：自动填充 + 可横向滚动
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

        var condParts = new List<string>();
        if (!string.IsNullOrEmpty(kw)) condParts.Add($"关键词「{kw}」");
        if (grade != null) condParts.Add($"分级={grade}");
        if (biz != null) condParts.Add($"业务={biz}");
        if (balMin.HasValue || balMax.HasValue)
            condParts.Add($"余额[{balMin?.ToString("N0") ?? "*"}~{balMax?.ToString("N0") ?? "*"}]");
        if (from.HasValue || to.HasValue)
            condParts.Add($"开户[{from?.ToString("yyyy-MM-dd") ?? "*"}~{to?.ToString("yyyy-MM-dd") ?? "*"}]");

        _countLabel.Text = Session.IsCustomer
            ? "我的账户信息（下方详情栏可滚动查看完整内容）"
            : condParts.Count == 0
                ? $"共 {_data.Count} 名客户 · 列表与详情均可滚动 · 选中一行查看完整信息"
                : $"联合查询命中 {_data.Count} / {_all.Count} 名 · " + string.Join(" · ", condParts);

        ShowSelectedDetail();
    }

    private static string MaskAccount(string acc)
    {
        if (Session.IsAdmin) return acc;
        if (string.IsNullOrEmpty(acc) || acc.Length <= 8) return acc;
        return acc[..4] + "****" + acc[^4..];
    }

    private Customer? Selected()
    {
        if (_grid.CurrentRow == null) return null;
        var idObj = _grid.CurrentRow.Cells["Id"].Value;
        if (idObj == null) return null;
        var id = Convert.ToInt32(idObj);
        return _data.FirstOrDefault(c => c.Id == id);
    }

    private void ShowSelectedDetail()
    {
        var c = Selected();
        if (c == null)
        {
            _detail.Text =
                "请在上方列表中选择一位客户。" + Environment.NewLine +
                Environment.NewLine +
                "可用上方联合查询：模糊关键词、价值分级、业务类型、余额区间、开户时间。" + Environment.NewLine +
                "关键词支持：空格表示并且，| 或 、 表示或者。";
            _holdingGrid.DataSource = null;
            return;
        }

        c = _svc.GetById(c.Id) ?? c;
        var rate = _svc.GetDepositRate();
        var interest = c.Balance * rate;
        var holdings = _svc.GetHoldings(c.Id);
        var productValue = holdings.Sum(h => h.MarketValue);
        var profitLoss = holdings.Sum(h => h.ProfitLoss);
        var total = c.Balance + interest + productValue;
        var accountNo = Session.IsAdmin ? c.AccountNo : MaskAccount(c.AccountNo);
        var password = Session.IsAdmin ? c.AccountPassword : "******";

        var sb = new StringBuilder();
        sb.AppendLine("========== 基本信息 ==========");
        sb.AppendLine($"内部编号：{c.Id}");
        sb.AppendLine($"客户编号：{c.CustomerNo}");
        sb.AppendLine($"姓名：{c.Name}");
        sb.AppendLine($"住址：{c.Address}");
        sb.AppendLine($"联系电话：{c.Phone}");
        sb.AppendLine($"开户时间：{c.CreatedAt:yyyy年MM月dd日 HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("========== 账户信息 ==========");
        sb.AppendLine($"银行账号：{accountNo}");
        sb.AppendLine($"账号密码：{password}");
        sb.AppendLine($"账户余额：{c.Balance:N2} 元");
        sb.AppendLine($"对应银行业务：{c.BusinessType}");
        sb.AppendLine($"价值分级：{c.ValueAssessment}");
        sb.AppendLine();
        sb.AppendLine("========== 价值计算 ==========");
        sb.AppendLine($"存款本金：{c.Balance:N2} 元");
        sb.AppendLine($"适用年利率：{rate:P2}");
        sb.AppendLine($"预计年利息：{interest:N2} 元");
        sb.AppendLine($"持有产品数量：{holdings.Count} 项");
        sb.AppendLine($"持有产品市值合计：{productValue:N2} 元");
        sb.AppendLine($"持仓浮动盈亏合计：{profitLoss:N2} 元");
        sb.AppendLine($"总账号价值：{total:N2} 元");
        sb.AppendLine($"（计算公式：存款本金 + 预计年利息 + 持有产品市值）");
        sb.AppendLine();
        sb.AppendLine("========== 持有产品明细 ==========");

        if (holdings.Count == 0)
        {
            sb.AppendLine("暂无持仓产品。");
        }
        else
        {
            for (int i = 0; i < holdings.Count; i++)
            {
                var h = holdings[i];
                sb.AppendLine($"【产品 {i + 1}】");
                sb.AppendLine($"  产品名称：{h.ProductName}");
                sb.AppendLine($"  产品类别：{h.Category}");
                sb.AppendLine($"  持有数量：{h.Quantity:N4}");
                sb.AppendLine($"  成本价格：{h.CostPrice:N4}");
                sb.AppendLine($"  当前价格：{h.CurrentPrice:N4}");
                sb.AppendLine($"  当前市值：{h.MarketValue:N2} 元");
                sb.AppendLine($"  浮动盈亏：{h.ProfitLoss:N2} 元");
                sb.AppendLine($"  参考收益率：{h.YieldRate:P2}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("========== 说明 ==========");
        sb.AppendLine("以上为该客户的完整账户信息，请使用滑条查看全部内容。");

        _detail.Text = sb.ToString();
        _detail.SelectionStart = 0;
        _detail.SelectionLength = 0;
        _detail.ScrollToCaret();
        _detailTitle.Text = $"客户信息展示栏 — {c.Name}（{c.CustomerNo}）· 可滚动 · 每项换行显示";

        _holdingGrid.DataSource = holdings.Select(h => new
        {
            产品名称 = h.ProductName,
            类别 = h.Category,
            数量 = h.Quantity.ToString("N4"),
            成本价 = h.CostPrice.ToString("N4"),
            现价 = h.CurrentPrice.ToString("N4"),
            市值 = h.MarketValue.ToString("N2"),
            盈亏 = h.ProfitLoss.ToString("N2"),
            收益率 = h.YieldRate.ToString("P2")
        }).ToList();
        _holdingGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
    }

    private void BtnAdd(object? s, EventArgs e)
    {
        using var dlg = new CustomerEditDialog();
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.Result == null) return;
        _svc.Add(dlg.Result);
        LoadData();
    }

    private void BtnEdit(object? s, EventArgs e)
    {
        var c = Selected();
        if (c == null) { MessageBox.Show("请先选择客户"); return; }
        using var dlg = new CustomerEditDialog(c);
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.Result == null) return;
        _svc.Update(dlg.Result);
        LoadData();
    }

    private void BtnDelete(object? s, EventArgs e)
    {
        var c = Selected();
        if (c == null) { MessageBox.Show("请先选择客户"); return; }
        if (MessageBox.Show($"确认删除客户 {c.Name}？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        _svc.Delete(c.Id);
        LoadData();
    }

    private void BtnDeposit(object? s, EventArgs e)
    {
        var c = Selected();
        if (c == null) { MessageBox.Show("请先选择客户"); return; }
        if (Session.IsCustomer && Session.CurrentUser?.CustomerId != c.Id)
        {
            MessageBox.Show("只能操作自己的账户");
            return;
        }
        var amount = PromptAmount("存款金额");
        if (amount == null) return;
        _svc.Deposit(c.Id, amount.Value);
        LoadData();
    }

    private void BtnWithdraw(object? s, EventArgs e)
    {
        var c = Selected();
        if (c == null) { MessageBox.Show("请先选择客户"); return; }
        if (Session.IsCustomer && Session.CurrentUser?.CustomerId != c.Id)
        {
            MessageBox.Show("只能操作自己的账户");
            return;
        }
        var amount = PromptAmount("取款金额");
        if (amount == null) return;
        try
        {
            _svc.Withdraw(c.Id, amount.Value);
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "取款失败");
        }
    }

    private static decimal? PromptAmount(string title)
    {
        using var f = new Form
        {
            Text = title,
            ClientSize = new Size(320, 140),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };
        var txt = new TextBox { Location = new Point(20, 30), Width = 270 };
        var ok = new Button { Text = "确定", DialogResult = DialogResult.OK, Location = new Point(120, 80), Width = 80 };
        f.Controls.Add(new Label { Text = "请输入金额：", Location = new Point(20, 8), AutoSize = true });
        f.Controls.Add(txt);
        f.Controls.Add(ok);
        f.AcceptButton = ok;
        if (f.ShowDialog() != DialogResult.OK) return null;
        if (!decimal.TryParse(txt.Text, out var v) || v <= 0)
        {
            MessageBox.Show("请输入有效金额");
            return null;
        }
        return v;
    }
}

public class CustomerEditDialog : Form
{
    public Customer? Result { get; private set; }
    private readonly TextBox _no = new();
    private readonly TextBox _name = new();
    private readonly TextBox _addr = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _acc = new();
    private readonly TextBox _pwd = new();
    private readonly TextBox _bal = new();
    private readonly TextBox _biz = new();
    private readonly ComboBox _val = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly int _id;
    private readonly Panel _scroll = new() { Dock = DockStyle.Fill, AutoScroll = true };

    public CustomerEditDialog(Customer? existing = null)
    {
        _id = existing?.Id ?? 0;
        var createdAt = existing?.CreatedAt ?? DateTime.Now;
        Text = existing == null ? "新增客户" : "编辑客户";
        ClientSize = new Size(460, 520);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Font = new Font("Microsoft YaHei UI", 9.5F);

        var fields = new (string, Control)[]
        {
            ("客户编号", _no), ("姓名", _name), ("住址", _addr), ("电话", _phone),
            ("银行账号", _acc), ("账号密码", _pwd), ("余额", _bal), ("银行业务", _biz), ("价值评估", _val)
        };
        _val.Items.AddRange(new object[] { "普通", "优质", "高净值" });
        int y = 16;
        foreach (var (label, ctrl) in fields)
        {
            _scroll.Controls.Add(new Label { Text = label, Location = new Point(20, y), AutoSize = true });
            ctrl.Location = new Point(120, y - 2);
            ctrl.Width = 280;
            _scroll.Controls.Add(ctrl);
            y += 40;
        }
        _scroll.AutoScrollMinSize = new Size(0, y + 20);

        if (existing != null)
        {
            _no.Text = existing.CustomerNo;
            _name.Text = existing.Name;
            _addr.Text = existing.Address;
            _phone.Text = existing.Phone;
            _acc.Text = existing.AccountNo;
            _pwd.Text = existing.AccountPassword;
            _bal.Text = existing.Balance.ToString("F2");
            _biz.Text = existing.BusinessType;
            _val.SelectedItem = existing.ValueAssessment;
        }
        else
        {
            _val.SelectedIndex = 0;
            _bal.Text = "0";
            _pwd.Text = "123456";
        }

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 52 };
        var ok = new Button { Text = "保存", Location = new Point(180, 10), Width = 100 };
        UiTheme.StylePrimaryButton(ok);
        ok.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_no.Text) || string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_acc.Text))
            {
                MessageBox.Show("编号、姓名、账号不能为空");
                return;
            }
            if (!decimal.TryParse(_bal.Text, out var bal))
            {
                MessageBox.Show("余额格式不正确");
                return;
            }
            Result = new Customer
            {
                Id = _id,
                CustomerNo = _no.Text.Trim(),
                Name = _name.Text.Trim(),
                Address = _addr.Text.Trim(),
                Phone = _phone.Text.Trim(),
                AccountNo = _acc.Text.Trim(),
                AccountPassword = _pwd.Text,
                Balance = bal,
                BusinessType = _biz.Text.Trim(),
                ValueAssessment = _val.SelectedItem?.ToString() ?? "普通",
                CreatedAt = createdAt
            };
            DialogResult = DialogResult.OK;
            Close();
        };
        bottom.Controls.Add(ok);
        Controls.Add(_scroll);
        Controls.Add(bottom);
    }
}
