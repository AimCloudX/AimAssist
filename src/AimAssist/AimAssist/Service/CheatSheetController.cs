using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using AimAssist.Core;
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
        
        // パフォーマンス最適化
        private readonly bool _isDebugMode;
        private DateTime _lastKeyboardEventTime = DateTime.MinValue;
        private DateTime _lastMouseEventTime = DateTime.MinValue;
        private readonly TimeSpan _debugEventThrottle = TimeSpan.FromMilliseconds(50); // デバッグ中は50msに制限
        private readonly TimeSpan _releaseEventThrottle = TimeSpan.FromMilliseconds(10); // リリース中は10msに制限
        
        // キーコードキャッシュ
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;

        private Dictionary<string, string> _cheatsheets = null!;
        private Dictionary<string, string> _webcCheatsheets = null!;

        private CheatsheetPopup? _cheatsheetPopup;
        private DispatcherTimer _timer = null!;

        private readonly Dispatcher dispatcher;
        private readonly IWindowHandleService _windowHandleService;

        public CheatSheetController(Dispatcher dispatcher, IWindowHandleService windowHandleService)
        {
            // デバッグモードの検出
            _isDebugMode = Debugger.IsAttached;
            
            _keyboardProc = KeyboardHookCallback;
            _mouseProc = MouseHookCallback;
            
            // デバッグ中でもフックを有効にするが、最適化を適用
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
            // 無効なコードは早期リターン
            if (nCode < 0)
                return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
                
            // イベント処理を制限
            var now = DateTime.Now;
            var throttleTime = _isDebugMode ? _debugEventThrottle : _releaseEventThrottle;
            
            if (now - _lastKeyboardEventTime < throttleTime)
            {
                return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
            }
            _lastKeyboardEventTime = now;
            
            try
            {
                // Marshal.ReadInt32の回数を減らすため、一度だけ読み取り
                int vkCode = Marshal.ReadInt32(lParam);

                // 定数を使用して比較を高速化
                if (vkCode == VK_LCONTROL || vkCode == VK_RCONTROL)
                {
                    bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN;
                    bool isKeyUp = wParam == (IntPtr)WM_KEYUP;
                    
                    if (isKeyDown && !_isCtrlPressed)
                    {
                        _isCtrlPressed = true;
                        _ctrlKeyPressStart = now; // 既にDateTime.Nowを取得済み
                        _timer.Start();
                    }
                    else if (isKeyUp && _isCtrlPressed)
                    {
                        _isCtrlPressed = false;
                        _timer.Stop();
                        
                        // 非同期実行で応答性を向上
                        dispatcher.BeginInvoke(() => CloseCheatSheet());
                    }
                }
            }
            catch (Exception ex)
            {
                // エラーログは最小限に
                if (!_isDebugMode)
                {
                    Debug.WriteLine($"KeyboardHook error: {ex.Message}");
                }
            }

            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // 無効なコードまたはCtrlキーが押されていない場合は早期リターン
            if (nCode < 0 || !_isCtrlPressed)
                return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
                
            // イベント処理を制限
            var now = DateTime.Now;
            var throttleTime = _isDebugMode ? _debugEventThrottle : _releaseEventThrottle;
            
            if (now - _lastMouseEventTime < throttleTime)
            {
                return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
            }
            _lastMouseEventTime = now;
            
            try
            {
                // IntPtr比較を一度にまとめて効率化
                IntPtr msgType = wParam;
                if (msgType == (IntPtr)WM_MOUSEWHEEL ||
                    msgType == (IntPtr)WM_LBUTTONDOWN ||
                    msgType == (IntPtr)WM_RBUTTONDOWN ||
                    msgType == (IntPtr)WM_MBUTTONDOWN)
                {
                    // Ctrl押下中にスクロールまたはクリックが検出された場合、タイマーをリセット
                    ResetTimer();
                }
            }
            catch (Exception ex)
            {
                // エラーログは最小限に
                if (!_isDebugMode)
                {
                    Debug.WriteLine($"MouseHook error: {ex.Message}");
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

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Ctrlキーが押されていない場合は早期リターン
            if (!_isCtrlPressed)
            {
                _timer.Stop();
                return;
            }
                
            // 1秒以上経過したかチェック
            if ((DateTime.Now - _ctrlKeyPressStart).TotalSeconds >= 1)
            {
                // 常にBeginInvokeで非同期実行（応答性向上）
                dispatcher.BeginInvoke(() => 
                {
                    var processName = _windowHandleService.GetActiveProcessName();
                    ShowCheatSheet(processName);
                });
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
        public Task ShowCheatSheet(string processName)
        {
            // 既にポップアップが表示されている場合は早期リターン
            if (_cheatsheetPopup != null)
                return Task.CompletedTask;

            try
            {
                // チートシートコンテンツを取得
                if (!_cheatsheets.TryGetValue(processName, out string? cheatsheetContent))
                    return Task.CompletedTask;

                string title = processName;

                // アプリケーション名の場合の特別処理
                if (processName == Constants.AppName)
                {
                    var unit = GetMainWindowCurrentUnit();
                    if (unit is UrlUnit urlUnit)
                    {
                        var domainName = GetDomainFromUrl(urlUnit.Description);
                        if (_webcCheatsheets.TryGetValue(domainName, out var webCheatSheet))
                        {
                            cheatsheetContent += webCheatSheet;
                            title = $"{processName} / {domainName}";
                        }
                        else
                        {
                            return Task.CompletedTask;
                        }
                    }
                }

                // ポップアップを作成
                _cheatsheetPopup = new CheatsheetPopup(cheatsheetContent, title);

                // 画面位置の計算を最適化
                IntPtr activeWindow = GetForegroundWindow();
                var activeScreen = System.Windows.Forms.Screen.FromHandle(activeWindow);

                const int popupHeight = 220;
                
                // ポップアップの位置とサイズを設定
                _cheatsheetPopup.Width = activeScreen.WorkingArea.Width;
                _cheatsheetPopup.Height = popupHeight;
                _cheatsheetPopup.Left = activeScreen.WorkingArea.Left;
                _cheatsheetPopup.Top = activeScreen.WorkingArea.Bottom - popupHeight;

                _cheatsheetPopup.Show();
            }
            catch (Exception ex)
            {
                // エラー時はポップアップをクリア
                _cheatsheetPopup = null;
                Debug.WriteLine($"チートシート表示エラー: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// チートシートをアプリケーション名で表示する
        /// </summary>
        /// <param name="applicationName">アプリケーション名</param>
        public Task ShowCheatSheetByApplicationName(string applicationName)
        {
            return ShowCheatSheet(applicationName);
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

        private IUnit? GetMainWindowCurrentUnit()
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
