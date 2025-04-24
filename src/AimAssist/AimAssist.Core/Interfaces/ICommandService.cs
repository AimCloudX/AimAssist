using Common.Commands;
using Common.Commands.Shortcus;
using System.Windows;
using System.Windows.Input;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// コマンド管理サービスのインターフェース
    /// </summary>
    public interface ICommandService
    {
        /// <summary>
        /// キーマップを設定します
        /// </summary>
        /// <param name="maps">キーマップのディクショナリ</param>
        void SetKeymap(Dictionary<string, KeySequence> maps);

        /// <summary>
        /// 現在のキーマップを取得します
        /// </summary>
        /// <returns>キーマップのディクショナリ</returns>
        Dictionary<string, KeySequence> GetKeymap();

        /// <summary>
        /// 最初のキーのみのコマンドを取得します
        /// </summary>
        /// <param name="keyGesture">キージェスチャー</param>
        /// <param name="command">取得したコマンド</param>
        /// <returns>コマンドが見つかった場合はtrue、それ以外はfalse</returns>
        bool TryGetFirstOnlyKeyCommand(KeyGesture keyGesture, out RelayCommand command);

        /// <summary>
        /// 1次、2次キーを使用するコマンドを取得します
        /// </summary>
        /// <param name="input">入力されたキーシーケンス</param>
        /// <param name="command">取得したコマンド</param>
        /// <returns>コマンドが見つかった場合はtrue、それ以外はfalse</returns>
        bool TryGetFirstSecontKeyCommand(KeySequence input, out RelayCommand command);

        /// <summary>
        /// コマンドを登録します
        /// </summary>
        /// <param name="command">登録するコマンド</param>
        /// <param name="defaultKeyMap">デフォルトのキーマップ</param>
        void Register(RelayCommand command, KeySequence defaultKeyMap);

        /// <summary>
        /// コマンド名からコマンドを取得します
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <param name="command">取得したコマンド</param>
        /// <returns>コマンドが見つかった場合はtrue、それ以外はfalse</returns>
        bool TryGetCommand(string commandName, out RelayCommand command);

        /// <summary>
        /// キージェスチャーを更新します
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <param name="key">新しいキーシーケンス</param>
        void UpdateKeyGesture(string commandName, KeySequence key);

        /// <summary>
        /// コマンド名からコマンドとキージェスチャーを取得します
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <param name="command">取得したコマンド</param>
        /// <param name="keyGesture">関連するキージェスチャー</param>
        /// <returns>コマンドが見つかった場合はtrue、それ以外はfalse</returns>
        bool TryGetKeyGesutre(string commandName, out RelayCommand command, out KeySequence keyGesture);

        /// <summary>
        /// コマンドを実行します
        /// </summary>
        /// <param name="commandName">コマンド名</param>
        /// <param name="window">実行対象のウィンドウ</param>
        void Execute(string commandName, Window window);
    }
}
