using System.Windows;

namespace AimPicker.Combos.Mode.Snippet
{
    public class SnippetPreviewFactory : IPreviewFactory
    {
        public bool IsKeepUiElement => false;

        public UIElement Create(IUnitViewModel combo)
        {
            return new System.Windows.Controls.TextBox()
            {
                Text = combo.Text,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0)
            };
        }
    }

}
