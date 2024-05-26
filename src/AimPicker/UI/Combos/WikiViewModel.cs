using System.Windows;

namespace AimPicker.UI.Combos
{
    public class WikiViewModel : IComboViewModel
    {
        public string Name { get; }

        public WikiViewModel(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public string Text { get; }

        public UIElement Create()
        {

            return new MarkdownView(this.Text);
        }
    }
}
