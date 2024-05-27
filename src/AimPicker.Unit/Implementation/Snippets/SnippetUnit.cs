using System.Windows;
using AimPicker.Unit.Core;

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

        public IPreviewFactory PreviewFactory => new SnippetPreviewFactory();
    }
}
