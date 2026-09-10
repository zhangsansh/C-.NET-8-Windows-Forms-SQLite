namespace BankManagementSystem.Services;

using BankManagementSystem.Data;
using BankManagementSystem.Models;
using BankManagementSystem.Utils;
using Microsoft.Data.Sqlite;

public class AuthService
{
    public User? Login(string username, string password)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT Id, Username, Password, Role, DisplayName, CustomerId, CreatedAt, IsActive
FROM Users WHERE Username = $u AND Password = $p AND IsActive = 1";
        cmd.Parameters.AddWithValue("$u", username.Trim());
        cmd.Parameters.AddWithValue("$p", password);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return new User
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            Password = reader.GetString(2),
            Role = (UserRole)reader.GetInt32(3),
            DisplayName = reader.GetString(4),
            CustomerId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            CreatedAt = DateTime.Parse(reader.GetString(6)),
            IsActive = reader.GetInt32(7) == 1
        };
    }
}

public class LogService
{
    public void Write(string action, string module, string detail)
    {
        var user = Session.CurrentUser;
        var username = user?.Username ?? "系统";
        var role = user?.Role.ToString() ?? "System";
        var userId = user?.Id;
        var now = DateTime.Now;
        var logFolder = DatabaseHelper.LogFolder;
        Directory.CreateDirectory(logFolder);
        var fileName = $"log_{now:yyyyMMdd}.txt";
        var filePath = Path.Combine(logFolder, fileName);
        var line = $"[{now:yyyy-MM-dd HH:mm:ss}] [{role}] [{username}] [{module}] {action} | {detail}";

        var enableFile = DatabaseHelper.GetSetting("EnableFileLog", "1") == "1";
        if (enableFile)
        {
            try { File.AppendAllText(filePath, line + Environment.NewLine); }
            catch { /* ignore file errors */ }
        }

        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO OperationLogs (UserId, Username, Role, Action, Module, Detail, CreatedAt, LogFilePath)
VALUES ($uid, $un, $role, $act, $mod, $det, $at, $path)";
        cmd.Parameters.AddWithValue("$uid", (object?)userId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$un", username);
        cmd.Parameters.AddWithValue("$role", role);
        cmd.Parameters.AddWithValue("$act", action);
        cmd.Parameters.AddWithValue("$mod", module);
        cmd.Parameters.AddWithValue("$det", detail);
        cmd.Parameters.AddWithValue("$at", now.ToString("o"));
        cmd.Parameters.AddWithValue("$path", filePath);
        cmd.ExecuteNonQuery();
    }

    public List<OperationLog> GetLogs(DateTime? from = null, DateTime? to = null, string? keyword = null, int limit = 500)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        var sql = "SELECT Id, UserId, Username, Role, Action, Module, Detail, CreatedAt, LogFilePath FROM OperationLogs WHERE 1=1";
        if (from.HasValue)
        {
            sql += " AND CreatedAt >= $from";
            cmd.Parameters.AddWithValue("$from", from.Value.ToString("o"));
        }
        if (to.HasValue)
        {
            sql += " AND CreatedAt <= $to";
            cmd.Parameters.AddWithValue("$to", to.Value.ToString("o"));
        }
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            sql += " AND (Username LIKE $kw OR Action LIKE $kw OR Module LIKE $kw OR Detail LIKE $kw)";
            cmd.Parameters.AddWithValue("$kw", "%" + keyword.Trim() + "%");
        }
        sql += " ORDER BY Id DESC LIMIT $limit";
        cmd.Parameters.AddWithValue("$limit", limit);
        cmd.CommandText = sql;

        var list = new List<OperationLog>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new OperationLog
            {
                Id = reader.GetInt32(0),
                UserId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                Username = reader.GetString(2),
                Role = reader.GetString(3),
                Action = reader.GetString(4),
                Module = reader.GetString(5),
                Detail = reader.GetString(6),
                CreatedAt = DateTime.Parse(reader.GetString(7)),
                LogFilePath = reader.GetString(8)
            });
        }
        return list;
    }
}

public class CustomerService
{
    private readonly LogService _log = new();

    public List<Customer> GetAll()
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, CustomerNo, Name, Address, Phone, AccountNo, AccountPassword, Balance, BusinessType, ValueAssessment, CreatedAt FROM Customers ORDER BY Id";
        var list = new List<Customer>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var c = Map(reader);
            c.TotalValue = CalculateTotalValue(c.Id, c.Balance);
            list.Add(c);
        }
        return list;
    }

    public Customer? GetById(int id)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, CustomerNo, Name, Address, Phone, AccountNo, AccountPassword, Balance, BusinessType, ValueAssessment, CreatedAt FROM Customers WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        var c = Map(reader);
        c.TotalValue = CalculateTotalValue(c.Id, c.Balance);
        return c;
    }

    public decimal CalculateTotalValue(int customerId, decimal balance)
    {
        var holdings = GetHoldings(customerId);
        var productValue = holdings.Sum(h => h.MarketValue);
        var interestEstimate = balance * GetDepositRate();
        return balance + interestEstimate + productValue;
    }

    public decimal GetDepositRate()
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Rate FROM InterestRates WHERE Name LIKE '%一年期%' LIMIT 1";
        var result = cmd.ExecuteScalar();
        return result == null ? 0.015m : Convert.ToDecimal(result);
    }

    public List<Holding> GetHoldings(int customerId)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT h.Id, h.CustomerId, h.ProductId, p.Name, p.Category, h.Quantity, h.CostPrice, p.Price, p.YieldRate
