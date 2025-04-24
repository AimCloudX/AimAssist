using AimAssist.Core.Commands;
using AimAssist.Core.Interfaces;
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
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace AimAssist
{
    /// <summary>
    /// アプリケーションの初期化を行うクラス
    /// </summary>
    internal class Initializer
    {
        private readonly IUnitsService _unitsService;
        private readonly ICommandService _commandService;
        private readonly IApplicationLogService _applicationLogService;
        private readonly IServiceProvider _serviceProvider;
        private readonly WindowHandleService _windowHandleService;
        private readonly PickerService _pickerService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="unitsService">ユニットサービス</param>
        /// <param name="commandService">コマンドサービス</param>
        /// <param name="applicationLogService">アプリケーションログサービス</param>
        /// <param name="serviceProvider">サービスプロバイダー</param>
        /// <param name="windowHandleService">ウィンドウハンドルサービス</param>
        /// <param name="pickerService">ピッカーサービス</param>
        public Initializer(
            IUnitsService unitsService,
            ICommandService commandService,
            IApplicationLogService applicationLogService,
            IServiceProvider serviceProvider,
            WindowHandleService windowHandleService,
            PickerService pickerService)
        {
            _unitsService = unitsService;
            _commandService = commandService;
            _applicationLogService = applicationLogService;
            _serviceProvider = serviceProvider;
            _windowHandleService = windowHandleService;
            _pickerService = pickerService;
        }

        /// <summary>
        /// アプリケーションを初期化します
        /// </summary>
        public void Initialize()
        {
            // AppCommandsの初期化
            AppCommands.Initialize(_windowHandleService, _pickerService);
            
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

            string snippetDefault = Path.Combine(roamingPath, "AimAssist", "SnippetsStandard.md");
            string snippetSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Snippets", "SnippetsStandard.md");
            if (!File.Exists(snippetDefault))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(snippetDefault));
                File.Copy(snippetSource, snippetDefault);
            }

            SnippetOptionServce.LoadOption();

            EditorOptionService.LoadOption();

            SystemTrayRegister.Register();

            // DIで注入されたユニットサービスを使用
            _unitsService.RegisterUnits(new UnitsFactory());

            var pluginService = new PluginsService();
            pluginService.LoadCommandPlugins();
            var factories = pluginService.GetFactories();
            foreach (var item in factories)
            {
                _unitsService.RegisterUnits(item);
            }

            var converters = pluginService.GetConterters();
            foreach (var item in converters)
            {
                UnitViewFactory.UnitToUIElementDicotionary.TryAdd(item.Key, item.Value);
            }

            _commandService.Register(AppCommands.ToggleMainWindow, new KeySequence(System.Windows.Input.Key.A, System.Windows.Input.ModifierKeys.Alt));
            _commandService.Register(AppCommands.ShowPickerWindow, new KeySequence(System.Windows.Input.Key.P, System.Windows.Input.ModifierKeys.Alt));
            _commandService.Register(AppCommands.ShutdownAimAssist, new KeySequence(System.Windows.Input.Key.D, System.Windows.Input.ModifierKeys.Control));

            // DIコンテナからWaitHotKeysWindowを取得
            var waitHotKeysWindow = _serviceProvider.GetRequiredService<UI.Tools.HotKeys.WaitHotKeysWindow>();
            waitHotKeysWindow.Show();

            _applicationLogService.Initialize();
        }
    }
}
