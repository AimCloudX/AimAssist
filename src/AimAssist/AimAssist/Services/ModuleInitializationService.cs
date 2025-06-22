using AimAssist.Services.Initialization;

namespace AimAssist.Services
{
    public interface IModuleInitializationService
    {
        Task InitializeAllModulesAsync();
        Task InitializeModuleAsync(string moduleName);
        Task ShutdownAllModulesAsync();
        Task ShutdownModuleAsync(string moduleName);
        bool IsModuleInitialized(string moduleName);
    }

    public class ModuleInitializationService : IModuleInitializationService
    {
        private readonly IApplicationInitializationService applicationInitializer;
        private readonly IFileInitializationService fileInitializer;
        private readonly IPluginInitializationService pluginInitializer;
        private readonly IConfigurationManagerService configurationManager;
        private readonly Core.Interfaces.IApplicationLogService logService;

        public ModuleInitializationService(
            IApplicationInitializationService applicationInitializer,
            IFileInitializationService fileInitializer,
            IPluginInitializationService pluginInitializer,
            IConfigurationManagerService configurationManager,
            Core.Interfaces.IApplicationLogService logService)
        {
            this.applicationInitializer = applicationInitializer;
            this.fileInitializer = fileInitializer;
            this.pluginInitializer = pluginInitializer;
            this.configurationManager = configurationManager;
            this.logService = logService;
        }

        public Task InitializeAllModulesAsync()
        {
            try
            {
                logService.Info("全モジュールの初期化を開始します");

                // await _applicationInitializer.InitializeAsync();
                // _logService.Info("アプリケーション初期化が完了しました");
                //
                // await _fileInitializer.InitializeAsync();
                // _logService.Info("ファイル初期化が完了しました");
                //
                // await _pluginInitializer.InitializeAsync();
                // _logService.Info("プラグイン初期化が完了しました");

                configurationManager.LoadAllSections();
                logService.Info("設定読み込みが完了しました");

                logService.Info("全モジュールの初期化が正常に完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "モジュール初期化中にエラーが発生しました");
                throw;
            }
            
            return Task.CompletedTask;
        }

        public Task InitializeModuleAsync(string moduleName)
        {
            try
            {
                logService.Info($"モジュール '{moduleName}' の初期化を開始します");

                switch (moduleName.ToLower())
                {
                    // case "application":
                    //     await _applicationInitializer.InitializeAsync();
                    //     break;
                    // case "file":
                    //     await _fileInitializer.InitializeAsync();
                    //     break;
                    // case "plugin":
                    //     await _pluginInitializer.InitializeAsync();
                    //     break;
                    case "configuration":
                        configurationManager.LoadAllSections();
                        break;
                    default:
                        logService.Warning($"不明なモジュール名: {moduleName}");
                        return Task.CompletedTask;
                }

                logService.Info($"モジュール '{moduleName}' の初期化が完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, $"モジュール '{moduleName}' の初期化中にエラーが発生しました");
                throw;
            }
            
            return Task.CompletedTask;
        }

        public Task ShutdownAllModulesAsync()
        {
            try
            {
                logService.Info("全モジュールのシャットダウンを開始します");

                configurationManager.SaveAllSections();
                logService.Info("設定保存が完了しました");

                // await _pluginInitializer.ShutdownAsync();
                // _logService.Info("プラグインシャットダウンが完了しました");
                //
                // await _fileInitializer.ShutdownAsync();
                // _logService.Info("ファイルシャットダウンが完了しました");
                //
                // await _applicationInitializer.ShutdownAsync();
                // _logService.Info("アプリケーションシャットダウンが完了しました");

                logService.Info("全モジュールのシャットダウンが正常に完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "モジュールシャットダウン中にエラーが発生しました");
                throw;
            }
            
            return Task.CompletedTask;
        }

        public Task ShutdownModuleAsync(string moduleName)
        {
            try
            {
                logService.Info($"モジュール '{moduleName}' のシャットダウンを開始します");

                switch (moduleName.ToLower())
                {
                    // case "application":
                    //     await _applicationInitializer.ShutdownAsync();
                    //     break;
                    // case "file":
                    //     await _fileInitializer.ShutdownAsync();
                    //     break;
                    // case "plugin":
                    //     await _pluginInitializer.ShutdownAsync();
                    //     break;
                    case "configuration":
                        configurationManager.SaveAllSections();
                        break;
                    default:
                        logService.Warning($"不明なモジュール名: {moduleName}");
                        return Task.CompletedTask;
                }

                logService.Info($"モジュール '{moduleName}' のシャットダウンが完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, $"モジュール '{moduleName}' のシャットダウン中にエラーが発生しました");
                throw;
            }
            
            return Task.CompletedTask;
        }

        public bool IsModuleInitialized(string moduleName)
        {
            return moduleName.ToLower() switch
            {
                "application" => true,
                "file" => true,
                "plugin" => true,
                "configuration" => true,
                _ => false
            };
        }
    }
}
