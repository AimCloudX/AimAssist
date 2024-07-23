using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows.Forms;
using System.IO;

namespace CheatSheet.Services
{
    public class CheatSheetController : IDisposable
    {
private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;
        private DateTime _ctrlKeyPressStart;
        private bool _isCtrlPressed = false;
        private Dictionary<string, string> _cheatsheets;
        private CheatsheetPopup _cheatsheetPopup;
        private DispatcherTimer _timer;

        private readonly Dispatcher dispatcher;

        public CheatSheetController(Dispatcher dispatcher)
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
            InitializeCheatsheets();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.5);
            _timer.Tick += Timer_Tick;
            this.dispatcher = dispatcher;
        }

        private void InitializeCheatsheets()
        {
            _cheatsheets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var dictInfo = new DirectoryInfo("Resources/CheatSheet/");
            foreach (var file in dictInfo.GetFiles())
            {
                var name =   Path.GetFileNameWithoutExtension(file.Name);
                var text = File.ReadAllText(file.FullName);
                _cheatsheets.Add(name, text);
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
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
                        dispatcher.Invoke(() => HideCheatsheet());
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_isCtrlPressed && (DateTime.Now - _ctrlKeyPressStart).TotalSeconds >= 1)
            {
                dispatcher.Invoke(() => ShowCheatsheet());
                _timer.Stop();
            }
        }

        private void ShowCheatsheet()
        {
            if (_cheatsheetPopup != null)
            {
                return;
            }

            string activeAppName = GetActiveApplicationName();
            if (_cheatsheets.TryGetValue(activeAppName, out string cheatsheetContent))
            {
                _cheatsheetPopup = new CheatsheetPopup(cheatsheetContent, activeAppName);
            }
            else
            {
                _cheatsheetPopup = new CheatsheetPopup(_cheatsheets["windows"], "Windows (Default)");
            }

            // アクティブウィンドウの位置を取得
            IntPtr activeWindow = GetForegroundWindow();
            RECT activeWindowRect;
            GetWindowRect(activeWindow, out activeWindowRect);

            // アクティブウィンドウが表示されているスクリーンを特定
            System.Windows.Forms.Screen activeScreen = System.Windows.Forms.Screen.FromHandle(activeWindow);

            // チートシートポップアップの位置とサイズを設定
            _cheatsheetPopup.Width = activeScreen.WorkingArea.Width;
            _cheatsheetPopup.Height = 130; // 必要に応じて調整
            _cheatsheetPopup.Left = activeScreen.WorkingArea.Left;
            _cheatsheetPopup.Top = activeScreen.WorkingArea.Bottom - _cheatsheetPopup.Height;

            _cheatsheetPopup.Show();
        }

        private void HideCheatsheet()
        {
            if (_cheatsheetPopup != null)
            {
                _cheatsheetPopup.Close();
                _cheatsheetPopup = null;
            }
        }

        private string GetActiveApplicationName()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint processId;
            GetWindowThreadProcessId(hwnd, out processId);
            Process process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

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
            UnhookWindowsHookEx(_hookID);
        }
    }

    public enum Keys
    {
        LControlKey = 0xA2,
        RControlKey = 0xA3
    }
}
