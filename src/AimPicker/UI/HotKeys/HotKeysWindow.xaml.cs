
using AimPicker.Service;
using AimPicker.Service.HotKeys;
using System.Windows;
using System.Windows.Input;

namespace AimPicker.UI.Tools.HotKeys
{
    public partial class HowKeysWindow : Window
    {
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
                                          new SnippetManager().RunSnippetTool();
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
