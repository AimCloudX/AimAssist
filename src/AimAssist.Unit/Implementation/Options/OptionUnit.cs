using AimAssist.Core.Editors;
using AimAssist.Core.Options;
using AimAssist.Core.Rsources;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Options
{
    public class OptionUnit : IUnit
    {
        public OptionUnit(string name)
        {
            Name = name;
        }

        public BitmapImage Icon => Constants.AimAssistIco;

        public IPickerMode Mode => OptionMode.Instance;

        public string Category => string.Empty;

        public string Name { get; }

        public string Text => string.Empty;

        public UIElement GetUiElement()
        {
            var editor =  new AimEditor();
            editor.NewTab(EditorOptionService.OptionPath);
            if (File.Exists(EditorOptionService.Option.CustomVimKeybindingPath))
            {
                editor.NewTab(EditorOptionService.Option.CustomVimKeybindingPath);
            }

            return editor;
        }
    }
}
