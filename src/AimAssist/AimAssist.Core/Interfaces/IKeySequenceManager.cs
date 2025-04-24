using System.Windows;
using System.Windows.Input;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// キーシーケンスを管理するインターフェース
    /// </summary>
    public interface IKeySequenceManager
    {
        /// <summary>
        /// キー入力を処理するメソッド
        /// </summary>
        /// <param name="key">入力されたキー</param>
        /// <param name="modifiers">修飾キー</param>
        /// <param name="window">ウィンドウ</param>
        /// <returns>キー入力が処理された場合はtrue、それ以外はfalse</returns>
        bool HandleKeyPress(Key key, ModifierKeys modifiers, Window window);
    }
}
