using Common.Commands;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// アプリケーションのコマンドを定義するインターフェース
    /// </summary>
    public interface IAppCommands
    {
        /// <summary>
        /// アプリケーションをシャットダウンするコマンド
        /// </summary>
        RelayCommand ShutdownAimAssist { get; }
        
        /// <summary>
        /// メインウィンドウを切り替えるコマンド
        /// </summary>
        HotkeyCommand ToggleMainWindow { get; }
        
        /// <summary>
        /// ピッカーウィンドウを表示するコマンド
        /// </summary>
        HotkeyCommand ShowPickerWindow { get; }
        
        /// <summary>
        /// コマンドを初期化します
        /// </summary>
        void Initialize();
    }
}
