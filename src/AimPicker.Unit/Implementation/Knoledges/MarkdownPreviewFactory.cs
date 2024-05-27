using AimPicker.UI.Combos;
using AimPicker.Unit.Core;
using System.Windows;

namespace AimPicker.Combos.Mode.Wiki
{
    public class MarkdownPreviewFactory : IPreviewFactory
    {
        public UIElement Create(IUnit combo)
        {
            return new MarkdownView(combo.Text);
        }
    }
}
