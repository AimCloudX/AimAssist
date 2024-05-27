using AimPicker.Unit.Core;

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
    }
}
