using System.Windows;

namespace AimPicker.UI.Combos.Snippets
{
    public class SnippetViewModel : IComboViewModel
    {
        public SnippetViewModel(string name, string text)
        {
            Name = name;
            Snippet = text;
        }

        public string Name { get; }

        public string Snippet { get; }

        public string Description => GetSnippetText;

        public string GetSnippetText => Snippet;

        public IPreviewFactory Factory => new SnippetPreviewFactory();
        public UIElement Create()
        {
            return Factory.Create(this);
        }
    }

}
