using AimAssist.Combos;
using AimAssist.UI;
using AimAssist.UI.Combos.Commands;
using AimAssist.Unit.Implementation.Web.Urls;
using Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AimAssist.WebViewCash
{
    public static class UIElementRepository
    {
        private static Dictionary<string, UIElement> _elements = new Dictionary<string, UIElement>();

        private static WebViewControl webView;
        private static AmazonWebViewControl amazonwebView;

        public static string RescentText { get; set; }

        public static PreviewWindow PreviewWindow { get; set; }

        public static UIElement GetUIElement(string workName)
        {
            if (_elements.ContainsKey(workName))
            {
                return _elements[workName];
            }

            return null;
        }

        internal static void RegisterUIElement(string workName, UIElement uiElement)
        {
            _elements[workName] = uiElement;
        }

        internal static UIElement GetWebViewControl(string url)
        {
            if (url.StartsWith("https://www.amazon"))
            {
                if (amazonwebView == null)
                {
                    amazonwebView = new AmazonWebViewControl(url);
                    return amazonwebView;
                }
                amazonwebView.Url = url;
                return amazonwebView;
            }

            if(webView == null)
            {
                webView = new WebViewControl(url);
                return webView;
            }

            webView.Url = url;
            return webView;
        }
    }
}
