using AimAssist.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AimAssist.Core.Services
{
    /// <summary>
    /// アプリケーションログを管理するサービス
    /// </summary>
    public class ApplicationLogService : IApplicationLogService
    {
        private readonly string _logFilePath;
        private readonly int _maxLogFileSize;
        private readonly List<string> _recentLogs;
        private readonly object _lockObject = new object();
        private readonly bool _isDebugMode;

        public ApplicationLogService(string logDirectory = null, int maxLogFileSize = 10485760, bool isDebugMode = false)
        {
            _isDebugMode = isDebugMode;
            _maxLogFileSize = maxLogFileSize; // デフォルト10MB
            _recentLogs = new List<string>();

            // ログディレクトリの設定
            string baseDir = logDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AimAssist", "Logs");

            // ディレクトリが存在しない場合は作成
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }

            // 日付でログファイル名を作成
            string date = DateTime.Now.ToString("yyyyMMdd");
            _logFilePath = Path.Combine(baseDir, $"AimAssist_{date}.log");

            // 起動時のログ
            Log(LogLevel.Info, "ApplicationLogService initialized");
        }

        /// <summary>
        /// ログを記録します
        /// </summary>
        public void Log(LogLevel level, string message)
        {
            if (level == LogLevel.Debug && !_isDebugMode)
                return;

            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            lock (_lockObject)
            {
                // メモリ内のログバッファに追加
                _recentLogs.Add(logEntry);
                if (_recentLogs.Count > 1000) // 最大1000件保持
                {
                    _recentLogs.RemoveAt(0);
                }

                try
                {
                    // ファイルへの書き込み
                    WriteToLogFile(logEntry);
                }
                catch (Exception ex)
                {
                    // ログファイルへの書き込みに失敗した場合はコンソールに出力
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                    Console.WriteLine(logEntry);
                }
            }
        }

        /// <summary>
        /// 例外をログに記録します
        /// </summary>
        public void LogException(Exception ex, string additionalInfo = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(additionalInfo != null ? $"Exception: {additionalInfo}" : "Exception:");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Type: {ex.GetType().FullName}");
            sb.AppendLine($"StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                sb.AppendLine("InnerException:");
                sb.AppendLine($"Message: {ex.InnerException.Message}");
                sb.AppendLine($"Type: {ex.InnerException.GetType().FullName}");
                sb.AppendLine($"StackTrace: {ex.InnerException.StackTrace}");
            }

            Log(LogLevel.Error, sb.ToString());
        }

        /// <summary>
        /// デバッグレベルのログを記録します
        /// </summary>
        public void Debug(string message) => Log(LogLevel.Debug, message);

        /// <summary>
        /// 情報レベルのログを記録します
        /// </summary>
        public void Info(string message) => Log(LogLevel.Info, message);

        /// <summary>
        /// 警告レベルのログを記録します
        /// </summary>
        public void Warning(string message) => Log(LogLevel.Warning, message);

        /// <summary>
        /// エラーレベルのログを記録します
        /// </summary>
        public void Error(string message) => Log(LogLevel.Error, message);

        /// <summary>
        /// 重大エラーレベルのログを記録します
        /// </summary>
        public void Critical(string message) => Log(LogLevel.Critical, message);

        /// <summary>
        /// 最近のログエントリを取得します
        /// </summary>
        public IEnumerable<string> GetRecentLogs(int count = 100)
        {
            lock (_lockObject)
            {
                int startIndex = Math.Max(0, _recentLogs.Count - count);
                for (int i = startIndex; i < _recentLogs.Count; i++)
                {
                    yield return _recentLogs[i];
                }
            }
        }

        /// <summary>
        /// ログファイルにエントリを書き込みます
        /// </summary>
        private void WriteToLogFile(string logEntry)
        {
            try
            {
                // ファイルが存在し、サイズ制限を超えている場合はローテーション
                if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length > _maxLogFileSize)
                {
                    RotateLogFile();
                }

                // ログをファイルに追記
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// ログファイルをローテーションします
        /// </summary>
        private void RotateLogFile()
        {
            try
            {
                string directory = Path.GetDirectoryName(_logFilePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(_logFilePath);
                string extension = Path.GetExtension(_logFilePath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string newPath = Path.Combine(directory, $"{fileNameWithoutExt}_{timestamp}{extension}");

                // 古いログファイルを名前変更
                File.Move(_logFilePath, newPath);

                // ログフォルダのクリーンアップ（14日以上前のログを削除）
                CleanupOldLogFiles(directory, 14);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rotating log file: {ex.Message}");
            }
        }

        /// <summary>
        /// 古いログファイルを削除します
        /// </summary>
        private void CleanupOldLogFiles(string directory, int daysToKeep)
        {
            try
            {
                DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                foreach (string file in Directory.GetFiles(directory, "AimAssist_*.log"))
                {
                    if (File.GetLastWriteTime(file) < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up old log files: {ex.Message}");
            }
        }
    }
}
