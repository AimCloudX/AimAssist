using AimAssist.Core.Commands;
using AimAssist.Core.Interfaces;
using Common;
using Common.Commands.Shortcus;
using Library.Options;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AimAssist.UI.Options
{
    /// <summary>
    /// CustomizeKeyboardShortcutsSettings.xaml の相互作用ロジック
    /// </summary>
    public partial class CustomizeKeyboardShortcutsSettings : System.Windows.Controls.UserControl
    {
        private readonly ICommandService _commandService;

        public CustomizeKeyboardShortcutsSettings(ICommandService commandService)
        {
            _commandService = commandService;
            InitializeComponent();
            foreach (var shortcut in _commandService.GetKeymap())
            {
                this.ShortcutSettings.Add(new ShortcutSource(shortcut.Key, shortcut.Value));
            }

            this.DataContext = this;
        }

        private bool _isFirstKeyEntered = false;
        private void ShortcutTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is ShortcutSource setting)
            {
                // リセット
                _isFirstKeyEntered = false;
            }
        }
        private void ShortcutTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
            if(e.Key == Key.Enter)
            {
                ApplyKey();
                return;
            }

            if (sender is TextBox textBox && textBox.DataContext is ShortcutSource setting)
            {
                var key = e.Key == Key.System ? e.SystemKey : e.Key;
                var modifiers = Keyboard.Modifiers;

                // モディファイアキーのみの場合は無視
                if (key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftAlt ||
                    key == Key.RightAlt || key == Key.LeftShift || key == Key.RightShift|| key == Key.Escape)
                {
                    return;
                }

                if(modifiers == ModifierKeys.None)
                {
                    return;
                }

                if (!_isFirstKeyEntered)
                {
                    // 最初のキー入力
                    setting.Gesture = new KeySequence(key, modifiers);
                    textBox.Text = setting.Gesture.ToString();
                    _isFirstKeyEntered = true;
                }
                else
                {
                    // 2つ目のキー入力
                    setting.Gesture = new KeySequence(setting.Gesture.FirstKey, setting.Gesture.FirstModifiers, key, modifiers);
                    textBox.Text = setting.Gesture.ToString();
                    _isFirstKeyEntered = false; // リセット
                }
            }
            else{

            }
        }

        public ObservableCollection<ShortcutSource> ShortcutSettings { get; set; } = new ObservableCollection<ShortcutSource>();

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
            ApplyKey();
        }

        private void ApplyKey()
        {
            var modifiedShortcutes = ShortcutSettings.Where(x => x.IsModified);
            if (modifiedShortcutes.Any())
            {
                foreach (var shortcut in modifiedShortcutes)
                {
                    _commandService.UpdateKeyGesture(shortcut.CommandName, shortcut.Gesture);
                }
            }
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
