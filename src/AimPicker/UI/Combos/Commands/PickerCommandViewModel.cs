using AimPicker.UI.Repositories;
using System.Windows;

namespace AimPicker.UI.Combos.Commands
{
    public class PickerCommandViewModel : IComboViewModel
    {
        public string Name { get; }

        public PickerCommandViewModel(string name, string description, IPreviewFactory factory)
        {
            Name = name;
            Description = description;
            Factory = factory;
            uiElement = UIElementRepository.GetUIElement(this);
        }

        public string Description { get; }
        public IPreviewFactory Factory { get; }

        private UIElement uiElement;

        public UIElement Create()
        {
            if(uiElement == null)
            {
                uiElement = Factory.Create(this);
            }

           return  uiElement;
        }
    }
}
