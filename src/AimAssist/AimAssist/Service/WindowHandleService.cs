using AimAssist.Core.Interfaces;
using AimAssist.UI.MainWindows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AimAssist.Service
{
    public class WindowHandleService : IWindowHandleService
    {
        public MainWindow Window { get; private set; }
        private bool isActivate;
        private readonly IServiceProvider _serviceProvider;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public WindowHandleService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ToggleMainWindow()
        {
            if (isActivate)
            {
                isActivate = false;
                if (Window != null)
                {
                    Window.Closed -= DoAction;
                    Window.Visibility = System.Windows.Visibility.Collapsed;
                }
                return;
            }

            isActivate = true;

            if (Window == null)
            {
                Window = _serviceProvider.GetRequiredService<MainWindow>();
            }

            Window.Visibility = System.Windows.Visibility.Visible;
            Window.Closed += DoAction;
            Window.Focus();
            Window.Show();
             
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
        }

        public int GetActiveProcessId()
        {
            IntPtr hwnd = GetForegroundWindow();
            GetWindowThreadProcessId(hwnd, out uint processId);
            return (int)processId;
        }

        public string GetActiveProcessName()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out uint processId);
                Process process = Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"プロセス名取得エラー: {ex.Message}");
                return string.Empty;
            }
        }

        public string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = GetForegroundWindow();
            var buff = new System.Text.StringBuilder(nChars);

            if (GetWindowText(handle, buff, nChars) > 0)
            {
                return buff.ToString();
            }
            return string.Empty;
        }

        public IEnumerable<IntPtr> GetProcessWindows(string processName)
        {
            var windows = new List<IntPtr>();
            var processes = Process.GetProcessesByName(processName);
            
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                foreach (var process in processes)
                {
                    if (process.Id == processId)
                    {
                        windows.Add(hWnd);
                        break;
                    }
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public void ActivateWindow(IntPtr hwnd)
        {
            SetForegroundWindow(hwnd);
        }

        private void DoAction(object? sender, EventArgs e)
        {
            isActivate = false;
        }
    }
}
