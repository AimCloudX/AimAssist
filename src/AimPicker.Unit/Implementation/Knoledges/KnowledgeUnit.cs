using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Wiki;
using System.Windows;

namespace AimPicker.Combos.Mode.Wiki
{
    public class KnowledgeUnit : IUnit
    {
        public string Name { get; }

        public KnowledgeUnit(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public string Text { get; }

        public IPreviewFactory PreviewFactory => new MarkdownPreviewFactory();

        public IPickerMode Mode => KnowledgeMode.Instance;

        public string Category => string.Empty;

        public UIElement GetUiElement()
        {
            return new MarkdownPreviewFactory().Create(this);
        }
    }
}
