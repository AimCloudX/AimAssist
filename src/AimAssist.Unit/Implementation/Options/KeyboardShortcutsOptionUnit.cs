using AimAssist.Core.Rsources;
using AimAssist.UI.Options;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Options
{
    public class KeyboardShortcutsOptionUnit : IUnit
    {
        public BitmapImage Icon => Constants.AimAssistIco;

        public IPickerMode Mode => OptionMode.Instance;

        public string Category => string.Empty;

        public string Name => "KeyboardShortcut Settings";

        public string Text => "キーボードショートカット設定";

        public UIElement GetUiElement()
        {
            return new CustomizeKeyboardShortcutsSettings();
        }
    }
}
