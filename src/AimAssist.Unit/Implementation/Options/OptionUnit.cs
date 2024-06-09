using AimAssist.Core.Options;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using Common.UI.Editor;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Options
{
    public class OptionUnit : IUnit
    {
        public OptionUnit(string name, string path, string category)
        {
            Name = name;
            Category = category;
            FilePath = path;
        }
        public BitmapImage Icon => new BitmapImage();

        public IPickerMode Mode => OptionMode.Instance;

        public string Category { get; }

        public string Name { get; }

        public string Text => FilePath;

        public string FilePath { get; }


        public UIElement GetUiElement()
        {
            var editor =  new MonacoEditor();
            editor.MinHeight = 500;
            editor.MinWidth = 500;
            var text = File.ReadAllText(FilePath);
            editor.SetOption(EditorOptionService.Option);
            editor.SetText(text);
            return editor;
        }
    }
}
