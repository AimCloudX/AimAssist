using AimPicker.Domain;
using AimPicker.DomainModels;
using AimPicker.Service;
using AimPicker.Service.Plugins;
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
            var pluginService = new PluginsService();
            pluginService.LoadCommandPlugins();
            var combos = pluginService.GetCombos();
            foreach (var item in combos)
            {
                if (item is SnippetCombo snippet)
                {
                    ComboService.ComboDictionary2[SnippetMode.Instance].Add(snippet);
                }

                if (item is WorkFlowCombo command)
                {
                    ComboService.ComboDictionary2[WorkFlowMode.Instance].Add(command);
                }
            }

            var window = new HowKeysWindow();
            window.Show();
        }
    }

}
