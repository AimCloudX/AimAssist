using AimPicker.UI.Tools.HotKeys;
using System.Runtime.CompilerServices;

namespace AimPicker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            var window = new HowKeysWindow();
            window.Show();
        }
    }

}
