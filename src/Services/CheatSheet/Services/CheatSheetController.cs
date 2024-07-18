using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

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
            _cheatsheets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "windows", @"ショートカットキー・チートシート Windows版
Ctrl+N 新規
Ctrl+S 保存
Ctrl+O 開く
Ctrl+P 印刷
Ctrl+A すべて選択
Ctrl+C コピー
Ctrl+X カット
Ctrl+V ペースト
Ctrl+Z アンドゥ
Ctrl+Y リドゥ
Ctrl+Shift+N 新規フォルダなど
Ctrl+F 検索
Ctrl+D 削除・複製など
Ctrl+R 更新
Tab 次⼊⼒エリア
Shift+Tab 前⼊⼒エリア
Alt+→ 次⾴
Alt+← 前⾴
Alt+Tab 次窓
Alt+Shift+Tab 前窓
!+Tab タスクビュー
⎙ 全画⾯撮影
!+⎙ 全画⾯収録
!+Shift+S 画⾯収録
Ctrl+W 閉じる
Alt+F4 終了
Ctrl+Alt+Del ｾｷｭｱ･ｱﾃﾝｼｮﾝ
Ctrl+Shift+Esc 強制終了
Ctrl+🡙 拡⼤縮⼩
Alt+↩︎ プロパティ
Shift+F10 ⽂脈メニュー
Alt+Space 窓メニュー
Shift+←↑→↓ 範囲選択
🡙 ホイールスクロール" },
                { "excel", @"ショートカットキー・チートシート Excelfor Windows版
Ctrl+N 新規
Ctrl+S 保存
Ctrl+O 開く
Ctrl+P 印刷
Ctrl+A すべて選択
Ctrl+C コピー
Ctrl+X カット
Ctrl+V ペースト
Ctrl+Z アンドゥ
Ctrl+Y リドゥ
Ctrl+F 検索
Ctrl+D 下セルにコピー
Ctrl+R 右セルにコピー
Ctrl+1 書式設定
Ctrl+↖↘ 先頭末尾
Ctrl+⇞⇟ 次前シート
Alt+⇞⇟ 次前画⾯
Tab 次列
Shift+Tab 前列
Alt+←→ 前次⾴
Alt+Tab 次窓
Alt+Shift+Tab 前窓
Alt+↩︎ 改⾏挿⼊ほか
Ctrl+↩︎ ⼀括⼊⼒
⎙|!+⎙|!+Shift+S 画⾯撮影
Ctrl+W 閉じる
Shift+F10 ⽂脈メニュー
Alt+Space 窓メニュー
Shift+←↑→↓ 範囲選択
Ctrl+←↑→↓ セル移動
Ctrl+Shift+←↑→↓ セル選択
Ctrl+Space 列選択
Ctrl+BackSpace セルに戻る
Ctrl+Shift+~ 標準表⽰形式
Ctrl+Shift+' 上セル数式コピー
Ctrl+Shift+"" 上セル値コピー
Ctrl+Shift+# ⽇付表⽰形式
Ctrl+Shift+$ 通貨表⽰形式
Ctrl+Shift+% ％表⽰形式
Ctrl+Shift+& 外枠罫線追加
Ctrl+Shift+_ 外枠罫線削除
Ctrl+| ⾏不⼀致選択
Ctrl+Shift+| 列不⼀致選択" },
                                { "code", @"ショートカットキー・チートシート VSCode
Ctrl+N 新規
Ctrl+S 保存
Ctrl+O 開く
"},
                                { "AimAssist", @"AimAssist

Alt+A AimAssistの表示切り替え
Alt+P ピッカーウィンドウ表示
Ctrl+D AimAssiを閉じる
Ctrl+K,Ctrl+S ショートカット設定
Ctrl+K,Ctrl+L コンテンツにフォーカス
Ctrl+K,Ctrl+J ユニットにフォーカス
Ctrl+N 次のユニット
Ctrl+P 前のユニット
"},
            };


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
            if (_isCtrlPressed && (DateTime.Now - _ctrlKeyPressStart).TotalSeconds >= 0.5)
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
