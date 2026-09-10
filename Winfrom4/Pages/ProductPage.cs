namespace BankManagementSystem.Pages;

using BankManagementSystem.Models;
using BankManagementSystem.Services;
using BankManagementSystem.Utils;

public class ProductPage : UserControl
{
    private readonly ProductService _svc = new();
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly Label _countTip = new()
    {
        AutoSize = true,
        Margin = new Padding(8, 10, 0, 0),
        ForeColor = UiTheme.TextMuted,
        Text = "产品列表可滚动查看"
    };

    private readonly TextBox _kw = SearchHelper.CreateBox(320, "关键词，空格=并且，|=或者");
    private readonly ComboBox _fieldScope = SearchHelper.CreateCombo(130,
        "全部字段", "代码", "名称", "类别", "说明");
    private readonly ComboBox _matchMode = SearchHelper.CreateCombo(120, "模糊包含", "精确匹配", "前缀匹配");
    private readonly ComboBox _category = SearchHelper.CreateCombo(120,
        "全部分级", "基金", "期货", "股票", "理财", "存款");
    private readonly ComboBox _status = SearchHelper.CreateCombo(110, "全部状态", "启用", "停用");
    private readonly TextBox _priceMin = SearchHelper.CreateBox(110, "价格下限");
    private readonly TextBox _priceMax = SearchHelper.CreateBox(110, "价格上限");
    private readonly TextBox _yieldMin = SearchHelper.CreateBox(110, "收益率下限");
    private readonly TextBox _yieldMax = SearchHelper.CreateBox(110, "收益率上限");
    private readonly DateTimePicker _dateFrom = SearchHelper.CreateDatePicker();
    private readonly DateTimePicker _dateTo = SearchHelper.CreateDatePicker();

    private List<Product> _all = new();
    private List<Product> _data = new();

