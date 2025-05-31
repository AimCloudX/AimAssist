using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.UI.CheatSheet;
using AimAssist.Units.Core.Units;

namespace AimAssist.Service
{
    public class CheatSheetController : ICheatSheetController, IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private IntPtr _keyboardHookID = IntPtr.Zero;
        private IntPtr _mouseHookID = IntPtr.Zero;
        private LowLevelKeyboardProc _keyboardProc;
        private LowLevelMouseProc _mouseProc;
        private DateTime _ctrlKeyPressStart;
        private bool _isCtrlPressed = false;

        private Dictionary<string, string> _cheatsheets;
        private Dictionary<string, string> _webcCheatsheets;

        private CheatsheetPopup _cheatsheetPopup;
        private DispatcherTimer _timer;

        private readonly Dispatcher dispatcher;
        private readonly IWindowHandleService _windowHandleService;

        public CheatSheetController(Dispatcher dispatcher, IWindowHandleService windowHandleService)
        {
            _keyboardProc = KeyboardHookCallback;
            _mouseProc = MouseHookCallback;
            _keyboardHookID = SetKeyboardHook(_keyboardProc);
            _mouseHookID = SetMouseHook(_mouseProc);
            InitializeCheatsheets();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.5);
            _timer.Tick += Timer_Tick;
            this.dispatcher = dispatcher;
            _windowHandleService = windowHandleService;
        }

        private void InitializeCheatsheets()
        {
            _cheatsheets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var dictInfo = new DirectoryInfo("Resources/CheatSheet/");
                foreach (var file in dictInfo.GetFiles())
                {
                    var name = Path.GetFileNameWithoutExtension(file.Name);
                    var text = File.ReadAllText(file.FullName);
                    _cheatsheets.Add(name, text);
                }

                _webcCheatsheets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var web = new DirectoryInfo("Resources/CheatSheet/Web/");
                foreach (var file in web.GetFiles())
                {
                    var name = Path.GetFileNameWithoutExtension(file.Name);
                    var text = File.ReadAllText(file.FullName);
                    _webcCheatsheets.Add(name, text);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"チートシート初期化エラー: {ex.Message}");
                // エラーログの記録などの追加処理を行う
            }
        }

        private IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr SetMouseHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if ((Keys)vkCode == Keys.LControlKey || (Keys)vkCode == Keys.RControlKey)
                {
                    if (wParam == (IntPtr)WM_KEYDOWN && !_isCtrlPressed)
                    {
                        _isCtrlPressed = true;
                        _ctrlKeyPressStart = DateTime.Now;
                        _timer.Start();
                    }
                    else if (wParam == (IntPtr)WM_KEYUP)
                    {
                        _isCtrlPressed = false;
                        _timer.Stop();
                        dispatcher.Invoke(() => CloseCheatSheet());
                    }
                }
            }

            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _isCtrlPressed)
            {
                if (wParam == (IntPtr)WM_MOUSEWHEEL ||
                    wParam == (IntPtr)WM_LBUTTONDOWN ||
                    wParam == (IntPtr)WM_RBUTTONDOWN ||
                    wParam == (IntPtr)WM_MBUTTONDOWN)
                {
                    // Ctrl押下中にスクロールまたはクリックが検出された場合、タイマーをリセット
                    ResetTimer();
                }
            }

            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        private void ResetTimer()
        {
            _timer.Stop();
            _ctrlKeyPressStart = DateTime.Now;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_isCtrlPressed && (DateTime.Now - _ctrlKeyPressStart).TotalSeconds >= 1)
            {
                dispatcher.Invoke(() => ShowCheatSheet(_windowHandleService.GetActiveProcessName()));
                _timer.Stop();
            }
        }
        
        public static string GetDomainFromUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Host;
            }
            catch (UriFormatException)
            {
                return "Invalid URL";
            }
        }

        /// <summary>
        /// チートシートを表示する
        /// </summary>
        /// <param name="processName">対象プロセス名</param>
        public async Task ShowCheatSheet(string processName)
        {
            if (_cheatsheetPopup != null)
            {
                return;
            }

            try
            {
                if (_cheatsheets.TryGetValue(processName, out string cheatsheetContent))
                {
                    if(processName == "AimAssist")
                    {
                        var unit = GetMainWindowCurrentUnit();
                        if(unit is UrlUnit urlUnit)
                        {
                            var domainName = GetDomainFromUrl(urlUnit.Description);
                            if(_webcCheatsheets.TryGetValue(domainName, out var webCheatSheet))
                            {
                                cheatsheetContent += webCheatSheet;
                                _cheatsheetPopup = new CheatsheetPopup(cheatsheetContent, processName + " / " + domainName);
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            _cheatsheetPopup = new CheatsheetPopup(cheatsheetContent, processName);
                        }
                    }
                    else
                    {
                        _cheatsheetPopup = new CheatsheetPopup(cheatsheetContent, processName);
                    }
                }
                else
                {
                    return;
                }

                // アクティブウィンドウの位置を取得
                IntPtr activeWindow = GetForegroundWindow();
                RECT activeWindowRect;
                GetWindowRect(activeWindow, out activeWindowRect);

                // アクティブウィンドウが表示されているスクリーンを特定
                System.Windows.Forms.Screen activeScreen = System.Windows.Forms.Screen.FromHandle(activeWindow);

                if(_cheatsheetPopup == null)
                {
                    return;
                }
                
                // チートシートポップアップの位置とサイズを設定
                _cheatsheetPopup.Width = activeScreen.WorkingArea.Width;
                _cheatsheetPopup.Height = 220; // 必要に応じて調整
                _cheatsheetPopup.Left = activeScreen.WorkingArea.Left;
                _cheatsheetPopup.Top = activeScreen.WorkingArea.Bottom - _cheatsheetPopup.Height;

                _cheatsheetPopup.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"チートシート表示エラー: {ex.Message}");
                // エラーログの記録などの追加処理を行う
            }
        }

        /// <summary>
        /// チートシートをアプリケーション名で表示する
        /// </summary>
        /// <param name="applicationName">アプリケーション名</param>
        public async Task ShowCheatSheetByApplicationName(string applicationName)
        {
            await ShowCheatSheet(applicationName);
        }

        /// <summary>
        /// チートシートを閉じる
        /// </summary>
        public void CloseCheatSheet()
        {
            if (_cheatsheetPopup != null)
            {
                _cheatsheetPopup.Close();
                _cheatsheetPopup = null;
            }
        }

        private IUnit GetMainWindowCurrentUnit()
        {
            // メインウィンドウから現在のユニットを取得する処理を追加
            // この部分は実際の実装に合わせて修正する必要があります
            // 例えば、WindowHandleServiceからメインウィンドウにアクセスして現在のユニットを取得
            return null;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_keyboardHookID);
            UnhookWindowsHookEx(_mouseHookID);
        }
    }

    public enum Keys
    {
        LControlKey = 0xA2,
        RControlKey = 0xA3
    }
}
