
using AimAssist.Commands;
using AimAssist.HotKeys;
using System.Windows;
using System.Windows.Input;

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
            this.hotkeyController.Register(ModifierKeys.Alt,
                                  Key.A,
                                  (_, __) =>
                                      {
                                          AimAssistCommands.ToggleAssistWindowCommand.Execute(this);
                                      });

            this.hotkeyController.Register(ModifierKeys.Alt,
                                  Key.P,
                                  (_, __) =>
                                      {
                                          AimAssistCommands.ShowPickerWIndowCommand.Execute(this);
                                      });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // HotKeyの登録解除
            this.hotkeyController.Dispose();
        }

    }
}