    public ProductPage()
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
            RowCount = 3,
            ColumnCount = 1
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildSearchPanel(), 0, 0);
        root.Controls.Add(BuildActionBar(), 0, 1);
        root.Controls.Add(UiTheme.WrapGrid(_grid), 0, 2);
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
            Text = "联合查询 / 模糊搜索 / 产品分级 / 时间范围（条件同时生效，控件完整可见）",
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
        row1.Controls.Add(SearchHelper.CreateLabel("产品分级"));
        row1.Controls.Add(_category);
        row1.Controls.Add(SearchHelper.CreateLabel("状态"));
        row1.Controls.Add(_status);

        var row2 = SearchHelper.CreateFilterRow();
        row2.Controls.Add(SearchHelper.CreateLabel("价格区间"));
        row2.Controls.Add(_priceMin);
        row2.Controls.Add(SearchHelper.CreateLabel("~"));
        row2.Controls.Add(_priceMax);
        row2.Controls.Add(SearchHelper.CreateLabel("收益率"));
        row2.Controls.Add(_yieldMin);
        row2.Controls.Add(SearchHelper.CreateLabel("~"));
        row2.Controls.Add(_yieldMax);
        row2.Controls.Add(SearchHelper.CreateLabel("更新时间"));
        row2.Controls.Add(_dateFrom);
        row2.Controls.Add(SearchHelper.CreateLabel("至"));
        row2.Controls.Add(_dateTo);
        row2.Controls.Add(Btn("查询", (_, _) => ApplyFilter()));
        row2.Controls.Add(Btn("重置条件", (_, _) => ResetFilters()));

        _kw.KeyDown += OnEnterSearch;
        _priceMin.KeyDown += OnEnterSearch;
        _priceMax.KeyDown += OnEnterSearch;
        _yieldMin.KeyDown += OnEnterSearch;
        _yieldMax.KeyDown += OnEnterSearch;

        stack.Controls.Add(row1);
        stack.Controls.Add(row2);
        return SearchHelper.CreateFilterPanel(stack);
    }

    private void OnEnterSearch(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) { ApplyFilter(); e.SuppressKeyPress = true; }
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
        toolbar.Controls.Add(Btn("刷新", (_, _) => LoadData()));
        if (Session.IsAdmin)
        {
            toolbar.Controls.Add(Btn("新增", BtnAdd));
            toolbar.Controls.Add(Btn("编辑", BtnEdit));
            toolbar.Controls.Add(Btn("删除", BtnDelete));
        }
        DataExportHelper.AppendExportButtons(
            toolbar.Controls,
            () => _grid,
            "产品列表",
            () => DataExportHelper.BuildGridSummary(_grid, "产品列表"));
        toolbar.Controls.Add(_countTip);
        toolbar.ParentChanged += (_, _) =>
        {
            if (toolbar.Parent == null) return;
            void Sync() => toolbar.Width = Math.Max(toolbar.Parent.ClientSize.Width - 4, 640);
            toolbar.Parent.Resize += (_, _) => Sync();
            Sync();
        };
        return toolbar;
    }

    private static Button Btn(string t, EventHandler h)
        => DataExportHelper.CreateToolButton(t, h, t.Length > 4 ? 100 : 88);

    private void ResetFilters()
    {
        _kw.Clear();
        _fieldScope.SelectedIndex = 0;
        _matchMode.SelectedIndex = 0;
        _category.SelectedIndex = 0;
        _status.SelectedIndex = 0;
        _priceMin.Clear();
        _priceMax.Clear();
        _yieldMin.Clear();
        _yieldMax.Clear();
        _dateFrom.Checked = false;
        _dateTo.Checked = false;
        ApplyFilter();
    }

    private void LoadData()
    {
        _all = Session.IsAdmin ? _svc.GetAll() : _svc.GetAll(true);
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
        var category = _category.SelectedIndex <= 0 ? null : _category.SelectedItem?.ToString();
        bool? activeOnly = _status.SelectedIndex switch
        {
            1 => true,
            2 => false,
            _ => null
        };
        var priceMin = SearchHelper.TryParseDecimal(_priceMin.Text);
        var priceMax = SearchHelper.TryParseDecimal(_priceMax.Text);
        var yieldMin = SearchHelper.TryParseDecimal(_yieldMin.Text);
        var yieldMax = SearchHelper.TryParseDecimal(_yieldMax.Text);
        DateTime? from = _dateFrom.Checked ? _dateFrom.Value.Date : null;
        DateTime? to = _dateTo.Checked ? _dateTo.Value.Date : null;

        if (priceMin.HasValue && priceMax.HasValue && priceMin > priceMax)
        {
            MessageBox.Show("价格下限不能大于上限。");
            return;
        }
        if (yieldMin.HasValue && yieldMax.HasValue && yieldMin > yieldMax)
        {
            MessageBox.Show("收益率下限不能大于上限。");
            return;
        }
        if (from.HasValue && to.HasValue && from > to)
        {
            MessageBox.Show("更新开始日期不能晚于结束日期。");
            return;
        }

        _data = _all.Where(p =>
        {
            if (category != null && !string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase))
                return false;
            if (activeOnly.HasValue && p.IsActive != activeOnly.Value)
                return false;
            if (!SearchHelper.InDecimalRange(p.Price, priceMin, priceMax))
                return false;
            if (!SearchHelper.InDecimalRange(p.YieldRate, yieldMin, yieldMax))
                return false;
            if (!SearchHelper.InDateRange(p.UpdatedAt, from, to))
                return false;
            if (string.IsNullOrEmpty(kw)) return true;

            return scope switch
            {
                "代码" => SearchHelper.MatchText(p.Code, kw, mode),
                "名称" => SearchHelper.MatchText(p.Name, kw, mode),
                "类别" => SearchHelper.MatchText(p.Category, kw, mode),
                "说明" => SearchHelper.MatchText(p.Description, kw, mode),
                _ => SearchHelper.MatchText(p.Code, kw, mode)
                    || SearchHelper.MatchText(p.Name, kw, mode)
                    || SearchHelper.MatchText(p.Category, kw, mode)
                    || SearchHelper.MatchText(p.Description, kw, mode)
                    || SearchHelper.MatchText(p.Price.ToString("N2"), kw, mode)
                    || SearchHelper.MatchText(p.YieldRate.ToString("P2"), kw, mode)
                    || SearchHelper.MatchText(p.IsActive ? "启用" : "停用", kw, mode)
                    || SearchHelper.MatchText(p.UpdatedAt.ToString("yyyy-MM-dd"), kw, mode)
            };
        }).ToList();

        _grid.DataSource = _data.Select(p => new
        {
            代码 = p.Code,
            名称 = p.Name,
            产品分级 = p.Category,
            价格 = p.Price.ToString("N2"),
            收益率 = p.YieldRate.ToString("P2"),
            盈利 = Session.IsAdmin ? p.Profit.ToString("N2") : "-",
            说明 = p.Description,
            状态 = p.IsActive ? "启用" : "停用",
            更新时间 = p.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            Id = p.Id
        }).ToList();

        if (_grid.Columns["Id"] != null) _grid.Columns["Id"].Visible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

        var condParts = new List<string>();
        if (!string.IsNullOrEmpty(kw)) condParts.Add($"关键词「{kw}」");
        if (category != null) condParts.Add($"分级={category}");
        if (activeOnly.HasValue) condParts.Add(activeOnly.Value ? "启用" : "停用");
        if (priceMin.HasValue || priceMax.HasValue)
            condParts.Add($"价格[{priceMin?.ToString("N2") ?? "*"}~{priceMax?.ToString("N2") ?? "*"}]");
        if (yieldMin.HasValue || yieldMax.HasValue)
            condParts.Add($"收益率[{yieldMin?.ToString("P2") ?? "*"}~{yieldMax?.ToString("P2") ?? "*"}]");
        if (from.HasValue || to.HasValue)
            condParts.Add($"更新[{from?.ToString("yyyy-MM-dd") ?? "*"}~{to?.ToString("yyyy-MM-dd") ?? "*"}]");

        _countTip.Text = condParts.Count == 0
            ? $"共 {_data.Count} 种产品（含 PROD001～PROD100），可横向/纵向滚动查看完整内容"
            : $"联合查询命中 {_data.Count} / {_all.Count} 种 · " + string.Join(" · ", condParts);
    }

    private Product? Selected()
    {
        if (_grid.CurrentRow == null) return null;
        var id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        return _data.FirstOrDefault(p => p.Id == id);
    }

    private void BtnAdd(object? s, EventArgs e)
    {
        using var dlg = new ProductEditDialog();
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.Result == null) return;
        _svc.Add(dlg.Result);
        LoadData();
    }

    private void BtnEdit(object? s, EventArgs e)
    {
        var p = Selected();
        if (p == null) { MessageBox.Show("请选择产品"); return; }
        using var dlg = new ProductEditDialog(p);
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.Result == null) return;
        _svc.Update(dlg.Result);
        LoadData();
    }

    private void BtnDelete(object? s, EventArgs e)
    {
        var p = Selected();
        if (p == null) { MessageBox.Show("请选择产品"); return; }
        if (MessageBox.Show($"确认删除 {p.Name}？", "确认", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        _svc.Delete(p.Id);
        LoadData();
    }
}

public class ProductEditDialog : Form
{
    public Product? Result { get; private set; }
    private readonly TextBox _code = new(), _name = new(), _price = new(), _yield = new(), _profit = new(), _desc = new();
    private readonly ComboBox _cat = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _active = new() { Text = "启用", Checked = true };
    private readonly int _id;

    public ProductEditDialog(Product? existing = null)
    {
        _id = existing?.Id ?? 0;
        Text = existing == null ? "新增产品" : "编辑产品";
        ClientSize = new Size(420, 420);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Font = new Font("Microsoft YaHei UI", 9.5F);
        _cat.Items.AddRange(new object[] { "基金", "期货", "股票", "理财", "存款" });

        var fields = new (string, Control)[]
        {
            ("代码", _code), ("名称", _name), ("类别", _cat), ("价格", _price),
            ("收益率(如0.05)", _yield), ("盈利", _profit), ("说明", _desc)
        };
        int y = 16;
        foreach (var (label, ctrl) in fields)
        {
            Controls.Add(new Label { Text = label, Location = new Point(20, y), AutoSize = true });
            ctrl.Location = new Point(140, y - 2);
            ctrl.Width = 240;
            Controls.Add(ctrl);
            y += 40;
        }
        _active.Location = new Point(140, y);
        Controls.Add(_active);

        if (existing != null)
        {
            _code.Text = existing.Code;
            _name.Text = existing.Name;
            _cat.SelectedItem = existing.Category;
            _price.Text = existing.Price.ToString("F2");
            _yield.Text = existing.YieldRate.ToString("F4");
            _profit.Text = existing.Profit.ToString("F2");
            _desc.Text = existing.Description;
            _active.Checked = existing.IsActive;
        }
        else
        {
            _cat.SelectedIndex = 0;
            _price.Text = "1.00";
            _yield.Text = "0.03";
            _profit.Text = "0";
        }

        var ok = new Button { Text = "保存", Location = new Point(200, 360), Width = 80 };
        UiTheme.StylePrimaryButton(ok);
        ok.Click += (_, _) =>
        {
            if (!decimal.TryParse(_price.Text, out var price) ||
                !decimal.TryParse(_yield.Text, out var yield) ||
                !decimal.TryParse(_profit.Text, out var profit))
            {
                MessageBox.Show("请输入有效数字");
                return;
            }
            Result = new Product
            {
                Id = _id,
                Code = _code.Text.Trim(),
                Name = _name.Text.Trim(),
                Category = _cat.SelectedItem?.ToString() ?? "理财",
                Price = price,
                YieldRate = yield,
                Profit = profit,
                Description = _desc.Text.Trim(),
                IsActive = _active.Checked
            };
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(ok);
    }
}
