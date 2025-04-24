namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// アプリケーションログサービスのインターフェース
    /// </summary>
    public interface IApplicationLogService
    {
        /// <summary>
        /// サービスを初期化します
        /// </summary>
        void Initialize();

        /// <summary>
        /// ログメッセージを記録します
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        void Log(string message);
    }
}
