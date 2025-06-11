using System.Windows;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Snippets;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBox = System.Windows.Controls.TextBox;

namespace AimAssist.Units.ViewProviders.Providers
{
    [ViewProvider(Priority = 80)]
    public class DynamicContentViewProvider : IViewProvider
    {
        public int Priority => 80;

        public bool CanProvideView(Type unitType) =>
            unitType == typeof(SnippetUnit);

        public UIElement CreateView(IItem unit, IServiceProvider serviceProvider)
        {
            var code = unit switch
            {
                SnippetUnit snippet => snippet.Code,
                _ => string.Empty
            };

            return new TextBox
            {
                Text = code,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0),
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                Background = System.Windows.Media.Brushes.LightGray
            };
        }
    }
}
