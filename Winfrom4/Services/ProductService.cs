namespace BankManagementSystem.Services;

using BankManagementSystem.Data;
using BankManagementSystem.Models;
using Microsoft.Data.Sqlite;

public class ProductService
{
    private readonly LogService _log = new();

    public List<Product> GetAll(bool activeOnly = false)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = activeOnly
            ? "SELECT Id, Code, Name, Category, Price, YieldRate, Profit, Description, IsActive, UpdatedAt FROM Products WHERE IsActive=1 ORDER BY Category, Id"
            : "SELECT Id, Code, Name, Category, Price, YieldRate, Profit, Description, IsActive, UpdatedAt FROM Products ORDER BY Category, Id";
        var list = new List<Product>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public Product? GetById(int id)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Code, Name, Category, Price, YieldRate, Profit, Description, IsActive, UpdatedAt FROM Products WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public void Add(Product p)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Products (Code, Name, Category, Price, YieldRate, Profit, Description, IsActive, UpdatedAt)
VALUES ($code, $name, $cat, $price, $y, $profit, $desc, $active, $at)";
        Bind(cmd, p);
        cmd.ExecuteNonQuery();
        _log.Write("新增产品", "产品管理", $"{p.Code} {p.Name}");
    }

    public void Update(Product p)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE Products SET Code=$code, Name=$name, Category=$cat, Price=$price, YieldRate=$y,
Profit=$profit, Description=$desc, IsActive=$active, UpdatedAt=$at WHERE Id=$id";
        Bind(cmd, p);
        cmd.Parameters.AddWithValue("$id", p.Id);
        cmd.ExecuteNonQuery();
        _log.Write("更新产品", "产品管理", $"{p.Code} {p.Name}");
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Products WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
        _log.Write("删除产品", "产品管理", $"Id={id}");
    }

    private static void Bind(SqliteCommand cmd, Product p)
    {
        cmd.Parameters.AddWithValue("$code", p.Code);
        cmd.Parameters.AddWithValue("$name", p.Name);
        cmd.Parameters.AddWithValue("$cat", p.Category);
        cmd.Parameters.AddWithValue("$price", (double)p.Price);
        cmd.Parameters.AddWithValue("$y", (double)p.YieldRate);
        cmd.Parameters.AddWithValue("$profit", (double)p.Profit);
        cmd.Parameters.AddWithValue("$desc", p.Description);
        cmd.Parameters.AddWithValue("$active", p.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("$at", DateTime.Now.ToString("o"));
    }

    private static Product Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Code = reader.GetString(1),
        Name = reader.GetString(2),
        Category = reader.GetString(3),
        Price = Convert.ToDecimal(reader.GetDouble(4)),
        YieldRate = Convert.ToDecimal(reader.GetDouble(5)),
        Profit = Convert.ToDecimal(reader.GetDouble(6)),
        Description = reader.GetString(7),
        IsActive = reader.GetInt32(8) == 1,
        UpdatedAt = DateTime.Parse(reader.GetString(9))
    };
}

public class BankService
{
    private readonly LogService _log = new();

    public BankInfo? GetBankInfo()
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, BankName, Address, Phone, Description, TotalAssets, TotalDeposits, TotalLoans, CustomerCount, UpdatedAt FROM BankInfo LIMIT 1";
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return new BankInfo
        {
            Id = reader.GetInt32(0),
            BankName = reader.GetString(1),
            Address = reader.GetString(2),
            Phone = reader.GetString(3),
            Description = reader.GetString(4),
            TotalAssets = Convert.ToDecimal(reader.GetDouble(5)),
            TotalDeposits = Convert.ToDecimal(reader.GetDouble(6)),
            TotalLoans = Convert.ToDecimal(reader.GetDouble(7)),
            CustomerCount = reader.GetInt32(8),
            UpdatedAt = DateTime.Parse(reader.GetString(9))
        };
    }

    public void UpdateBankInfo(BankInfo info)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE BankInfo SET BankName=$n, Address=$a, Phone=$p, Description=$d,
