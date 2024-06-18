using AimAssist.Core.Commands;
using AimAssist.Core.Options;
using System.Windows;
using System.Windows.Input;

namespace AimAssist.UI.Options
{
    /// <summary>
    /// CustomizeKeyboardShortcutsSettings.xaml の相互作用ロジック
    /// </summary>
    public partial class CustomizeKeyboardShortcutsSettings : System.Windows.Controls.UserControl
    {
        public CustomizeKeyboardShortcutsSettings()
        {
            InitializeComponent();
            foreach (var shortcut in CommandService.Commands)
            {
                this.ShortcutSetting.Add(new ShortcutSource(shortcut.Key.CommandName, shortcut.Value));
            }

            this.DataContext = this;
        }

        public List<ShortcutSource> ShortcutSetting { get; set; } = new List<ShortcutSource>();

        private static string GetKeyString(Key key, ModifierKeys modifierKeys)
        {
            if (modifierKeys == ModifierKeys.None)
            {
                return key.ToString();
            }

            var modifierKeysConverter = new ModifierKeysConverter();
            var convertToString = modifierKeysConverter.ConvertToString(modifierKeys);

            if (key == Key.LeftCtrl || key == Key.LeftShift || key == Key.RightCtrl || key == Key.LeftAlt
                || key == Key.RightShift || key == Key.RightAlt)
            {
                return convertToString;
            }

            return convertToString + "+" + key;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            // save
            // 更新
        }


        private void UIElement_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox == false)
            {
                return;
            }

            if (e.Key == Key.Apps)
            {
                return;
            }

            if (e.Key == Key.Tab)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.None || e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                {
                    return;
                }
            }

            if (e.Key == Key.Escape || e.Key == Key.Space || e.Key == Key.Enter)
            {
                return;
            }

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    textBox.Text = string.Empty;
                    e.Handled = true;
                    return;
                }
            }

            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var text = GetKeyString(key, e.KeyboardDevice.Modifiers);
            textBox.Text = text;
            e.Handled = true;
        }
    }
}
