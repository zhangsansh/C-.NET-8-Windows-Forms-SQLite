namespace BankManagementSystem.Pages;

using BankManagementSystem.Services;
using BankManagementSystem.Utils;

public class HomePage : UserControl
{
    private readonly BankService _bank = new();
    private readonly ProductService _products = new();
    private readonly CustomerService _customers = new();

    public HomePage()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Bg;
        Load += (_, _) => Build();
    }

    private void Build()
    {
        Controls.Clear();
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = false };
        DataGridView? rateGridRef = null;
        DataGridView? prodGridRef = null;

        toolbar.Controls.Add(DataExportHelper.CreateToolButton("刷新", (_, _) => Build(), 80));
        toolbar.Controls.Add(DataExportHelper.CreateToolButton("导出CSV(利率)", (_, _) =>
        {
            if (rateGridRef == null) return;
            DataExportHelper.ExportToCsv(rateGridRef, "主页利率");
        }, 120));
        toolbar.Controls.Add(DataExportHelper.CreateToolButton("导出表格(利率)", (_, _) =>
        {
            if (rateGridRef == null) return;
            DataExportHelper.ExportToExcelTable(rateGridRef, "主页利率");
        }, 120));
        toolbar.Controls.Add(DataExportHelper.CreateToolButton("导出CSV(产品)", (_, _) =>
        {
            if (prodGridRef == null) return;
            DataExportHelper.ExportToCsv(prodGridRef, "主页产品");
        }, 120));
        toolbar.Controls.Add(DataExportHelper.CreateToolButton("导出表格(产品)", (_, _) =>
        {
            if (prodGridRef == null) return;
            DataExportHelper.ExportToExcelTable(prodGridRef, "主页产品");
        }, 120));
        toolbar.Controls.Add(DataExportHelper.CreateToolButton("复制摘要", (_, _) =>
        {
            var text = rateGridRef != null
                ? DataExportHelper.BuildGridSummary(rateGridRef, "主页利率") + "\n" +
                  (prodGridRef != null ? DataExportHelper.BuildGridSummary(prodGridRef, "主页产品") : "")
                : "暂无数据";
            DataExportHelper.CopyText(text, "主页");
        }, 100));
        toolbar.Controls.Add(DataExportHelper.CreateToolButton("打印预览(产品)", (_, _) =>
        {
            if (prodGridRef == null) return;
            DataExportHelper.ShowPrintPreview(prodGridRef, "主页产品");
        }, 130));
        toolbar.Controls.Add(DataExportHelper.CreateToolButton("打开目录", (_, _) =>
            DataExportHelper.OpenDirectory(DataExportHelper.ExportFolder), 100));

        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(16),
            AutoScrollMinSize = new Size(1000, 860)
        };

        var info = _bank.GetBankInfo();
        var rates = _bank.GetRates();
        var products = _products.GetAll(true);
        var customers = Session.IsCustomer && Session.CurrentUser?.CustomerId is int cid
            ? _customers.GetAll().Where(c => c.Id == cid).ToList()
            : _customers.GetAll();

        var header = new Label
        {
            Text = !Session.IsLoggedIn ? "银行公开信息（请先登录）"
                : Session.IsCustomer ? "我的银行概览" : "银行经营总览",
            Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            AutoSize = true,
            Location = new Point(8, 8)
        };
        scroll.Controls.Add(header);

        var cards = new FlowLayoutPanel
        {
            Location = new Point(8, 50),
            Width = 980,
            Height = 160,
            AutoSize = false,
            WrapContents = false
        };

        if (info != null)
        {
            if (!Session.IsLoggedIn || Session.IsAdmin)
            {
                cards.Controls.Add(StatCard("总资产", $"{info.TotalAssets / 100000000m:N2} 亿元", 220));
                cards.Controls.Add(StatCard("存款余额", $"{info.TotalDeposits / 100000000m:N2} 亿元", 220));
                cards.Controls.Add(StatCard("贷款余额", $"{info.TotalLoans / 100000000m:N2} 亿元", 220));
                cards.Controls.Add(StatCard("客户数量", info.CustomerCount.ToString(), 180));
            }
            else
            {
                var me = customers.FirstOrDefault();
                cards.Controls.Add(StatCard("账户余额", me == null ? "-" : $"{me.Balance:N2} 元", 220));
                cards.Controls.Add(StatCard("账户总价值", me == null ? "-" : $"{me.TotalValue:N2} 元", 240));
                cards.Controls.Add(StatCard("价值评估", me?.ValueAssessment ?? "-", 180));
                cards.Controls.Add(StatCard("业务类型", me?.BusinessType ?? "-", 220));
            }
        }
        scroll.Controls.Add(cards);

        var bankPanel = new Panel
        {
            Location = new Point(8, 220),
            Size = new Size(960, 110),
            BackColor = Color.White,
            Padding = new Padding(12)
        };
        bankPanel.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(220, 225, 230));
            e.Graphics.DrawRectangle(pen, 0, 0, bankPanel.Width - 1, bankPanel.Height - 1);
        };
        if (info != null)
        {
            bankPanel.Controls.Add(new Label
            {
                Text = $"{info.BankName}  |  {info.Address}  |  客服 {info.Phone}",
                Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
                ForeColor = UiTheme.Primary,
                AutoSize = true,
                Location = new Point(12, 12)
            });
            bankPanel.Controls.Add(new Label
            {
                Text = info.Description,
                ForeColor = UiTheme.TextMuted,
                AutoSize = false,
                Size = new Size(920, 50),
                Location = new Point(12, 45)
            });
        }
        scroll.Controls.Add(bankPanel);

        var rateTitle = new Label
        {
            Text = "利率信息",
            Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            Location = new Point(8, 350),
            AutoSize = true
        };
        scroll.Controls.Add(rateTitle);

        var rateGrid = new DataGridView { Location = new Point(8, 380), Size = new Size(960, 200), ScrollBars = ScrollBars.Both };
        UiTheme.StyleDataGrid(rateGrid);
        rateGrid.DataSource = rates.Select(r => new
        {
            名称 = r.Name,
            期限 = r.Period,
            利率 = $"{r.Rate:P2}",
            说明 = r.Description
        }).ToList();
        rateGridRef = rateGrid;
        scroll.Controls.Add(rateGrid);

        var prodTitle = new Label
        {
            Text = Session.IsCustomer ? "可购银行产品" : "产品盈利一览（基金 / 期货 / 股票 / 理财）",
            Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            Location = new Point(8, 600),
            AutoSize = true
        };
        scroll.Controls.Add(prodTitle);

        var prodGrid = new DataGridView { Location = new Point(8, 630), Size = new Size(960, 260), ScrollBars = ScrollBars.Both };
        UiTheme.StyleDataGrid(prodGrid);
        prodGrid.DataSource = products.Select(p => new
        {
            代码 = p.Code,
            名称 = p.Name,
            类别 = p.Category,
            现价 = p.Price.ToString("N2"),
            收益率 = p.YieldRate.ToString("P2"),
            盈利 = Session.IsAdmin ? p.Profit.ToString("N0") : (Session.IsLoggedIn ? "-" : "-"),
            说明 = p.Description
        }).ToList();
        prodGridRef = prodGrid;
        scroll.Controls.Add(prodGrid);

        root.Controls.Add(toolbar, 0, 0);
        root.Controls.Add(scroll, 0, 1);
        Controls.Add(root);
    }

    private static Panel StatCard(string title, string value, int width)
    {
        var p = new Panel
        {
            Width = width,
            Height = 130,
            Margin = new Padding(0, 0, 12, 0),
            BackColor = Color.White,
            Padding = new Padding(14)
        };
        p.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(220, 225, 230));
            e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        };
        p.Controls.Add(new Label
        {
            Text = title,
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(14, 18)
        });
        p.Controls.Add(new Label
        {
            Text = value,
            Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            AutoSize = true,
            Location = new Point(14, 55)
        });
        return p;
    }
}
