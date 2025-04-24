using Common.Commands.Shortcus;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// アプリケーション設定の管理インターフェース
    /// </summary>
    public interface ISettingManager
    {
        /// <summary>
        /// 設定を保存するメソッド
        /// </summary>
        /// <param name="settings">保存する設定のディクショナリ</param>
        void SaveSettings(Dictionary<string, KeySequence> settings);

        /// <summary>
        /// 設定を読み込むメソッド
        /// </summary>
        /// <returns>読み込んだ設定のディクショナリ</returns>
        Dictionary<string, KeySequence> LoadSettings();
    }
}