TotalAssets=$ta, TotalDeposits=$td, TotalLoans=$tl, CustomerCount=$cc, UpdatedAt=$at WHERE Id=$id";
        cmd.Parameters.AddWithValue("$n", info.BankName);
        cmd.Parameters.AddWithValue("$a", info.Address);
        cmd.Parameters.AddWithValue("$p", info.Phone);
        cmd.Parameters.AddWithValue("$d", info.Description);
        cmd.Parameters.AddWithValue("$ta", (double)info.TotalAssets);
        cmd.Parameters.AddWithValue("$td", (double)info.TotalDeposits);
        cmd.Parameters.AddWithValue("$tl", (double)info.TotalLoans);
        cmd.Parameters.AddWithValue("$cc", info.CustomerCount);
        cmd.Parameters.AddWithValue("$at", DateTime.Now.ToString("o"));
        cmd.Parameters.AddWithValue("$id", info.Id);
        cmd.ExecuteNonQuery();
        _log.Write("更新银行信息", "管理", info.BankName);
    }

    public List<InterestRate> GetRates()
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Period, Rate, Description, UpdatedAt FROM InterestRates ORDER BY Id";
        var list = new List<InterestRate>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new InterestRate
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Period = reader.GetString(2),
                Rate = Convert.ToDecimal(reader.GetDouble(3)),
                Description = reader.GetString(4),
                UpdatedAt = DateTime.Parse(reader.GetString(5))
            });
        }
        return list;
    }

    public void AddRate(InterestRate r)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO InterestRates (Name, Period, Rate, Description, UpdatedAt) VALUES ($n,$p,$r,$d,$at)";
        cmd.Parameters.AddWithValue("$n", r.Name);
        cmd.Parameters.AddWithValue("$p", r.Period);
        cmd.Parameters.AddWithValue("$r", (double)r.Rate);
        cmd.Parameters.AddWithValue("$d", r.Description);
        cmd.Parameters.AddWithValue("$at", DateTime.Now.ToString("o"));
        cmd.ExecuteNonQuery();
        _log.Write("新增利率", "管理", r.Name);
    }

    public void UpdateRate(InterestRate r)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE InterestRates SET Name=$n, Period=$p, Rate=$r, Description=$d, UpdatedAt=$at WHERE Id=$id";
        cmd.Parameters.AddWithValue("$n", r.Name);
        cmd.Parameters.AddWithValue("$p", r.Period);
        cmd.Parameters.AddWithValue("$r", (double)r.Rate);
        cmd.Parameters.AddWithValue("$d", r.Description);
        cmd.Parameters.AddWithValue("$at", DateTime.Now.ToString("o"));
        cmd.Parameters.AddWithValue("$id", r.Id);
        cmd.ExecuteNonQuery();
        _log.Write("更新利率", "管理", r.Name);
    }

    public void DeleteRate(int id)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM InterestRates WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
        _log.Write("删除利率", "管理", $"Id={id}");
    }

    public void RefreshCustomerCount()
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE BankInfo SET CustomerCount = (SELECT COUNT(*) FROM Customers), UpdatedAt=$at";
        cmd.Parameters.AddWithValue("$at", DateTime.Now.ToString("o"));
        cmd.ExecuteNonQuery();
    }
}

public class UserService
{
    private readonly LogService _log = new();

    public List<User> GetAll()
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, Password, Role, DisplayName, CustomerId, CreatedAt, IsActive FROM Users ORDER BY Role DESC, Id";
        var list = new List<User>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Password = reader.GetString(2),
                Role = (UserRole)reader.GetInt32(3),
                DisplayName = reader.GetString(4),
                CustomerId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                CreatedAt = DateTime.Parse(reader.GetString(6)),
                IsActive = reader.GetInt32(7) == 1
            });
        }
        return list;
    }

    public void Add(User u)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Users (Username, Password, Role, DisplayName, CustomerId, CreatedAt, IsActive)
VALUES ($u,$p,$r,$d,$c,$at,$a)";
        cmd.Parameters.AddWithValue("$u", u.Username);
        cmd.Parameters.AddWithValue("$p", u.Password);
        cmd.Parameters.AddWithValue("$r", (int)u.Role);
        cmd.Parameters.AddWithValue("$d", u.DisplayName);
        cmd.Parameters.AddWithValue("$c", (object?)u.CustomerId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$at", DateTime.Now.ToString("o"));
        cmd.Parameters.AddWithValue("$a", u.IsActive ? 1 : 0);
        cmd.ExecuteNonQuery();
        _log.Write("新增用户", "用户管理", $"{u.Username} ({u.Role})");
    }

    public void Update(User u)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE Users SET Username=$u, Password=$p, Role=$r, DisplayName=$d, CustomerId=$c, IsActive=$a WHERE Id=$id";
        cmd.Parameters.AddWithValue("$u", u.Username);
        cmd.Parameters.AddWithValue("$p", u.Password);
        cmd.Parameters.AddWithValue("$r", (int)u.Role);
        cmd.Parameters.AddWithValue("$d", u.DisplayName);
        cmd.Parameters.AddWithValue("$c", (object?)u.CustomerId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$a", u.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", u.Id);
        cmd.ExecuteNonQuery();
        _log.Write("更新用户", "用户管理", $"{u.Username}");
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
        _log.Write("删除用户", "用户管理", $"Id={id}");
    }
}

public class CustomPageService
{
    private readonly LogService _log = new();

