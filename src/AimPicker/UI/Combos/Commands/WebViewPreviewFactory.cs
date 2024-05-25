using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AimPicker.UI.Combos.Commands
{
    public class WebViewPreviewFactory : IPreviewFactory
    {
        public bool IsKeepUiElement => true;

        public UIElement Create(IComboViewModel combo)
        {
            return new WebViewControl(combo.Text);
        }
    }
}
