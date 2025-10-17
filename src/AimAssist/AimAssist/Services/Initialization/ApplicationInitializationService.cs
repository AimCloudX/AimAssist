using AimAssist.Core.Interfaces;
using AimAssist.UI.HotKeys;
using AimAssist.UI.SystemTray;
using Common.UI.Commands.Shortcus;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.Services.Initialization
{
    public class ApplicationInitializationService : IApplicationInitializationService
    {
        private readonly IAppCommands appCommands;
        private readonly ICommandService commandService;
        private readonly IUnitsService unitsService;
        private readonly IWorkItemOptionService workItemOptionService;
        private readonly IEditorOptionService editorOptionService;
        private readonly IServiceProvider serviceProvider;
        private readonly IFileInitializationService fileInitializationService;
        private readonly IPluginInitializationService pluginInitializationService;
        private readonly IFactoryInitializationService factoryInitializationService;
        private readonly IApplicationLogService logService;

        public ApplicationInitializationService(
            IAppCommands appCommands,
            ICommandService commandService,
            IUnitsService unitsService,
            IWorkItemOptionService workItemOptionService,
            IEditorOptionService editorOptionService,
            IServiceProvider serviceProvider,
            IFileInitializationService fileInitializationService,
            IPluginInitializationService pluginInitializationService,
            IFactoryInitializationService factoryInitializationService,
            IApplicationLogService logService)
        {
            this.appCommands = appCommands;
            this.commandService = commandService;
            this.unitsService = unitsService;
            this.workItemOptionService = workItemOptionService;
            this.editorOptionService = editorOptionService;
            this.serviceProvider = serviceProvider;
            this.fileInitializationService = fileInitializationService;
            this.pluginInitializationService = pluginInitializationService;
            this.factoryInitializationService = factoryInitializationService;
            this.logService = logService;
        }

        public void Initialize()
        {
            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            try
            {
                logService.Info("アプリケーション初期化を開始します");

                // Phase 1: 同期的な基本初期化
                InitializeCommands();
                fileInitializationService.InitializeFiles();
                InitializeSystemTray();
                RegisterHotKeys();

                // Phase 2: 非同期でFactoryとUnitsを初期化
                await Task.Run(() =>
                {
                    try
                    {
                        InitializeFactoriesAndUnits();
                    }
                    catch (Exception ex)
                    {
                        logService.LogException(ex, "Factory初期化中にエラーが発生しました");
                        throw;
                    }
                });

                // Phase 3: 非同期でPluginを初期化
                await Task.Run(() =>
                {
                    try
                    {
                        pluginInitializationService.InitializePlugins();
                    }
                    catch (Exception ex)
                    {
                        logService.LogException(ex, "Plugin初期化中にエラーが発生しました");
                        // Pluginエラーは致命的ではないため、継続
                    }
                });

                // Phase 4: UIスレッドでMainWindow表示
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        ShowMainWindow();
                        logService.Info("アプリケーション初期化が完了しました");
                    }
                    catch (Exception ex)
                    {
                        logService.LogException(ex, "MainWindow表示中にエラーが発生しました");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "アプリケーション初期化中に致命的なエラーが発生しました");
                throw;
            }
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
