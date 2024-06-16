using AimAssist.Core.Editors;
using AimAssist.Core.Options;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Standard;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Options
{
    public class OptionUnit : IUnit
    {
        public OptionUnit(string name, string category)
        {
            Name = name;
            Category = category;
        }
        public BitmapImage Icon => new BitmapImage();

        public IPickerMode Mode => StandardMode.Instance;

        public string Category { get; }

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
