using AimAssist.Combos.Mode.Wiki;
using AimAssist.Unit.Implementation.Knoledges;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Core.Mode
{
    public class ModeChangeUnit : IUnit
    {
        public ModeChangeUnit(IPickerMode combo)
        {
            this.Mode = combo;
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/AimAssist.ico"));
        }

        public string Name => Mode.Name;

        public string Text => Mode.Prefix;
        public string Category => "Mode";

        public BitmapImage Icon { get; set; }

        public IPickerMode Mode { get; }

        public UIElement GetUiElement()
        {
            var markdownView = new MarkdownPreviewFactory().Create("Resources/Knowledge/README.md");
            return new ModeDscriptionControl(this.Mode.Description, markdownView);
        }
    }
}