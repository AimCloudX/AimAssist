using AimPicker.Combos;
using AimPicker.UI;
using AimPicker.UI.Combos.Commands;
using AimPicker.Unit.Implementation.Web.Urls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AimPicker.WebViewCash
{
    public static class UIElementRepository
    {
        private static Dictionary<string, UIElement> _elements = new Dictionary<string, UIElement>();

        public static string RescentText { get; set; }

        public static PreviewWindow PreviewWindow { get; set; }

        public static UIElement GetUIElement(string urlPath)
        {
            if (_elements.ContainsKey(urlPath))
            {
                return _elements[urlPath];
            }

            return null;
        }

        internal static void RegisterUIElement(string path, UIElement uiElement)
        {
            _elements[path] = uiElement;
        }
    }
}
