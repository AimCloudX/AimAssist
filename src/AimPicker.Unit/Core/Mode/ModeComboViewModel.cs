using AimPicker.Combos;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimPicker.Unit.Core.Mode
{
    public class ModeComboViewModel : IUnitViewModel
    {
        private readonly ModeUnit combo;

        public ModeComboViewModel(ModeUnit combo)
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
            return new System.Windows.Controls.TextBox()
            {
                Text = combo.PickerMode.Description,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0)
            };
        }
    }
}