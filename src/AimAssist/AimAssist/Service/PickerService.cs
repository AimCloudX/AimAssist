
using AimAssist.UI.MainWindows;
using AimAssist.UI.PickerWindows;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AimAssist.Service;
internal static class PickerService
{
    private static IntPtr beforeWindow;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    public static void ShowPickerWindow()
    {
        // HotKey押下時のウィンドウのハンドルを取得
        beforeWindow = GetForegroundWindow();

        // 自身のウィンドウハンドルをアクティブにする
        SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

        var window = new PickerWindow();
        window.Closed += DoAction;
        window.Show();
    }

    private static void DoAction(object? sender, EventArgs e)
    {
        try
        {
            if (sender is PickerWindow window)
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
