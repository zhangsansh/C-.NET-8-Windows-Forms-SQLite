namespace BankManagementSystem.Utils;

/// <summary>拦截鼠标/键盘消息，用于重置空闲计时。</summary>
public sealed class ActivityMessageFilter : IMessageFilter
{
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MOUSEWHEEL = 0x020A;
    private const int WM_MOUSEMOVE = 0x0200;

    private Point _lastMouse = Point.Empty;

    public bool PreFilterMessage(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_KEYDOWN:
            case WM_SYSKEYDOWN:
            case WM_LBUTTONDOWN:
            case WM_RBUTTONDOWN:
            case WM_MBUTTONDOWN:
            case WM_MOUSEWHEEL:
                Session.Touch();
                break;
            case WM_MOUSEMOVE:
                var p = Control.MousePosition;
                if (p != _lastMouse)
                {
                    _lastMouse = p;
                    Session.Touch();
                }
                break;
        }
        return false;
    }
}
