using AimAssist.Core.Interfaces;
using Common.UI.Commands;

namespace AimAssist.Commands
{
    /// <summary>
    /// アプリケーションのコマンドを定義するクラス
    /// </summary>
    public class AppCommands : IAppCommands
    {
        private readonly IWindowHandleService windowHandleService;
        private readonly IPickerService pickerService;

        /// <summary>
        /// AppCommandsのコンストラクタ
        /// </summary>
        /// <param name="windowHandleService">ウィンドウハンドルサービス</param>
        /// <param name="pickerService">ピッカーサービス</param>
        public AppCommands(IWindowHandleService windowHandleService, IPickerService pickerService)
        {
            this.windowHandleService = windowHandleService;
            this.pickerService = pickerService;
        }

        /// <summary>
        /// アプリケーションをシャットダウンするコマンド
        /// </summary>
        public Common.UI.Commands.RelayCommand ShutdownAimAssist => new Common.UI.Commands.RelayCommand(nameof(ShutdownAimAssist), (_) => App.Current.Shutdown());
        
        /// <summary>
        /// メインウィンドウを切り替えるコマンド
        /// </summary>
        public HotkeyCommand ToggleMainWindow { get; private set; } = null!;
        
        /// <summary>
        /// ピッカーウィンドウを表示するコマンド
        /// </summary>
        public HotkeyCommand ShowPickerWindow { get; private set; } = null!;
        
        /// <summary>
        /// コマンドを初期化します
        /// </summary>
        public void Initialize()
        {
            ToggleMainWindow = new HotkeyCommand(nameof(ToggleMainWindow), (_) => windowHandleService.ToggleMainWindow());
            ShowPickerWindow = new HotkeyCommand(nameof(ShowPickerWindow), async (_) => await pickerService.ShowPicker());
        }
    }
}
