using AimAssist.Core.Editors;
using AimAssist.Core.Options;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
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

        public IPickerMode Mode => OptionMode.Instance;

        public string Category { get; }

        public string Name { get; }

        public string Text => string.Empty;

        public UIElement GetUiElement()
        {
            var editor =  new AimEditor();
            editor.NewTab(EditorOptionService.OptionPath);
            if (EditorOptionService.Option.Mode == Common.UI.Editor.EditorMode.Vim)
            {
                editor.NewTab(EditorOptionService.Option.CustomVimKeybindingPath);
            }

            return editor;
        }
    }
}
