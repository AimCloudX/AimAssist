namespace AimPicker.Service;

using AimPicker.UI.Tools.Snippets;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

internal class SnippetManager
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    public void RunSnippetTool()
    {
        IntPtr hWnd = GetForegroundWindow();

        // 自身のプロセスを取得
        var currentProcess = Process.GetCurrentProcess();

        // 自身のウィンドウハンドルをアクティブにする
        SetForegroundWindow(currentProcess.MainWindowHandle);

        var window = new PickerWindow();
        window.ShowDialog();
        var text = window.SnippetText;
        if (string.IsNullOrEmpty(text))
        {
            // 元のプロセスをアクティブにする
            SetForegroundWindow(hWnd);
            return;
        }

        System.Windows.Clipboard.SetText(text);

        // 元のプロセスをアクティブにする
        SetForegroundWindow(hWnd);
        Thread.Sleep(100);

        // SendKeysを使用してキーを送信するためにSystem.Windows.Formsを追加する必要がある
        SendKeys.SendWait("^v");
    }
}
