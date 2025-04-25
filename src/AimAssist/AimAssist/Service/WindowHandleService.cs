using AimAssist.Core.Interfaces;
using AimAssist.UI.MainWindows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AimAssist.Service
{
    /// <summary>
    /// ウィンドウハンドル操作サービスの実装クラス
    /// </summary>
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="serviceProvider">サービスプロバイダ</param>
        public WindowHandleService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// メインウィンドウの表示・非表示を切り替える
        /// </summary>
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

        /// <summary>
        /// アクティブウィンドウのプロセスIDを取得
        /// </summary>
        /// <returns>プロセスID</returns>
        public int GetActiveProcessId()
        {
            IntPtr hwnd = GetForegroundWindow();
            GetWindowThreadProcessId(hwnd, out uint processId);
            return (int)processId;
        }

        /// <summary>
        /// アクティブウィンドウのプロセス名を取得
        /// </summary>
        /// <returns>プロセス名</returns>
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

        /// <summary>
        /// アクティブウィンドウのタイトルを取得
        /// </summary>
        /// <returns>ウィンドウタイトル</returns>
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

        /// <summary>
        /// 指定したプロセス名のウィンドウを取得
        /// </summary>
        /// <param name="processName">プロセス名</param>
        /// <returns>ウィンドウハンドル一覧</returns>
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

        /// <summary>
        /// ウィンドウをアクティブにする
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
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
