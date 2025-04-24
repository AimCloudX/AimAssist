using AimAssist.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Threading;

namespace AimAssist.Service
{
    /// <summary>
    /// アプリケーションログサービスの実装クラス
    /// </summary>
    public class ApplicationLogService : IApplicationLogService
    {
        private DispatcherTimer _timer;
        private List<LogEntry> _logEntries = new List<LogEntry>();
        private string _logDirectoryPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AimAssist", "ApplicationLog");

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ApplicationLogService()
        {
            // DIでインスタンス化されるように、パブリックコンストラクタを提供
        }

        /// <summary>
        /// サービスを初期化します
        /// </summary>
        public void Initialize()
        {
            Directory.CreateDirectory(_logDirectoryPath); // ログディレクトリを作成
            LoadLogFromFile();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(1); // 1分ごとにチェック
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var activeWindowInfo = GetActiveWindowInfo();
            if (activeWindowInfo != null)
            {
                // ログに追加
                LogActiveWindow(activeWindowInfo);
            }
        }

        private ActiveWindowInfo GetActiveWindowInfo()
        {
            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero)
            {
                return null;
            }

            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (GetWindowText(handle, Buff, nChars) == 0)
            {
                return null;
            }
            string windowTitle = Buff.ToString();

            GetWindowThreadProcessId(handle, out uint processId);
            Process process = Process.GetProcessById((int)processId);
            string appName = process.ProcessName;

            return new ActiveWindowInfo
            {
                WindowTitle = windowTitle,
                AppName = appName,
                Timestamp = DateTime.Now
            };
        }

        private void LogActiveWindow(ActiveWindowInfo activeWindowInfo)
        {
            var logEntry = new LogEntry
            {
                Time = activeWindowInfo.Timestamp.ToString("yyyy-MM-ddTHH:mm"), // ISO 8601形式のタイムスタンプ
                Title = activeWindowInfo.WindowTitle,
                App = activeWindowInfo.AppName
            };
            _logEntries.Add(logEntry);
            Debug.WriteLine($"Active Window: {activeWindowInfo.WindowTitle} (App: {activeWindowInfo.AppName}) at {logEntry.Time}");

            // JSONファイルにログを保存
            SaveLogToFile(logEntry);
        }

        private void SaveLogToFile(LogEntry logEntry)
        {
            string fileName = $"ActiveWindowLog_{DateTime.Now:yyyy_MM}.json";
            string filePath = System.IO.Path.Combine(_logDirectoryPath, fileName);
            using (StreamWriter writer = new StreamWriter(filePath, true)) // Append mode
            {
                var json = JsonConvert.SerializeObject(logEntry, Formatting.None);
                writer.WriteLine(json);
            }
        }

        private void LoadLogFromFile()
        {
            string fileName = $"ActiveWindowLog_{DateTime.Now:yyyy_MM}.json";
            string filePath = System.IO.Path.Combine(_logDirectoryPath, fileName);

            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var logEntry = JsonConvert.DeserializeObject<LogEntry>(line);
                    _logEntries.Add(logEntry);
                }
            }
        }
    }

    /// <summary>
    /// ログエントリクラス
    /// </summary>
    public class LogEntry
    {
        public string Time { get; set; }
        public string Title { get; set; }
        public string App { get; set; }
    }

    /// <summary>
    /// アクティブウィンドウ情報クラス
    /// </summary>
    public class ActiveWindowInfo
    {
        public string WindowTitle { get; set; }
        public string AppName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
