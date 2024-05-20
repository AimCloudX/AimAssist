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
            uiElement = Factory.Create(this);

        }

        public string Description { get; }
        public IPreviewFactory Factory { get; }

        private UIElement uiElement { get; set; }

        public UIElement Create()
        {
            return uiElement;
        }
    }
}
