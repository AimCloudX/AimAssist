using AimPicker.UI.Combos;
using System.Windows;

namespace AimPicker.Combos.Mode.Wiki
{
    public class KnowledgeViewModel : IUnitViewModel
    {
        public string Name { get; }

        public KnowledgeViewModel(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public string Text { get; }

        public UIElement Create()
        {

            return new MarkdownView(Text);
        }
    }
}
