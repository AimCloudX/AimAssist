using AimAssist.Service;
using Common.Commands;

namespace AimAssist.Core.Commands
{
    /// <summary>
    /// アプリケーションのコマンドを定義するクラス
    /// </summary>
    public class AppCommands
    {
        /// <summary>
        /// アプリケーションをシャットダウンするコマンド
        /// </summary>
        public static RelayCommand ShutdownAimAssist => new RelayCommand(nameof(ShutdownAimAssist), (_) => App.Current.Shutdown());
        
        /// <summary>
        /// メインウィンドウを切り替えるコマンド
        /// </summary>
        public static HotkeyCommand ToggleMainWindow { get; private set; }
        
        /// <summary>
        /// ピッカーウィンドウを表示するコマンド
        /// </summary>
        public static HotkeyCommand ShowPickerWindow { get; private set; }
        
        /// <summary>
        /// コマンドを初期化します
        /// </summary>
        /// <param name="windowHandleService">ウィンドウハンドルサービス</param>
        /// <param name="pickerService">ピッカーサービス</param>
        public static void Initialize(WindowHandleService windowHandleService, PickerService pickerService)
        {
            ToggleMainWindow = new HotkeyCommand(nameof(ToggleMainWindow), (_) => windowHandleService.ToggleMainWindow());
            ShowPickerWindow = new HotkeyCommand(nameof(ShowPickerWindow), (_) => pickerService.ShowPickerWindow());
        }
    }
}