FROM Holdings h INNER JOIN Products p ON h.ProductId = p.Id WHERE h.CustomerId = $cid";
        cmd.Parameters.AddWithValue("$cid", customerId);
        var list = new List<Holding>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Holding
            {
                Id = reader.GetInt32(0),
                CustomerId = reader.GetInt32(1),
                ProductId = reader.GetInt32(2),
                ProductName = reader.GetString(3),
                Category = reader.GetString(4),
                Quantity = Convert.ToDecimal(reader.GetDouble(5)),
                CostPrice = Convert.ToDecimal(reader.GetDouble(6)),
                CurrentPrice = Convert.ToDecimal(reader.GetDouble(7)),
                YieldRate = Convert.ToDecimal(reader.GetDouble(8))
            });
        }
        return list;
    }

    public void Add(Customer c)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Customers (CustomerNo, Name, Address, Phone, AccountNo, AccountPassword, Balance, BusinessType, ValueAssessment, CreatedAt)
VALUES ($no, $name, $addr, $phone, $acc, $pwd, $bal, $biz, $val, $at)";
        Bind(cmd, c);
        cmd.ExecuteNonQuery();
        _log.Write("新增客户", "客户管理", $"{c.CustomerNo} {c.Name}");
    }

    public void Update(Customer c)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE Customers SET CustomerNo=$no, Name=$name, Address=$addr, Phone=$phone,
AccountNo=$acc, AccountPassword=$pwd, Balance=$bal, BusinessType=$biz, ValueAssessment=$val WHERE Id=$id";
        Bind(cmd, c);
        cmd.Parameters.AddWithValue("$id", c.Id);
        cmd.ExecuteNonQuery();
        _log.Write("更新客户", "客户管理", $"{c.CustomerNo} {c.Name}");
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using (var h = conn.CreateCommand())
        {
            h.CommandText = "DELETE FROM Holdings WHERE CustomerId=$id";
            h.Parameters.AddWithValue("$id", id);
            h.ExecuteNonQuery();
        }
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Customers WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
        _log.Write("删除客户", "客户管理", $"Id={id}");
    }

    public void Deposit(int customerId, decimal amount)
    {
        ChangeBalance(customerId, amount, "存款");
    }

    public void Withdraw(int customerId, decimal amount)
    {
        var c = GetById(customerId) ?? throw new InvalidOperationException("客户不存在");
        if (c.Balance < amount) throw new InvalidOperationException("余额不足");
        ChangeBalance(customerId, -amount, "取款");
    }

    private void ChangeBalance(int customerId, decimal delta, string action)
    {
        using var conn = DatabaseHelper.CreateConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Customers SET Balance = Balance + $d WHERE Id=$id";
        cmd.Parameters.AddWithValue("$d", (double)delta);
        cmd.Parameters.AddWithValue("$id", customerId);
        cmd.ExecuteNonQuery();
        _log.Write(action, "客户业务", $"客户Id={customerId}, 金额={delta:F2}");
    }

    private static void Bind(SqliteCommand cmd, Customer c)
    {
        cmd.Parameters.AddWithValue("$no", c.CustomerNo);
        cmd.Parameters.AddWithValue("$name", c.Name);
        cmd.Parameters.AddWithValue("$addr", c.Address);
        cmd.Parameters.AddWithValue("$phone", c.Phone);
        cmd.Parameters.AddWithValue("$acc", c.AccountNo);
        cmd.Parameters.AddWithValue("$pwd", c.AccountPassword);
        cmd.Parameters.AddWithValue("$bal", (double)c.Balance);
        cmd.Parameters.AddWithValue("$biz", c.BusinessType);
        cmd.Parameters.AddWithValue("$val", c.ValueAssessment);
        cmd.Parameters.AddWithValue("$at", c.CreatedAt == default ? DateTime.Now.ToString("o") : c.CreatedAt.ToString("o"));
    }

    private static Customer Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        CustomerNo = reader.GetString(1),
        Name = reader.GetString(2),
        Address = reader.GetString(3),
        Phone = reader.GetString(4),
        AccountNo = reader.GetString(5),
        AccountPassword = reader.GetString(6),
        Balance = Convert.ToDecimal(reader.GetDouble(7)),
        BusinessType = reader.GetString(8),
        ValueAssessment = reader.GetString(9),
        CreatedAt = DateTime.Parse(reader.GetString(10))
    };
}
