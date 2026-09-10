namespace BankManagementSystem.Models;

public class Customer
{
    public int Id { get; set; }
    public string CustomerNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string AccountPassword { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string BusinessType { get; set; } = string.Empty;
    public string ValueAssessment { get; set; } = "普通";
    public DateTime CreatedAt { get; set; }

    /// <summary>存款 + 利息估算 + 持有产品市值</summary>
    public decimal TotalValue { get; set; }
}
