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
        }

        public string Description { get; }
        public IPreviewFactory Factory { get; }

        public UIElement Create()
        {
            return Factory.Create(this);
        }
    }

}
