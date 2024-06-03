using AimAssist.Combos.Mode.WorkFlows;
using AimAssist.Unit.Core;
using System.Windows;

namespace AimAssist.WebViewCash
{
    internal static class UnitExteinsions
    {
        public static UIElement GetOrCreateUiElemnt(this IUnit unit)
        {
            if(unit is WorkFlowUnit work)
            {
                var uiElement = UIElementRepository.GetUIElement(unit.Text);
                if(uiElement == null)
                {
                    uiElement = work.GetUiElement();
                    UIElementRepository.RegisterUIElement(work.Name, uiElement);
                }

                return uiElement;
            }

            if (unit.Text.StartsWith("https:"))
            {
                var uiElement = UIElementRepository.GetWebViewControl(unit.Text);
                return uiElement;
            }

            return unit.GetUiElement();
        }
    }
}
