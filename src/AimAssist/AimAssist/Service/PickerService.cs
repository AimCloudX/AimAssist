
using AimAssist.Core.Interfaces;
using AimAssist.UI.MainWindows;
using AimAssist.UI.PickerWindows;
using Common.Commands.Shortcus;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AimAssist.Service;

public class PickerService
{
    private static IntPtr beforeWindow;
    private static bool isActive;
    private readonly ICommandService _commandService;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    public PickerService(ICommandService commandService)
    {
        _commandService = commandService;
    }

    public void ShowPickerWindow()
    {
        if(isActive) { return; }
        isActive = true;

        // HotKey押下時のウィンドウのハンドルを取得
        beforeWindow = GetForegroundWindow();

        // 自身のウィンドウハンドルをアクティブにする
        //SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

        GetWindowThreadProcessId(beforeWindow, out var processId);
        Process process = Process.GetProcessById((int)processId);

        var window = new PickerWindow(process.ProcessName, _commandService);
        window.Closed += DoAction;
        window.Show();
        window.Activate();
    }

    private static void DoAction(object? sender, EventArgs e)
    {
        isActive = false;

        string text = string.Empty;
        KeySequence keySequence = null;

        try
        {
            if (sender is PickerWindow window)
            {
                text = window.SnippetText;
                if (!string.IsNullOrEmpty(text))
                {
                    System.Windows.Clipboard.SetText(text);
                }
                keySequence = window.KeySequence;

            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message);
        }

        // 元のプロセスをアクティブにする
        SetForegroundWindow(beforeWindow);
        Thread.Sleep(100); // アクティブになるまで少し待つ

        // SendKeysを使用してキーを送信するためにSystem.Windows.Formsを追加する必要がある

        if(keySequence != null)
        {
            var keyText = keySequence.Parse();
            SendKeys.SendWait(keyText);
        }
        else if(!string.IsNullOrEmpty(text))
        {
            SendKeys.SendWait("^v");
        }
    }
}
