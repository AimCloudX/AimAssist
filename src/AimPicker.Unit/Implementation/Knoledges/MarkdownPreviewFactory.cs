using AimPicker.UI.Combos;
using System.Windows;

namespace AimPicker.Combos.Mode.Wiki
{
    public class MarkdownPreviewFactory
    {
        public UIElement Create(string path)
        {
            return new MarkdownView(path);
        }
    }
}
