
using AimPicker.Domain;
using AimPicker.UI.Tools.Snippets;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AimPicker.Service;
internal class PickerService
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    public static void Run(PickerMode mode)
    {
        // HotKey押下時のウィンドウのハンドルを取得
        IntPtr hWnd = GetForegroundWindow();

        // 自身のウィンドウハンドルをアクティブにする
        SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

        try
        {
            var window = new PickerWindow(mode);
            window.ShowDialog();

            var text = window.SnippetText;
            if (string.IsNullOrEmpty(text))
            {
                // 元のプロセスをアクティブにする
                SetForegroundWindow(hWnd);
                return;
            }

            System.Windows.Clipboard.SetText(text);

        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message);
        }

        // 元のプロセスをアクティブにする
        SetForegroundWindow(hWnd);
        Thread.Sleep(100); // アクテイブになるまで少し待つ

        // SendKeysを使用してキーを送信するためにSystem.Windows.Formsを追加する必要がある
        SendKeys.SendWait("^v");
    }
}
