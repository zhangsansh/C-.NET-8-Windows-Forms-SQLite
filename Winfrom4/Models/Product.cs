namespace BankManagementSystem.Models;

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    /// <summary>基金 / 期货 / 股票 / 理财 / 存款</summary>
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal YieldRate { get; set; }
    public decimal Profit { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}
