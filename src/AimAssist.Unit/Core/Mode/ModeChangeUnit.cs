using AimAssist.Combos;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Core.Mode
{
    public class ModeChangeUnit : IUnit
    {
        private readonly IPickerMode pickerMode;

        public ModeChangeUnit(IPickerMode combo)
        {
            this.pickerMode = combo;
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Ap.ico"));
        }

        public string Name => pickerMode.Name;

        public string Text => pickerMode.Prefix;
        public string Category => "Mode";

        public BitmapImage Icon { get; set; }

        public IPickerMode Mode => this.Mode;

        public UIElement GetUiElement()
        {
            return new System.Windows.Controls.TextBox()
            {
                Text = this.pickerMode.Description,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0)
            };
        }
    }
}