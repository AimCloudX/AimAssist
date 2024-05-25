using AimPicker.DomainModels;
using System.Windows;

namespace AimPicker.UI.Combos
{
    public class ModeComboViewModel : IComboViewModel
    {
        private readonly ModeCombo combo;

        public ModeComboViewModel(ModeCombo combo)
        {
            this.combo = combo;
        }

        public string Name => combo.Name;

        public string Text => combo.Text;

        public UIElement Create()
        {
            return new System.Windows.Controls.TextBox() {
                Text = combo.PickerMode.Description,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0)
            };
        }
    }
}