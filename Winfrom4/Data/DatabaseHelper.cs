namespace BankManagementSystem.Data;

using Microsoft.Data.Sqlite;
using System.Data;

public static class DatabaseHelper
{
    private static readonly string DbFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BankManagementSystem");

    public static string DbPath => Path.Combine(DbFolder, "bank.db");

    public static string ConnectionString => $"Data Source={DbPath}";

    public static string LogFolder
    {
        get
        {
            var custom = GetSetting("LogFolder");
            if (!string.IsNullOrWhiteSpace(custom) && Directory.Exists(custom))
                return custom;
            return Path.Combine(DbFolder, "Logs");
        }
    }

    public static SqliteConnection CreateConnection()
    {
        Directory.CreateDirectory(DbFolder);
        Directory.CreateDirectory(Path.Combine(DbFolder, "Logs"));
        return new SqliteConnection(ConnectionString);
    }

    public static void Initialize()
    {
        Directory.CreateDirectory(DbFolder);
        Directory.CreateDirectory(Path.Combine(DbFolder, "Logs"));

        using var conn = CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL,
    Role INTEGER NOT NULL,
    DisplayName TEXT NOT NULL,
    CustomerId INTEGER NULL,
    CreatedAt TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Customers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerNo TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    Address TEXT NOT NULL,
    Phone TEXT NOT NULL,
    AccountNo TEXT NOT NULL UNIQUE,
    AccountPassword TEXT NOT NULL,
    Balance REAL NOT NULL DEFAULT 0,
    BusinessType TEXT NOT NULL DEFAULT '',
    ValueAssessment TEXT NOT NULL DEFAULT '普通',
    CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Products (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    Category TEXT NOT NULL,
    Price REAL NOT NULL,
    YieldRate REAL NOT NULL DEFAULT 0,
    Profit REAL NOT NULL DEFAULT 0,
    Description TEXT NOT NULL DEFAULT '',
    IsActive INTEGER NOT NULL DEFAULT 1,
    UpdatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Holdings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    Quantity REAL NOT NULL,
    CostPrice REAL NOT NULL,
    FOREIGN KEY(CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY(ProductId) REFERENCES Products(Id)
);

CREATE TABLE IF NOT EXISTS InterestRates (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Period TEXT NOT NULL,
    Rate REAL NOT NULL,
    Description TEXT NOT NULL DEFAULT '',
    UpdatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS BankInfo (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BankName TEXT NOT NULL,
    Address TEXT NOT NULL,
    Phone TEXT NOT NULL,
    Description TEXT NOT NULL,
    TotalAssets REAL NOT NULL,
    TotalDeposits REAL NOT NULL,
    TotalLoans REAL NOT NULL,
    CustomerCount INTEGER NOT NULL,
    UpdatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS OperationLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NULL,
    Username TEXT NOT NULL,
    Role TEXT NOT NULL,
    Action TEXT NOT NULL,
    Module TEXT NOT NULL,
    Detail TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    LogFilePath TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS CustomPages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    PageKey TEXT NOT NULL UNIQUE,
    Description TEXT NOT NULL DEFAULT '',
    ButtonsJson TEXT NOT NULL DEFAULT '[]',
    VisibleToAdmin INTEGER NOT NULL DEFAULT 1,
    VisibleToCustomer INTEGER NOT NULL DEFAULT 0,
    VisibleToSuperAdmin INTEGER NOT NULL DEFAULT 1,
    SortOrder INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL,
    ControlKey TEXT NOT NULL DEFAULT 'buttons_only',
    ControlConfigJson TEXT NOT NULL DEFAULT '{}',
    ButtonBarKey TEXT NOT NULL DEFAULT 'btn_none',
    ButtonBarConfigJson TEXT NOT NULL DEFAULT '{}'
);

CREATE TABLE IF NOT EXISTS AppSettings (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL
);
";
        cmd.ExecuteNonQuery();
        EnsureSchemaUpgrades(conn);
        SeedIfEmpty(conn);
        try
        {
            EnsureLargeDemoData(conn);
        }
        catch (Exception ex)
        {
            // 演示数据补齐失败不应阻止程序启动
            System.Diagnostics.Debug.WriteLine("EnsureLargeDemoData: " + ex);
        }
    }

    /// <summary>兼容旧库：为 CustomPages 增加用户控件相关字段。</summary>
    private static void EnsureSchemaUpgrades(SqliteConnection conn)
    {
        var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info(CustomPages)";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                cols.Add(reader.GetString(1));
        }

        void AddCol(string name, string ddl)
        {
            if (cols.Contains(name)) return;
            using var alter = conn.CreateCommand();
            alter.CommandText = ddl;
            alter.ExecuteNonQuery();
        }

        AddCol("ControlKey", "ALTER TABLE CustomPages ADD COLUMN ControlKey TEXT NOT NULL DEFAULT 'buttons_only'");
        AddCol("ControlConfigJson", "ALTER TABLE CustomPages ADD COLUMN ControlConfigJson TEXT NOT NULL DEFAULT '{}'");
        AddCol("ButtonBarKey", "ALTER TABLE CustomPages ADD COLUMN ButtonBarKey TEXT NOT NULL DEFAULT 'btn_none'");
        AddCol("ButtonBarConfigJson", "ALTER TABLE CustomPages ADD COLUMN ButtonBarConfigJson TEXT NOT NULL DEFAULT '{}'");
    }

    private static readonly string[] Surnames =
    {
        "张", "李", "王", "刘", "陈", "杨", "赵", "黄", "周", "吴",
        "徐", "孙", "胡", "朱", "高", "林", "何", "郭", "马", "罗"
    };

    private static readonly string[] GivenNames =
    {
        "伟", "芳", "娜", "敏", "静", "丽", "强", "磊", "军", "洋",
        "勇", "艳", "杰", "涛", "明", "超", "秀英", "霞", "平", "刚",
        "桂英", "建华", "文", "辉", "力", "飞", "鹏", "婷", "浩", "宇"
    };

    private static readonly string[] Cities =
    {
        "北京市朝阳区", "上海市浦东新区", "广州市天河区", "深圳市南山区", "杭州市西湖区",
        "成都市武侯区", "武汉市江汉区", "南京市鼓楼区", "重庆市渝中区", "西安市雁塔区",
        "苏州市工业园区", "天津市和平区", "青岛市市南区", "长沙市岳麓区", "郑州市金水区"
    };

    private static readonly string[] BusinessTypes =
    {
        "储蓄", "储蓄+理财", "储蓄+基金", "综合理财", "股票投资", "期货交易", "存款+保险"
    };

    private static readonly string[] Assessments = { "普通", "优质", "高净值" };

    private static void SeedIfEmpty(SqliteConnection conn)
    {
        using var check = conn.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Users";
        var count = Convert.ToInt64(check.ExecuteScalar());
        if (count > 0) return;

        using var tx = conn.BeginTransaction();
        var now = DateTime.Now.ToString("o");

        Exec(conn, tx, @"INSERT INTO BankInfo (BankName, Address, Phone, Description, TotalAssets, TotalDeposits, TotalLoans, CustomerCount, UpdatedAt)
VALUES ('华夏示范银行', '北京市朝阳区金融大街88号', '400-800-8888', '数字化综合金融服务银行，提供存款、理财、基金、期货、股票等多元化产品。', 12500000000, 8200000000, 5600000000, 100, $now)",
            ("$now", now));

        SeedAdminUsers(conn, tx, now);
        SeedCustomerBatch(conn, tx, 1, 100, now);
        SeedProductBatch(conn, tx, 1, 100, now);

        // 为全部客户写入持仓明细（基于 100 种产品）
        EnsureHoldingsForAllCustomers(conn, tx);

        var rates = new[]
        {
            ("活期存款利率", "活期", 0.0025m, "人民币活期存款"),
            ("三个月定期", "3个月", 0.011m, "整存整取"),
            ("一年期定期", "1年", 0.015m, "整存整取"),
            ("三年期定期", "3年", 0.022m, "整存整取"),
            ("贷款基准利率", "1年期", 0.036m, "人民币贷款"),
            ("住房贷款利率", "5年以上", 0.039m, "个人住房贷款")
        };
        foreach (var r in rates)
        {
            Exec(conn, tx, @"INSERT INTO InterestRates (Name, Period, Rate, Description, UpdatedAt)
VALUES ($n, $p, $r, $d, $at)",
                ("$n", r.Item1), ("$p", r.Item2), ("$r", r.Item3), ("$d", r.Item4), ("$at", now));
        }

        Exec(conn, tx, "INSERT INTO AppSettings (Key, Value) VALUES ('LogFolder', $v)",
            ("$v", Path.Combine(DbFolder, "Logs")));
        Exec(conn, tx, "INSERT INTO AppSettings (Key, Value) VALUES ('BankShortName', '华夏示范银行')");
        Exec(conn, tx, "INSERT INTO AppSettings (Key, Value) VALUES ('EnableFileLog', '1')");
        Exec(conn, tx, "INSERT INTO AppSettings (Key, Value) VALUES ('IdleTimeoutSeconds', '300')");
        Exec(conn, tx, "INSERT INTO AppSettings (Key, Value) VALUES ('DemoDataVersion', '5')");

        tx.Commit();
    }

    /// <summary>已有旧库时补齐：管理员、100 客户、100 产品、全部客户持仓。</summary>
    private static void EnsureLargeDemoData(SqliteConnection conn)
    {
        using var tx = conn.BeginTransaction();
        var now = DateTime.Now.ToString("o");
        SeedAdminUsers(conn, tx, now);
        SeedCustomerBatch(conn, tx, 1, 100, now);
        SeedProductBatch(conn, tx, 1, 100, now);
        EnsureHoldingsForAllCustomers(conn, tx);

        Exec(conn, tx, "UPDATE BankInfo SET CustomerCount = (SELECT COUNT(*) FROM Customers), UpdatedAt=$at",
            ("$at", now));
        Exec(conn, tx, @"INSERT INTO AppSettings (Key, Value) VALUES ('DemoDataVersion', '5')
ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value");
        tx.Commit();
    }

    private static readonly string[] ProductCategories = { "基金", "期货", "股票", "理财", "存款" };

    private static readonly string[] FundThemes =
    {
        "稳健增长", "科技创新", "消费升级", "新能源", "医疗健康", "人工智能", "制造业升级", "红利优选",
        "量化对冲", "债券增强", "指数增强", "价值精选", "成长先锋", "区域发展", "绿色低碳"
    };

    private static readonly string[] FutureNames =
    {
        "沪铜", "螺纹钢", "铁矿石", "原油", "黄金", "白银", "豆粕", "玉米", "橡胶", "焦炭",
        "玻璃", "棉花", "白糖", "甲醇", "PTA"
    };

    private static readonly string[] StockNames =
    {
        "华夏银行", "工商银行", "建设银行", "招商银行", "贵州茅台", "五粮液", "宁德时代", "比亚迪",
        "中国平安", "中国人寿", "万科A", "保利发展", "中国中车", "中国电信", "中国移动",
        "海康威视", "立讯精密", "隆基绿能", "阳光电源", "药明康德", "恒瑞医药", "美的集团",
        "格力电器", "海尔智家", "伊利股份", "双汇发展", "三一重工", "中联重科", "中信证券", "东方财富"
    };

    private static readonly string[] WealthPeriods =
    {
        "30天", "60天", "90天", "180天", "270天", "一年期", "十八个月", "灵活申赎"
    };

    private static readonly string[] DepositTypes =
    {
        "活期宝", "整存整取三个月", "整存整取半年", "整存整取一年", "整存整取两年",
        "整存整取三年", "通知存款一天", "通知存款七天", "大额存单", "零存整取"
    };

    /// <summary>补齐 PROD001～PROD100 共 100 种不同产品。</summary>
    private static void SeedProductBatch(SqliteConnection conn, SqliteTransaction tx, int fromInclusive, int toInclusive, string now)
    {
        for (int i = fromInclusive; i <= toInclusive; i++)
        {
            var code = $"PROD{i:D3}";
            using (var exists = conn.CreateCommand())
            {
                exists.Transaction = tx;
                exists.CommandText = "SELECT COUNT(*) FROM Products WHERE Code=$c";
                exists.Parameters.AddWithValue("$c", code);
                if (Convert.ToInt64(exists.ExecuteScalar()) > 0) continue;
            }

            string category;
            string name;
            decimal price;
            decimal yield;
            decimal profit;
            string desc;

            // 25 基金 + 20 期货 + 30 股票 + 15 理财 + 10 存款
            if (i <= 25)
            {
                category = "基金";
                var theme = FundThemes[(i - 1) % FundThemes.Length];
                name = $"{theme}混合基金{i:D2}号";
                price = 0.85m + (i % 40) * 0.05m;
                yield = 0.025m + (i % 20) * 0.005m;
                profit = 80000m + i * 12500m;
                desc = $"公募{theme}主题基金，风险等级 R{(i % 4) + 1}";
            }
            else if (i <= 45)
            {
                category = "期货";
                var fut = FutureNames[(i - 26) % FutureNames.Length];
                name = $"{fut}主力合约{i:D2}";
                price = fut is "黄金" or "白银" or "原油" or "沪铜"
                    ? 2000m + (i % 50) * 120m
                    : 800m + (i % 30) * 85m;
                yield = 0.015m + (i % 15) * 0.003m;
                profit = 50000m + i * 8000m;
                desc = $"{fut}商品期货合约，适合风险承受能力较高的客户";
            }
            else if (i <= 75)
            {
                category = "股票";
                var stk = StockNames[(i - 46) % StockNames.Length];
                name = $"{stk}（{i:D2}）";
                price = stk.Contains("茅台") || stk.Contains("五粮液")
                    ? 800m + (i % 20) * 40m
                    : 5m + (i % 80) * 1.25m;
                yield = 0.02m + (i % 25) * 0.004m;
                profit = 100000m + i * 15000m;
                desc = $"A股标的 {stk}，行业龙头/优质蓝筹配置";
            }
            else if (i <= 90)
            {
                category = "理财";
                var period = WealthPeriods[(i - 76) % WealthPeriods.Length];
                name = $"安享理财{period}-{i:D2}";
                price = 1.00m;
                yield = 0.018m + (i % 12) * 0.002m;
                profit = 40000m + i * 6000m;
                desc = $"银行理财产品，期限{period}，中低风险";
            }
            else
            {
                category = "存款";
                var dep = DepositTypes[(i - 91) % DepositTypes.Length];
                name = $"{dep}产品{i:D2}";
                price = 1.00m;
                yield = 0.005m + (i % 10) * 0.002m;
                profit = 0m;
                desc = $"存款类产品：{dep}，本金保障型";
            }

            Exec(conn, tx, @"INSERT INTO Products (Code, Name, Category, Price, YieldRate, Profit, Description, IsActive, UpdatedAt)
VALUES ($c, $n, $cat, $price, $y, $profit, $d, 1, $at)",
                ("$c", code), ("$n", name), ("$cat", category), ("$price", (double)price),
                ("$y", (double)yield), ("$profit", (double)profit), ("$d", desc), ("$at", now));
        }
    }

    /// <summary>基于产品库为每位客户配置 5～8 条持仓明细（优先覆盖 PROD 系列）。</summary>
    private static void EnsureHoldingsForAllCustomers(SqliteConnection conn, SqliteTransaction tx)
    {
        var products = new List<(int Id, double Price)>();
        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = tx;
            // 优先使用 PROD 开头的 100 种产品
            cmd.CommandText = @"SELECT Id, Price FROM Products
WHERE IsActive=1 AND Code LIKE 'PROD%'
ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                products.Add((reader.GetInt32(0), reader.GetDouble(1)));
        }

        if (products.Count == 0)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "SELECT Id, Price FROM Products WHERE IsActive=1 ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                products.Add((reader.GetInt32(0), reader.GetDouble(1)));
        }
        if (products.Count == 0) return;

        var customerIds = new List<int>();
        using (var cmd = conn.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = "SELECT Id FROM Customers ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                customerIds.Add(reader.GetInt32(0));
        }

        foreach (var cid in customerIds)
        {
            var owned = new HashSet<int>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "SELECT ProductId FROM Holdings WHERE CustomerId=$cid";
                cmd.Parameters.AddWithValue("$cid", cid);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    owned.Add(reader.GetInt32(0));
            }

            var prodIdSet = products.Select(p => p.Id).ToHashSet();
            int prodHeld = owned.Count(id => prodIdSet.Contains(id));

            // 每位客户至少持有 5～8 项来自 100 种产品库的仓位
            int target = 5 + (cid % 4);
            int added = 0;
            for (int k = 0; prodHeld < target && k < products.Count * 2; k++)
            {
                var p = products[(cid * 7 + k * 3) % products.Count];
                if (!owned.Add(p.Id)) continue;

                double qty = p.Price >= 100
                    ? Math.Max(1, 3 + (cid % 25) + added * 2)
                    : 200 + cid * 13 + added * 80 + (p.Id % 50) * 5;
                double cost = Math.Max(0.01, p.Price * (0.86 + (cid % 12) * 0.01));

                Exec(conn, tx,
                    "INSERT INTO Holdings (CustomerId, ProductId, Quantity, CostPrice) VALUES ($cid, $pid, $q, $cp)",
                    ("$cid", cid), ("$pid", p.Id), ("$q", qty), ("$cp", cost));
                added++;
                prodHeld++;
            }
        }
    }

    private static void SeedAdminUsers(SqliteConnection conn, SqliteTransaction tx, string now)
    {
        // 超级管理员 + 多名管理员（已存在则跳过）
        var admins = new (string User, string Pwd, int Role, string Name)[]
        {
            ("superadmin", "admin123", 2, "超级管理员"),
            ("superadmin2", "admin123", 2, "超级管理员二号"),
            ("superadmin3", "admin123", 2, "超级管理员三号"),
            ("admin", "admin123", 1, "系统管理员"),
            ("admin2", "admin123", 1, "业务管理员"),
            ("admin3", "admin123", 1, "风控管理员"),
            ("admin4", "admin123", 1, "产品管理员"),
            ("admin5", "admin123", 1, "客户管理员"),
            ("admin6", "admin123", 1, "信贷管理员"),
            ("admin7", "admin123", 1, "运营管理员"),
            ("admin8", "admin123", 1, "合规管理员"),
            ("manager1", "admin123", 1, "分行经理甲"),
            ("manager2", "admin123", 1, "分行经理乙"),
            ("manager3", "admin123", 1, "分行经理丙"),
            ("auditor", "admin123", 1, "审计管理员"),
            ("teller1", "admin123", 1, "柜面主管甲"),
            ("teller2", "admin123", 1, "柜面主管乙")
        };

        foreach (var a in admins)
        {
            using var exists = conn.CreateCommand();
            exists.Transaction = tx;
            exists.CommandText = "SELECT COUNT(*) FROM Users WHERE Username=$u";
            exists.Parameters.AddWithValue("$u", a.User);
            if (Convert.ToInt64(exists.ExecuteScalar()) > 0) continue;

            Exec(conn, tx, @"INSERT INTO Users (Username, Password, Role, DisplayName, CustomerId, CreatedAt, IsActive)
VALUES ($u, $p, $r, $d, NULL, $at, 1)",
                ("$u", a.User), ("$p", a.Pwd), ("$r", a.Role), ("$d", a.Name), ("$at", now));
        }
    }

    private static void SeedCustomerBatch(SqliteConnection conn, SqliteTransaction tx, int fromInclusive, int toInclusive, string now)
    {
        // 兼容旧演示账号命名
        var special = new Dictionary<int, (string User, string Name)>
        {
            [1] = ("zhangsan", "张三"),
            [2] = ("lisi", "李四"),
            [3] = ("wangwu", "王五")
        };

        for (int i = fromInclusive; i <= toInclusive; i++)
        {
            string name;
            string username;
            if (special.TryGetValue(i, out var sp))
            {
                name = sp.Name;
                username = sp.User;
            }
            else
            {
                name = Surnames[(i - 1) % Surnames.Length] + GivenNames[(i * 3) % GivenNames.Length];
                if (i > 20 && (i % 7 == 0))
                    name += GivenNames[(i * 5) % GivenNames.Length];
                username = $"user{i:D3}";
            }

            var city = Cities[(i - 1) % Cities.Length];
            var address = $"{city}示例路{i}号";
            var phone = $"13{(i % 10)}{i:D8}"[..11];
            var customerNo = $"C2026{i:D4}";
            var accountNo = $"62220210{i:D8}";
            var balance = 5000m + (i * 1379 % 500000);
            var biz = BusinessTypes[(i - 1) % BusinessTypes.Length];
            var assess = Assessments[(i - 1) % Assessments.Length];

            // 客户资料（OR IGNORE 避免唯一约束中断整段事务）
            using (var exists = conn.CreateCommand())
            {
                exists.Transaction = tx;
                exists.CommandText = "SELECT COUNT(*) FROM Customers WHERE CustomerNo=$no OR AccountNo=$acc";
                exists.Parameters.AddWithValue("$no", customerNo);
                exists.Parameters.AddWithValue("$acc", accountNo);
                if (Convert.ToInt64(exists.ExecuteScalar()) == 0)
                {
                    Exec(conn, tx, @"INSERT INTO Customers (CustomerNo, Name, Address, Phone, AccountNo, AccountPassword, Balance, BusinessType, ValueAssessment, CreatedAt)
VALUES ($no, $name, $addr, $phone, $acc, '123456', $bal, $biz, $val, $at)",
                        ("$no", customerNo), ("$name", name), ("$addr", address), ("$phone", phone),
                        ("$acc", accountNo), ("$bal", (double)balance), ("$biz", biz), ("$val", assess), ("$at", now));
                }
            }

            object? idObj;
            using (var idCmd = conn.CreateCommand())
            {
                idCmd.Transaction = tx;
                idCmd.CommandText = "SELECT Id FROM Customers WHERE CustomerNo=$no";
                idCmd.Parameters.AddWithValue("$no", customerNo);
                idObj = idCmd.ExecuteScalar();
            }
            if (idObj == null || idObj is DBNull) continue;
            var customerId = Convert.ToInt64(idObj);

            using (var exists = conn.CreateCommand())
            {
                exists.Transaction = tx;
                exists.CommandText = "SELECT COUNT(*) FROM Users WHERE Username=$u";
                exists.Parameters.AddWithValue("$u", username);
                if (Convert.ToInt64(exists.ExecuteScalar()) > 0) continue;

                Exec(conn, tx, @"INSERT INTO Users (Username, Password, Role, DisplayName, CustomerId, CreatedAt, IsActive)
VALUES ($u, '123456', 0, $d, $cid, $at, 1)",
                    ("$u", username), ("$d", name), ("$cid", customerId), ("$at", now));
            }
        }
    }

    private static void Exec(SqliteConnection conn, SqliteTransaction tx, string sql, params (string Name, object Value)[] args)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        foreach (var (name, value) in args)
            cmd.Parameters.AddWithValue(name, value);
        cmd.ExecuteNonQuery();
    }

    public static string GetSetting(string key, string defaultValue = "")
    {
        try
        {
            using var conn = CreateConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Value FROM AppSettings WHERE Key = $k";
            cmd.Parameters.AddWithValue("$k", key);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    public static void SetSetting(string key, string value)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO AppSettings (Key, Value) VALUES ($k, $v)
ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value";
        cmd.Parameters.AddWithValue("$k", key);
        cmd.Parameters.AddWithValue("$v", value);
        cmd.ExecuteNonQuery();
    }

    public static DataTable QueryTable(string sql, params (string Name, object Value)[] args)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in args)
            cmd.Parameters.AddWithValue(name, value);
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }
}