    public List<CustomPage> GetAll()
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Title, PageKey, Description, ButtonsJson, VisibleToAdmin, VisibleToCustomer, VisibleToSuperAdmin, SortOrder, CreatedAt, IFNULL(ControlKey,'buttons_only'), IFNULL(ControlConfigJson,'{}'), IFNULL(ButtonBarKey,'btn_none'), IFNULL(ButtonBarConfigJson,'{}') FROM CustomPages ORDER BY SortOrder, Id";
        var list = new List<CustomPage>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    public List<CustomPage> GetVisibleFor(UserRole role)
    {
        return GetAll().Where(p => role switch
        {
            UserRole.SuperAdmin => p.VisibleToSuperAdmin,
            UserRole.Admin => p.VisibleToAdmin,
            UserRole.Customer => p.VisibleToCustomer,
            _ => false
        }).ToList();
    }

    public void Add(CustomPage page)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO CustomPages (Title, PageKey, Description, ButtonsJson, VisibleToAdmin, VisibleToCustomer, VisibleToSuperAdmin, SortOrder, CreatedAt, ControlKey, ControlConfigJson, ButtonBarKey, ButtonBarConfigJson)
VALUES ($t,$k,$d,$b,$va,$vc,$vs,$s,$at,$ck,$cc,$bk,$bc)";
        Bind(cmd, page);
        cmd.ExecuteNonQuery();
        _log.Write("新增自定义页面", "页面扩展", $"{page.Title} / {page.ControlKey} / {page.ButtonBarKey}");
    }

    public void Update(CustomPage page)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE CustomPages SET Title=$t, PageKey=$k, Description=$d, ButtonsJson=$b,
VisibleToAdmin=$va, VisibleToCustomer=$vc, VisibleToSuperAdmin=$vs, SortOrder=$s,
ControlKey=$ck, ControlConfigJson=$cc, ButtonBarKey=$bk, ButtonBarConfigJson=$bc WHERE Id=$id";
        Bind(cmd, page);
        cmd.Parameters.AddWithValue("$id", page.Id);
        cmd.ExecuteNonQuery();
        _log.Write("更新自定义页面", "页面扩展", $"{page.Title} / {page.ControlKey} / {page.ButtonBarKey}");
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM CustomPages WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
        _log.Write("删除自定义页面", "页面扩展", $"Id={id}");
    }

    public static List<PageButtonDef> ParseButtons(string json)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<PageButtonDef>>(json) ?? new();
        }
        catch { return new(); }
    }

    public static string SerializeButtons(List<PageButtonDef> buttons)
        => System.Text.Json.JsonSerializer.Serialize(buttons);

    private static void Bind(SqliteCommand cmd, CustomPage page)
    {
        cmd.Parameters.AddWithValue("$t", page.Title);
        cmd.Parameters.AddWithValue("$k", page.PageKey);
        cmd.Parameters.AddWithValue("$d", page.Description);
        cmd.Parameters.AddWithValue("$b", page.ButtonsJson);
        cmd.Parameters.AddWithValue("$va", page.VisibleToAdmin ? 1 : 0);
        cmd.Parameters.AddWithValue("$vc", page.VisibleToCustomer ? 1 : 0);
        cmd.Parameters.AddWithValue("$vs", page.VisibleToSuperAdmin ? 1 : 0);
        cmd.Parameters.AddWithValue("$s", page.SortOrder);
        cmd.Parameters.AddWithValue("$at", DateTime.Now.ToString("o"));
        cmd.Parameters.AddWithValue("$ck", string.IsNullOrWhiteSpace(page.ControlKey) ? "buttons_only" : page.ControlKey);
        cmd.Parameters.AddWithValue("$cc", string.IsNullOrWhiteSpace(page.ControlConfigJson) ? "{}" : page.ControlConfigJson);
        cmd.Parameters.AddWithValue("$bk", string.IsNullOrWhiteSpace(page.ButtonBarKey) ? "btn_none" : page.ButtonBarKey);
        cmd.Parameters.AddWithValue("$bc", string.IsNullOrWhiteSpace(page.ButtonBarConfigJson) ? "{}" : page.ButtonBarConfigJson);
    }

    private static CustomPage Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Title = reader.GetString(1),
        PageKey = reader.GetString(2),
        Description = reader.GetString(3),
        ButtonsJson = reader.GetString(4),
        VisibleToAdmin = reader.GetInt32(5) == 1,
        VisibleToCustomer = reader.GetInt32(6) == 1,
        VisibleToSuperAdmin = reader.GetInt32(7) == 1,
        SortOrder = reader.GetInt32(8),
        CreatedAt = DateTime.Parse(reader.GetString(9)),
        ControlKey = reader.FieldCount > 10 && !reader.IsDBNull(10) ? reader.GetString(10) : "buttons_only",
        ControlConfigJson = reader.FieldCount > 11 && !reader.IsDBNull(11) ? reader.GetString(11) : "{}",
        ButtonBarKey = reader.FieldCount > 12 && !reader.IsDBNull(12) ? reader.GetString(12) : "btn_none",
        ButtonBarConfigJson = reader.FieldCount > 13 && !reader.IsDBNull(13) ? reader.GetString(13) : "{}"
    };
}
