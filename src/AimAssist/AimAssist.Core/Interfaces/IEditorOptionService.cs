using Common.UI.Editor;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// エディター設定サービスのインターフェース
    /// </summary>
    public interface IEditorOptionService
    {
        /// <summary>
        /// エディター設定
        /// </summary>
        EditorOption Option { get; }

        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        string OptionPath { get; }

        /// <summary>
        /// 設定をロードする
        /// </summary>
        void LoadOption();

        /// <summary>
        /// 設定を保存する
        /// </summary>
        void SaveOption();
    }
}
