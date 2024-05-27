using AimPicker.Unit.Core;
using AimPicker.Unit.Implementation.Web.Urls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AimPicker.WebViewCash
{
    internal static class UnitExteinsions
    {
        public static UIElement GetOrCreateUiElemnt(this IUnit unit)
        {
            if (unit.Text.StartsWith("https:"))
            {
                var uiElement = UIElementRepository.GetUIElement(unit.Text);
                if (uiElement == null)
                {
                    uiElement = unit.GetUiElement();
                    UIElementRepository.RegisterUIElement(unit.Text, uiElement);
                    return uiElement;
                }

                return uiElement;
            }

            return unit.GetUiElement();
        }
    }
}
