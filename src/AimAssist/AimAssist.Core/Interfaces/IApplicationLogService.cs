namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// アプリケーションログの重要度レベル
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// アプリケーションログを管理するインターフェース
    /// </summary>
    public interface IApplicationLogService
    {
        /// <summary>
        /// ログを記録します
        /// </summary>
        /// <param name="level">ログレベル</param>
        /// <param name="message">メッセージ</param>
        void Log(LogLevel level, string message);

        /// <summary>
        /// 例外をログに記録します
        /// </summary>
        /// <param name="ex">例外</param>
        /// <param name="additionalInfo">追加情報</param>
        void LogException(Exception ex, string? additionalInfo = null);

        /// <summary>
        /// デバッグレベルのログを記録します
        /// </summary>
        /// <param name="message">メッセージ</param>
        void Debug(string message);

        /// <summary>
        /// 情報レベルのログを記録します
        /// </summary>
        /// <param name="message">メッセージ</param>
        void Info(string message);

        /// <summary>
        /// 警告レベルのログを記録します
        /// </summary>
        /// <param name="message">メッセージ</param>
        void Warning(string message);

        /// <summary>
        /// エラーレベルのログを記録します
        /// </summary>
        /// <param name="message">メッセージ</param>
        void Error(string message);

        /// <summary>
        /// 重大エラーレベルのログを記録します
        /// </summary>
        /// <param name="message">メッセージ</param>
        void Critical(string message);

        /// <summary>
        /// 最近のログエントリを取得します
        /// </summary>
        /// <param name="count">取得する件数</param>
        /// <returns>ログエントリのリスト</returns>
        IEnumerable<string> GetRecentLogs(int count = 100);
    }
}
