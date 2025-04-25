using AimAssist.Core.Interfaces;
using AimAssist.Service;
using AimAssist.UI.SystemTray;
using AimAssist.UI.UnitContentsView;
using AimAssist.Units.Implementation;
using Common.Commands.Shortcus;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly WindowHandleService _windowHandleService;
        private readonly PickerService _pickerService;
        private readonly IAppCommands _appCommands;
        private readonly IEditorOptionService _editorOptionService;
        private readonly ISnippetOptionService _snippetOptionService;
        private readonly IWorkItemOptionService _workItemOptionService;
        private readonly IPluginsService _pluginsService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="unitsService">ユニットサービス</param>
        /// <param name="commandService">コマンドサービス</param>
        /// <param name="serviceProvider">サービスプロバイダー</param>
        /// <param name="windowHandleService">ウィンドウハンドルサービス</param>
        /// <param name="pickerService">ピッカーサービス</param>
        /// <param name="appCommands">アプリケーションコマンド</param>
        /// <param name="editorOptionService">エディターオプションサービス</param>
        /// <param name="snippetOptionService">スニペットオプションサービス</param>
        /// <param name="workItemOptionService">作業項目オプションサービス</param>
        /// <param name="pluginsService">プラグインサービス</param>
        public Initializer(
            IUnitsService unitsService,
            ICommandService commandService,
            IServiceProvider serviceProvider,
            WindowHandleService windowHandleService,
            PickerService pickerService,
            IAppCommands appCommands,
            IEditorOptionService editorOptionService,
            ISnippetOptionService snippetOptionService,
            IWorkItemOptionService workItemOptionService,
            IPluginsService pluginsService)
        {
            _unitsService = unitsService;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _windowHandleService = windowHandleService;
            _pickerService = pickerService;
            _appCommands = appCommands;
            _editorOptionService = editorOptionService;
            _snippetOptionService = snippetOptionService;
            _workItemOptionService = workItemOptionService;
            _pluginsService = pluginsService;
        }

        /// <summary>
        /// アプリケーションを初期化します
        /// </summary>
        public void Initialize()
        {
            // AppCommandsの初期化
            _appCommands.Initialize();
            
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string editorOptionPath = Path.Combine(roamingPath, "AimAssist", "editor.option.json");
            string editorOptionSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Settings", "editor.option.json");
            if (!File.Exists(editorOptionPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(editorOptionPath));
                File.Copy(editorOptionSource, editorOptionPath);
            }

            string workItemOptionSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "WorkItems", "workitem.option.json");
            if (!File.Exists(_workItemOptionService.OptionPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_workItemOptionService.OptionPath));
                File.Copy(workItemOptionSource, _workItemOptionService.OptionPath);
            }

            string targetPath = Path.Combine(roamingPath, "AimAssist", "WorkItem.md");
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "WorkItems", "WorkItem.md");
            if (!File.Exists(targetPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                File.Copy(sourcePath, targetPath);
            }

            // DI経由で取得したWorkItemOptionServiceを使用
            try
            {
                _workItemOptionService.LoadOption();
            }
            catch (Exception ex)
            {
            }

            string snippetoption = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Snippets", "snippet.option.json");
            if (!File.Exists(_snippetOptionService.OptionPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_snippetOptionService.OptionPath));
                File.Copy(snippetoption, _snippetOptionService.OptionPath);
            }

            string snippetDefault = Path.Combine(roamingPath, "AimAssist", "SnippetsStandard.md");
            string snippetSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Snippets", "SnippetsStandard.md");
            if (!File.Exists(snippetDefault))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(snippetDefault));
                File.Copy(snippetSource, snippetDefault);
            }

            // DI経由で取得したSnippetOptionServiceを使用
            try
            {
                _snippetOptionService.LoadOption();
            }
            catch (Exception ex)
            {
            }

            // DIから取得したEditorOptionServiceを使用
            try
            {
                _editorOptionService.LoadOption();
            }
            catch (Exception ex)
            {
            }

            // DIでSystemTrayRegisterを取得して使用
            var systemTrayRegister = _serviceProvider.GetRequiredService<SystemTrayRegister>();
            systemTrayRegister.Register();

            // DIで注入されたユニットサービスを使用
            _unitsService.RegisterUnits(new UnitsFactory(
                _editorOptionService, 
                _workItemOptionService, 
                _snippetOptionService));

            // DIから取得したIPluginsServiceを使用
            try
            {
                _pluginsService.LoadCommandPlugins();
                var factories = _pluginsService.GetFactories();
                foreach (var item in factories)
                {
                    _unitsService.RegisterUnits(item);
                }

                var converters = _pluginsService.GetConverters();
                foreach (var item in converters)
                {
                    UnitViewFactory.UnitToUIElementDictionary.TryAdd(item.Key, item.Value);
                }
            }
            catch (Exception ex)
            {
            }

            _commandService.Register(_appCommands.ToggleMainWindow, new KeySequence(System.Windows.Input.Key.A, System.Windows.Input.ModifierKeys.Alt));
            _commandService.Register(_appCommands.ShowPickerWindow, new KeySequence(System.Windows.Input.Key.P, System.Windows.Input.ModifierKeys.Alt));
            _commandService.Register(_appCommands.ShutdownAimAssist, new KeySequence(System.Windows.Input.Key.D, System.Windows.Input.ModifierKeys.Control));

            // DIコンテナからWaitHotKeysWindowを取得
            var waitHotKeysWindow = _serviceProvider.GetRequiredService<UI.Tools.HotKeys.WaitHotKeysWindow>();
            waitHotKeysWindow.Show();
        }
    }
}
