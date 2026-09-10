namespace BankManagementSystem.Models;

public class BankInfo
{
    public int Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalAssets { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal TotalLoans { get; set; }
    public int CustomerCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}
