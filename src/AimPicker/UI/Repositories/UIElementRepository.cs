using AimPicker.UI.Combos.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AimPicker.UI.Repositories
{
    public static class UIElementRepository
    {
        private static Dictionary<string, UIElement> _elements = new Dictionary<string, UIElement>();

        public static PreviewWindow PreviewWindow { get; set; }

        public static UIElement GetUIElement(PickerCommandViewModel pickerCommandViewModel)
        {
            if(_elements.ContainsKey(pickerCommandViewModel.Name))
            {
                return _elements[pickerCommandViewModel.Name];
            }

            var uiElement = pickerCommandViewModel.Create();
            _elements.Add(pickerCommandViewModel.Name, uiElement);

            return uiElement;
        }
    }
}
