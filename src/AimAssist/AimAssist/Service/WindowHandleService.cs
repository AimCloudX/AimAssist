
using AimAssist.UI.MainWindows;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AimAssist.Service;
internal static class WindowHandleService
{
    public static MainWindow Window { get; private set; } = new MainWindow();
    private static IntPtr beforeWindow;
    private static bool isActivate;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

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
        // HotKey押下時のウィンドウのハンドルを取得
        beforeWindow = GetForegroundWindow();

        // 自身のウィンドウハンドルをアクティブにする
        SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

        if (Window.IsClosing)
        {
            Window = new MainWindow();
        }

        Window.Visibility = System.Windows.Visibility.Visible;
        Window.Closed += DoAction;
        Window.Show();
    }

    private static void DoAction(object? sender, EventArgs e)
    {
        try
        {
            isActivate = false;
            if (sender is MainWindow window)
            {
                var text = window.SnippetText;
                if (string.IsNullOrEmpty(text))
                {
                    // 元のプロセスをアクティブにする
                    SetForegroundWindow(beforeWindow);
                    return;
                }

                System.Windows.Clipboard.SetText(text);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message);
        }

        // 元のプロセスをアクティブにする
        SetForegroundWindow(beforeWindow);
        Thread.Sleep(100); // アクテイブになるまで少し待つ

        // SendKeysを使用してキーを送信するためにSystem.Windows.Formsを追加する必要がある
        SendKeys.SendWait("^v");
    }
}
