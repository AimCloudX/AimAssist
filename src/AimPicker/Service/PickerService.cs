namespace AimPicker.Service;

using AimPicker.Domain;
using AimPicker.UI.Tools.Snippets;
using MaterialDesignThemes.Wpf;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

internal class PickerService
{
    private static PickerWindow window;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    public static void Run(PickerMode mode)
    {
        if (window?.Visibility == Visibility.Visible)
        {
            return;
        }

        // HotKey押下時のウィンドウのハンドルを取得
        IntPtr hWnd = GetForegroundWindow();

        // 自身のウィンドウハンドルをアクティブにする
        SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

        //var window = new PickerWindow(mode);
        if(window != null  && window.IsClosing == false)
        {

            window.WindowActivate();
            window.ShowDialog();
        }
        else
        {
            window = new PickerWindow(mode);
            window.ShowDialog();
        }

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
        Thread.Sleep(100); // アクテイブになるまで少し待つ

        // SendKeysを使用してキーを送信するためにSystem.Windows.Formsを追加する必要がある
        SendKeys.SendWait("^v");
    }
}
