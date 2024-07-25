using AimAssist.Core.Commands;
using AimAssist.Plugins;
using AimAssist.Service;
using AimAssist.UI.SystemTray;
using AimAssist.UI.Tools.HotKeys;
using AimAssist.UI.UnitContentsView;
using AimAssist.Units.Implementation;
using Common.Commands.Shortcus;
using Library.Options;
using System.IO;

namespace AimAssist
{
    internal class Initializer
    {
        public void Initialize()
        {
            EditorOptionService.LoadOption();
            SystemTrayRegister.Register();
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string targetPath = Path.Combine(roamingPath, "AimAssist", "WorkItem.md");
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "WorkItems", "WorkItem.md");
            if (!File.Exists(targetPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                File.Copy(sourcePath, targetPath);
            }

            var unitsService = UnitsService.Instnace;
            unitsService.RegisterUnits(new UnitsFactory());

            var pluginService = new PluginsService();
            pluginService.LoadCommandPlugins();
            var facotries = pluginService.GetFactories();
            foreach (var item in facotries)
            {
                unitsService.RegisterUnits(item);
            }

            var converters = pluginService.GetConterters();
            foreach (var item in converters)
            {
                UnitViewFactory.UnitToUIElementDicotionary.TryAdd(item.Key, item.Value);
            }

            CommandService.Register(AppCommands.ToggleMainWindow, new KeySequence(System.Windows.Input.Key.A, System.Windows.Input.ModifierKeys.Alt));
            CommandService.Register(AppCommands.ShowPickerWindow, new KeySequence(System.Windows.Input.Key.P, System.Windows.Input.ModifierKeys.Alt));
            CommandService.Register(AppCommands.ShutdownAimAssist, new KeySequence(System.Windows.Input.Key.D, System.Windows.Input.ModifierKeys.Control));

            new WaitHowKeysWindow().Show();

            ApplicationLogService.Instance.Initialize();
        }
    }
}
