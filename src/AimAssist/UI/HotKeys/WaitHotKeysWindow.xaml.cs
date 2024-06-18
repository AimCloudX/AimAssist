
using AimAssist.Core.Commands;
using AimAssist.HotKeys;
using System.Windows;

namespace AimAssist.UI.Tools.HotKeys
{
    public partial class WaitHowKeysWindow : Window
    {
        private HotKeyController hotkeyController;
        public WaitHowKeysWindow()
        {
            this.InitializeComponent();
            this.Visibility = Visibility.Hidden;
            this.ShowInTaskbar = false;

            // HotKeyの登録
            this.hotkeyController = new HotKeyController(this);
            RegisterHotKey(CommandNames.ToggleMainWindow);
            RegisterHotKey(CommandNames.ShowPickerWindow);
        }

        private void RegisterHotKey(string commandName)
        {
            if (CommandService.TryGetKeyGesutre(commandName, out var command, out var keyGesture))
            {
                    this.hotkeyController.Register(keyGesture.Modifiers,
                                          keyGesture.Key,
                                              command
                                          );
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // HotKeyの登録解除
            this.hotkeyController.Dispose();
        }
    }
}
