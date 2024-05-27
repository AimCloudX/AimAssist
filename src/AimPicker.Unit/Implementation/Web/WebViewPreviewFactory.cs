using AimPicker.Combos;
using AimPicker.UI.Combos.Commands;
using AimPicker.Unit.Core;
using Common.UI;
using System.Windows;

namespace AimPicker.Unit.Implementation.Web
{
    public class WebViewPreviewFactory : IPreviewFactory
    {
        public bool IsKeepUiElement => true;

        public UIElement Create(IUnit combo)
        {
            if (combo.Text.StartsWith("https://www.amazon"))
            {
                return new AmazonWebViewControl(combo.Text);
            }

            return new WebViewControl(combo.Text);
        }
    }
}
