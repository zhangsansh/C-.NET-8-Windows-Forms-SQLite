namespace BankManagementSystem.Models;

public class OperationLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string LogFilePath { get; set; } = string.Empty;
}
