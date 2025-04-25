using System.Threading.Tasks;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// チートシート制御のためのインターフェース
    /// </summary>
    public interface ICheatSheetController
    {
        /// <summary>
        /// チートシートを表示する
        /// </summary>
        /// <param name="processName">対象プロセス名</param>
        Task ShowCheatSheet(string processName);

        /// <summary>
        /// チートシートをアプリケーション名で表示する
        /// </summary>
        /// <param name="applicationName">アプリケーション名</param>
        Task ShowCheatSheetByApplicationName(string applicationName);

        /// <summary>
        /// チートシートを閉じる
        /// </summary>
        void CloseCheatSheet();
    }
}
