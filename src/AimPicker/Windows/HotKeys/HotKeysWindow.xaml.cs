
using AimPicker.Commands;
using AimPicker.HotKeys;
using AimPicker.Service;
using System.Windows;
using System.Windows.Input;

namespace AimPicker.UI.Tools.HotKeys
{
    public partial class HowKeysWindow : Window
    {
        private bool isPickerServiceActivated;
        private HotKeyController hotkeyController;
        public HowKeysWindow()
        {
            this.InitializeComponent();
            this.Visibility = Visibility.Hidden;
            this.ShowInTaskbar = false;

           // HotKeyの登録
            this.hotkeyController = new HotKeyController(this);
            this.hotkeyController.Register( ModifierKeys.Alt,
                                  Key.P,
                                  (_, __) =>
                                      {
                                          PickerCommands.ShowWindowCommand.Execute(this);
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
