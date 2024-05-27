using System.Windows;
using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Snippets;

namespace AimPicker.Combos.Mode.Snippet
{
    public class SnippetUnit : IUnit
    {
        public SnippetUnit(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public string Name { get; }

        public string Text { get; }

        public IPickerMode Mode => SnippetMode.Instance;

        public string Category => string.Empty;

        public UIElement GetUiElement()
        {
            return new SnippetPreviewFactory().Create(this);
        }
    }
}
