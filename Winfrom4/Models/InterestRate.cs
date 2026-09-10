namespace BankManagementSystem.Models;

public class InterestRate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
