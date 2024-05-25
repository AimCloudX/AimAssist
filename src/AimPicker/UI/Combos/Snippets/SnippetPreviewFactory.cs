using System.Windows;

namespace AimPicker.UI.Combos.Snippets
{
    public class SnippetPreviewFactory : IPreviewFactory
    {
        public bool IsKeepUiElement => false;

        public UIElement Create(IComboViewModel combo)
        {
            return new System.Windows.Controls.TextBox() {
                Text = combo.Text,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0)
            };
        }
    }

}
