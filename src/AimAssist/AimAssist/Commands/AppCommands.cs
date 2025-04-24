using AimAssist.Core.Interfaces;
using AimAssist.Service;
using Common.Commands;

namespace AimAssist.Core.Commands
{
    /// <summary>
    /// アプリケーションのコマンドを定義するクラス
    /// </summary>
    public class AppCommands : IAppCommands
    {
        private readonly WindowHandleService _windowHandleService;
        private readonly PickerService _pickerService;

        /// <summary>
        /// AppCommandsのコンストラクタ
        /// </summary>
        /// <param name="windowHandleService">ウィンドウハンドルサービス</param>
        /// <param name="pickerService">ピッカーサービス</param>
        public AppCommands(WindowHandleService windowHandleService, PickerService pickerService)
        {
            _windowHandleService = windowHandleService;
            _pickerService = pickerService;
        }

        /// <summary>
        /// アプリケーションをシャットダウンするコマンド
        /// </summary>
        public RelayCommand ShutdownAimAssist => new RelayCommand(nameof(ShutdownAimAssist), (_) => App.Current.Shutdown());
        
        /// <summary>
        /// メインウィンドウを切り替えるコマンド
        /// </summary>
        public HotkeyCommand ToggleMainWindow { get; private set; }
        
        /// <summary>
        /// ピッカーウィンドウを表示するコマンド
        /// </summary>
        public HotkeyCommand ShowPickerWindow { get; private set; }
        
        /// <summary>
        /// コマンドを初期化します
        /// </summary>
        public void Initialize()
        {
            ToggleMainWindow = new HotkeyCommand(nameof(ToggleMainWindow), (_) => _windowHandleService.ToggleMainWindow());
            ShowPickerWindow = new HotkeyCommand(nameof(ShowPickerWindow), (_) => _pickerService.ShowPickerWindow());
        }
    }
}
