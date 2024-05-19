using System.Windows;
using AimPicker.UI.Combos;

namespace AimPicker.UI.Combos.Snippets
{
    public class SnippetPreviewFactory : IPreviewFactory
    {
        public UIElement Create(IComboViewModel combo)
        {
            return new System.Windows.Controls.TextBox() { Text = combo.Description };
        }
    }

}
