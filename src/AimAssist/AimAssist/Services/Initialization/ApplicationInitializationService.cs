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
        private readonly IAppCommands _appCommands;
        private readonly ICommandService _commandService;
        private readonly IUnitsService _unitsService;
        private readonly ISnippetOptionService _snippetOptionService;
        private readonly IWorkItemOptionService _workItemOptionService;
        private readonly IEditorOptionService _editorOptionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileInitializationService _fileInitializationService;
        private readonly IPluginInitializationService _pluginInitializationService;

        public ApplicationInitializationService(
            IAppCommands appCommands,
            ICommandService commandService,
            IUnitsService unitsService,
            ISnippetOptionService snippetOptionService,
            IWorkItemOptionService workItemOptionService,
            IEditorOptionService editorOptionService,
            IServiceProvider serviceProvider,
            IFileInitializationService fileInitializationService,
            IPluginInitializationService pluginInitializationService)
        {
            _appCommands = appCommands;
            _commandService = commandService;
            _unitsService = unitsService;
            _snippetOptionService = snippetOptionService;
            _workItemOptionService = workItemOptionService;
            _editorOptionService = editorOptionService;
            _serviceProvider = serviceProvider;
            _fileInitializationService = fileInitializationService;
            _pluginInitializationService = pluginInitializationService;
        }

        public void Initialize()
        {
            InitializeCommands();
            _fileInitializationService.InitializeFiles();
            InitializeSystemTray();
            InitializeUnits();
            _pluginInitializationService.InitializePlugins();
            RegisterHotKeys();
            ShowMainWindow();
        }

        private void InitializeCommands()
        {
            _appCommands.Initialize();
        }

        private void InitializeSystemTray()
        {
            var systemTrayRegister = _serviceProvider.GetRequiredService<SystemTrayRegister>();
            systemTrayRegister.Register();
        }

        private void InitializeUnits()
        {
            _unitsService.RegisterUnits(new UnitsFactory(
                _editorOptionService,
                _workItemOptionService,
                _snippetOptionService));
        }

        private void RegisterHotKeys()
        {
            _commandService.Register(_appCommands.ToggleMainWindow, new KeySequence(System.Windows.Input.Key.A, System.Windows.Input.ModifierKeys.Alt));
            _commandService.Register(_appCommands.ShowPickerWindow, new KeySequence(System.Windows.Input.Key.P, System.Windows.Input.ModifierKeys.Alt));
            _commandService.Register(_appCommands.ShutdownAimAssist, new KeySequence(System.Windows.Input.Key.D, System.Windows.Input.ModifierKeys.Control));
        }

        private void ShowMainWindow()
        {
            var waitHotKeysWindow = _serviceProvider.GetRequiredService<WaitHotKeysWindow>();
            waitHotKeysWindow.Show();
        }
    }
}
