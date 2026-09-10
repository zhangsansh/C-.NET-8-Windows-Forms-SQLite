namespace BankManagementSystem.Models;

public class Holding
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal YieldRate { get; set; }

    public decimal MarketValue => Quantity * CurrentPrice;
    public decimal ProfitLoss => (CurrentPrice - CostPrice) * Quantity;
}
