using System.Windows;
using System.Windows.Media.Imaging;

namespace AimPicker.UI.Combos.Snippets
{
    public class SnippetViewModel : IComboViewModel
    {
        public SnippetViewModel(string name, string text)
        {
            Name = name;
            Snippet = text;
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Snippet.ico"));
        }

        public string Name { get; }

        public string Snippet { get; }

        public string Text => GetSnippetText;

        public string GetSnippetText => Snippet;
        public BitmapImage Icon { get; set; }

        public IPreviewFactory Factory => new SnippetPreviewFactory();
        public UIElement Create()
        {
            return Factory.Create(this);
        }
    }

}
