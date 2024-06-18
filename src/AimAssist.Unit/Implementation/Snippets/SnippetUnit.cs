using System.Windows;
using System.Windows.Media.Imaging;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Snippets;

namespace AimAssist.Combos.Mode.Snippet
{
    public class SnippetUnit : IUnit
    {
        public SnippetUnit(string name, string text, string category = "")
        {
            Name = name;
            Text = text;
            Category = category;
        }

        public BitmapImage Icon => new BitmapImage();
        public string Name { get; }

        public string Text { get; }

        public string Code => this.Text;

        public IPickerMode Mode => SnippetMode.Instance;

        public string Category { get; }

        public UIElement GetUiElement()
        {
            return new SnippetPreviewFactory().Create(this.Code);
        }
    }
}
