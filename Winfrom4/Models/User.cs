namespace BankManagementSystem.Models;

public enum UserRole
{
    Customer = 0,
    Admin = 1,
    SuperAdmin = 2
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
