using System.Windows;

namespace AimAssist.Combos.Mode.Snippet
{
    public class SnippetPreviewFactory
    {
        public UIElement Create(string code)
        {
            return new System.Windows.Controls.TextBox()
            {
                Text = code,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0)
            };
        }
    }

}
