using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AimAssist.Core.Interfaces;
using AimAssist.Services.Options;
using Common.UI.Commands.Shortcus;

namespace AimAssist.Units.Implementation.Options
{
    public partial class CustomizeKeyboardShortcutsSettings
    {
        private readonly ICommandService commandService;

        public CustomizeKeyboardShortcutsSettings(ICommandService commandService)
        {
            this.commandService = commandService;
            InitializeComponent();
            foreach (var shortcut in this.commandService.GetKeymap())
            {
                this.ShortcutSettings.Add(new ShortcutSource(shortcut.Key, shortcut.Value));
            }

            this.DataContext = this;
        }

        public CustomizeKeyboardShortcutsSettings() : this(null)
        {
        }

        private bool isFirstKeyEntered;
        private void ShortcutTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is ShortcutSource)
            {
                isFirstKeyEntered = false;
            }
        }
        private void ShortcutTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
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

                if (key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftAlt ||
                    key == Key.RightAlt || key == Key.LeftShift || key == Key.RightShift|| key == Key.Escape)
                {
                    return;
                }

                if(modifiers == ModifierKeys.None)
                {
                    return;
                }

                if (!isFirstKeyEntered)
                {
                    setting.Gesture = new KeySequence(key, modifiers);
                    textBox.Text = setting.Gesture.ToString();
                    isFirstKeyEntered = true;
                }
                else
                {
                    if (setting.Gesture != null)
                    {
                        setting.Gesture = new KeySequence(setting.Gesture.FirstKey, setting.Gesture.FirstModifiers, key,
                            modifiers);
                        textBox.Text = setting.Gesture.ToString();
                    }

                    isFirstKeyEntered = false;
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
            if (convertToString == null)
            {
                return key.ToString();
            }

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
            var shortcutSources = modifiedShortcutes as ShortcutSource[] ?? modifiedShortcutes.ToArray();
            if (shortcutSources.Length == 0) return;
            foreach (var shortcut in shortcutSources)
            {
                commandService.UpdateKeyGesture(shortcut.CommandName, shortcut.Gesture);
            }
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox == false)
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
