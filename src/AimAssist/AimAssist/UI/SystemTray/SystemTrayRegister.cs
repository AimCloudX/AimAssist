
using AimAssist.Core.Commands;
using AimAssist.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.UI.SystemTray
{
    internal class SystemTrayRegister
    {
        public static void Register() 
        { 
            var menu = new ContextMenuStrip();
            menu.Items.Add("Show PickerWindow", null, Show_Click);
            menu.Items.Add("Quit AimAssist", null, Exit_Click);
            var icon = App.GetResourceStream(new Uri("Resources/Icons/AimAssist.ico", UriKind.Relative)).Stream;
            var notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = new Icon(icon),
                Text = "AimAssist",
                ContextMenuStrip = menu,
            };
            notifyIcon.MouseClick += new MouseEventHandler(NotifyIcon_Click);
        }
        private static void Show_Click(object? sender, EventArgs e)
        {
            var appCommands = ((App)App.Current)._serviceProvider.GetRequiredService<IAppCommands>();
            appCommands.ToggleMainWindow.Execute(null);
        }

        private static void NotifyIcon_Click(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var appCommands = ((App)App.Current)._serviceProvider.GetRequiredService<IAppCommands>();
                appCommands.ToggleMainWindow.Execute(null);
            }
        }

        private static void Exit_Click(object? sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

    }
}
