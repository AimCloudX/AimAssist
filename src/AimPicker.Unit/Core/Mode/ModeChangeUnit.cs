using AimPicker.Combos;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimPicker.Unit.Core.Mode
{
    public class ModeChangeUnit : IUnit
    {
        private readonly IPickerMode combo;

        public ModeChangeUnit(IPickerMode combo)
        {
            this.combo = combo;
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/Ap.ico"));
        }

        public string Name => combo.Name;

        public string Text => combo.Description;
        public string Category => "Mode";

        public BitmapImage Icon { get; set; }

        public IPreviewFactory PreviewFactory => new ModePreviewFactory();

        public class ModePreviewFactory : IPreviewFactory
        {
            public UIElement Create(IUnit combo)
            {
                return new System.Windows.Controls.TextBox()
                {
                    Text = combo.Text,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0)
                };
            }
        }
    }
}