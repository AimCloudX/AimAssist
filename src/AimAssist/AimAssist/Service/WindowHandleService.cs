
using AimAssist.UI.MainWindows;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AimAssist.Service;
internal static class WindowHandleService
{
    public static MainWindow Window { get; private set; } = new MainWindow();
    private static bool isActivate;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void ToggleMainWindow()
    {
        if (isActivate)
        {
            isActivate = false;
            Window.Closed -= DoAction;
            Window.Visibility = System.Windows.Visibility.Collapsed;
            return;
        }

        isActivate = true;

        if (Window.IsClosing)
        {
            Window = new MainWindow();
        }

        Window.Visibility = System.Windows.Visibility.Visible;
        Window.Closed += DoAction;
        Window.Focus();
        Window.Show();
         
        // 自身のウィンドウハンドルをアクティブにする
        SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
    }

    private static void DoAction(object? sender, EventArgs e)
    {
        isActivate = false;
    }
}
