using AimPicker.DomainModels;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimPicker.UI.Combos
{
    public class ModeComboViewModel : IComboViewModel
    {
        private readonly ModeCombo combo;

        public ModeComboViewModel(ModeCombo combo)
        {
            this.combo = combo;
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Ap.ico"));
        }

        public string Name => combo.Name;

        public string Text => combo.Text;
        public string Category => "Mode";

        public BitmapImage Icon { get; set; }

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