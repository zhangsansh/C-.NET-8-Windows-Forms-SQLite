namespace BankManagementSystem.Utils;

using BankManagementSystem.Models;

public static class Session
{
    public static User? CurrentUser { get; private set; }

    public static bool IsLoggedIn => CurrentUser != null;

    public static bool IsAdmin => CurrentUser?.Role is UserRole.Admin or UserRole.SuperAdmin;

    public static bool IsSuperAdmin => CurrentUser?.Role == UserRole.SuperAdmin;

    public static bool IsCustomer => CurrentUser?.Role == UserRole.Customer;

    public static DateTime? LoginTime { get; private set; }

    public static DateTime LastActivity { get; private set; } = DateTime.Now;

    /// <summary>无操作自动登出秒数，默认 300 秒（5 分钟）</summary>
    public static int IdleTimeoutSeconds { get; set; } = 300;

    public static event Action? LoggedIn;
    public static event Action? LoggedOut;

    public static void Login(User user)
    {
        CurrentUser = user;
        LoginTime = DateTime.Now;
        Touch();
        LoggedIn?.Invoke();
    }

    public static void Logout()
    {
        if (CurrentUser == null) return;
        CurrentUser = null;
        LoginTime = null;
        LoggedOut?.Invoke();
    }

    public static void Touch() => LastActivity = DateTime.Now;

    public static int RemainingIdleSeconds
    {
        get
        {
            if (!IsLoggedIn) return IdleTimeoutSeconds;
            var elapsed = (int)(DateTime.Now - LastActivity).TotalSeconds;
            return Math.Max(0, IdleTimeoutSeconds - elapsed);
        }
    }

    public static bool IsIdleTimedOut =>
        IsLoggedIn && IdleTimeoutSeconds > 0 && RemainingIdleSeconds <= 0;
}
