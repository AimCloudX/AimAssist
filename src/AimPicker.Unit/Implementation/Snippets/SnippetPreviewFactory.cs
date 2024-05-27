using AimPicker.Unit.Core;
using System.Windows;

namespace AimPicker.Combos.Mode.Snippet
{
    public class SnippetPreviewFactory : IPreviewFactory
    {
        public bool IsKeepUiElement => false;

        public UIElement Create(IUnit unit)
        {
            return new System.Windows.Controls.TextBox()
            {
                Text = unit.Text,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0)
            };
        }
    }

}
