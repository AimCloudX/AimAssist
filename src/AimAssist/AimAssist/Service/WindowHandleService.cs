
using AimAssist.UI.MainWindows;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AimAssist.Service;

public class WindowHandleService
{
    public MainWindow Window { get; private set; }
    private bool isActivate;
    private readonly IServiceProvider _serviceProvider;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public WindowHandleService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ToggleMainWindow()
    {
        if (isActivate)
        {
            isActivate = false;
            Window.Closed -= DoAction;
            Window.Visibility = System.Windows.Visibility.Collapsed;
            return;
        }

        isActivate = true;

        if (Window == null || Window.IsClosing)
        {
            // DIコンテナからMainWindowを取得
            Window = _serviceProvider.GetRequiredService<MainWindow>();
        }

        Window.Visibility = System.Windows.Visibility.Visible;
        Window.Closed += DoAction;
        Window.Focus();
        Window.Show();
         
        // 自身のウィンドウハンドルをアクティブにする
        SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
    }

    private void DoAction(object? sender, EventArgs e)
    {
        isActivate = false;
    }
}
