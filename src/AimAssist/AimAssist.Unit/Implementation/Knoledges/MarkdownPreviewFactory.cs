using AimAssist.UI.Combos;
using System.Windows;

namespace AimAssist.Combos.Mode.Wiki
{
    public class MarkdownPreviewFactory
    {
        public UIElement Create(string path)
        {
            return new MarkdownView(path);
        }
    }
}
