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

        public IPickerMode Mode => StandardMode.Instance;

        public string Category => string.Empty;

        public string Name => "AimAssist Shutdown";

        public string Text => "AimAssistを終了する";

        public void Execute()
        {
            AppCommands.AimAssistShutdown.Execute();
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
