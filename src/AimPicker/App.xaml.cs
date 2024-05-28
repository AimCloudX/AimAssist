using AimPicker.Combos.Mode;
using AimPicker.Combos.Mode.Snippet;
using AimPicker.Combos.Mode.WorkFlows;
using AimPicker.Plugins;
using AimPicker.Service;
using AimPicker.UI.Tools.HotKeys;
using AimPicker.Unit.Implementation.Snippets;
using AimPicker.Unit.Implementation.WorkFlows;

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
                if (UnitService.UnitDictionary.ContainsKey(item.Mode))
                {
                    UnitService.UnitDictionary[item.Mode].Add(item);
                }
                else
                {
                    UnitService.UnitDictionary[item.Mode] = new List<Unit.Core.IUnit> { item };
                }
            }

            var window = new HowKeysWindow();
            window.Show();
        }
    }

}
