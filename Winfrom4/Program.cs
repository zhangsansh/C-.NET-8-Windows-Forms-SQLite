namespace BankManagementSystem;

using BankManagementSystem.Data;
using BankManagementSystem.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
            MessageBox.Show(e.Exception.Message + "\n\n" + e.Exception.GetType().Name,
                "程序异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                MessageBox.Show(ex.Message, "未处理异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        try
        {
            DatabaseHelper.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show("数据库初始化失败：\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new MainForm());
    }
}
