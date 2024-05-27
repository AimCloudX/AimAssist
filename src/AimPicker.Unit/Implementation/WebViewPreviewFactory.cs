using AimPicker.Combos;
using AimPicker.UI.Combos.Commands;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Common.UI
{
    public class WebViewPreviewFactory : IPreviewFactory
    {
        public bool IsKeepUiElement => true;

        public UIElement Create(IUnitViewModel combo)
        {
            return new WebViewControl(combo.Text);
        }
    }
}
