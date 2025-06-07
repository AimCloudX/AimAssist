using AimAssist.Core.Interfaces;
using AimAssist.UI.HotKeys;
using AimAssist.UI.SystemTray;
using AimAssist.Units.Implementation;
using Common.UI.Commands.Shortcus;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.Services.Initialization
{
    public class ApplicationInitializationService : IApplicationInitializationService
    {
        private readonly IAppCommands appCommands;
        private readonly ICommandService commandService;
        private readonly IUnitsService unitsService;
        private readonly ISnippetOptionService snippetOptionService;
        private readonly IWorkItemOptionService workItemOptionService;
        private readonly IEditorOptionService editorOptionService;
        private readonly IServiceProvider serviceProvider;
        private readonly IFileInitializationService fileInitializationService;
        private readonly IPluginInitializationService pluginInitializationService;
        private readonly IFactoryInitializationService factoryInitializationService;

        public ApplicationInitializationService(
            IAppCommands appCommands,
            ICommandService commandService,
            IUnitsService unitsService,
            ISnippetOptionService snippetOptionService,
            IWorkItemOptionService workItemOptionService,
            IEditorOptionService editorOptionService,
            IServiceProvider serviceProvider,
            IFileInitializationService fileInitializationService,
            IPluginInitializationService pluginInitializationService,
            IFactoryInitializationService factoryInitializationService)
        {
            this.appCommands = appCommands;
            this.commandService = commandService;
            this.unitsService = unitsService;
            this.snippetOptionService = snippetOptionService;
            this.workItemOptionService = workItemOptionService;
            this.editorOptionService = editorOptionService;
            this.serviceProvider = serviceProvider;
            this.fileInitializationService = fileInitializationService;
            this.pluginInitializationService = pluginInitializationService;
            this.factoryInitializationService = factoryInitializationService;
        }

        public void Initialize()
        {
            InitializeCommands();
            fileInitializationService.InitializeFiles();
            InitializeSystemTray();
            InitializeFactoriesAndUnits();
            pluginInitializationService.InitializePlugins();
            RegisterHotKeys();
            ShowMainWindow();
        }

        private void InitializeCommands()
        {
            appCommands.Initialize();
        }

        private void InitializeSystemTray()
        {
            var systemTrayRegister = serviceProvider.GetRequiredService<SystemTrayRegister>();
            systemTrayRegister.Register();
        }

        private void InitializeFactoriesAndUnits()
        {
            // Phase 3: 属性ベース自動登録システムのテスト
            factoryInitializationService.InitializeFactories();
            
            // テスト用：属性ベース自動登録のみで動作確認
            System.Diagnostics.Debug.WriteLine("=== 属性ベース自動登録システム テスト開始 ===");
            
            // デバッグ用：Unit数の確認
            var allUnits = unitsService.GetAllUnits().ToList();
            System.Diagnostics.Debug.WriteLine($"属性ベース自動登録Unit数: {allUnits.Count}");
            
            foreach (var unit in allUnits.Take(10)) // 最初の10個を表示
            {
                System.Diagnostics.Debug.WriteLine($"  - {unit.GetType().Name}: {unit.Name} (Category: {unit.Category})");
            }
            
            foreach (var mode in unitsService.GetAllModes())
            {
                var modeUnits = unitsService.CreateUnits(mode).ToList();
                System.Diagnostics.Debug.WriteLine($"Mode '{mode.GetType().Name}': {modeUnits.Count} units");
            }
            
            System.Diagnostics.Debug.WriteLine("=== 属性ベース自動登録システム テスト完了 ===");
        }

        private void RegisterHotKeys()
        {
            commandService.Register(appCommands.ToggleMainWindow, new KeySequence(System.Windows.Input.Key.A, System.Windows.Input.ModifierKeys.Alt));
            commandService.Register(appCommands.ShowPickerWindow, new KeySequence(System.Windows.Input.Key.P, System.Windows.Input.ModifierKeys.Alt));
            commandService.Register(appCommands.ShutdownAimAssist, new KeySequence(System.Windows.Input.Key.D, System.Windows.Input.ModifierKeys.Control));
        }

        private void ShowMainWindow()
        {
            var waitHotKeysWindow = serviceProvider.GetRequiredService<WaitHotKeysWindow>();
            waitHotKeysWindow.Show();
        }
    }
}
