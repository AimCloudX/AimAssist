using AimAssist.Core.Commands;
using AimAssist.Plugins;
using AimAssist.Service;
using AimAssist.UI.SystemTray;
using AimAssist.UI.Tools.HotKeys;
using AimAssist.UI.UnitContentsView;
using AimAssist.Units.Implementation;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.WorkTools;
using Common.Commands.Shortcus;
using Library.Options;
using System.IO;

namespace AimAssist
{
    internal class Initializer
    {
        public void Initialize()
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string editorOptionPath = Path.Combine(roamingPath, "AimAssist", "editor.option.json");
            string editorOptionSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Settings", "editor.option.json");
            if (!File.Exists(editorOptionPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(editorOptionPath));
                File.Copy(editorOptionSource, editorOptionPath);
            }

            string workItemOptionSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "WorkItems", "workitem.option.json");
            if (!File.Exists(WorkItemOptionService.OptionPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(WorkItemOptionService.OptionPath));
                File.Copy(workItemOptionSource, WorkItemOptionService.OptionPath);
            }

            string targetPath = Path.Combine(roamingPath, "AimAssist", "WorkItem.md");
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "WorkItems", "WorkItem.md");
            if (!File.Exists(targetPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                File.Copy(sourcePath, targetPath);
            }

            WorkItemOptionService.LoadOption();

            string snippetoption = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Snippets", "snippet.option.json");
            if (!File.Exists(SnippetOptionServce.OptionPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SnippetOptionServce.OptionPath));
                File.Copy(snippetoption, SnippetOptionServce.OptionPath);
            }

            string snippetDefault = Path.Combine(roamingPath, "AimAssist", "standard.md");
            string snippetSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Snippets", "standard.md");
            if (!File.Exists(snippetDefault))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(snippetDefault));
                File.Copy(snippetSource, snippetDefault);
            }

            SnippetOptionServce.LoadOption();


            EditorOptionService.LoadOption();

            SystemTrayRegister.Register();

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
