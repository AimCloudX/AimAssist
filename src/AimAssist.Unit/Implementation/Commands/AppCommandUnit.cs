using AimAssist.Core.Commands;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Standard;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Commands
{
    public class AppCommandUnit : ICommandUnit
    {
        public BitmapImage Icon => new BitmapImage();

        public IMode Mode => AllInclusiveMode.Instance;

        public string Category => string.Empty;

        public string Name => "AimAssist Close";

        public string Text => "AimAssistを終了する";

        public void Execute()
        {
            CommandService.Execute(CommandNames.ShutdownAimAssist);
        }

        public UIElement GetUiElement()
        {
            return new System.Windows.Controls.TextBox()
            {
                Text = this.Text,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0)
            };
        }
    }
}
