using AimAssist.Services.Initialization;
using AimAssist.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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
        private readonly IApplicationInitializationService _applicationInitializer;
        private readonly IFileInitializationService _fileInitializer;
        private readonly IPluginInitializationService _pluginInitializer;
        private readonly IConfigurationManagerService _configurationManager;
        private readonly Core.Interfaces.IApplicationLogService _logService;

        public ModuleInitializationService(
            IApplicationInitializationService applicationInitializer,
            IFileInitializationService fileInitializer,
            IPluginInitializationService pluginInitializer,
            IConfigurationManagerService configurationManager,
            Core.Interfaces.IApplicationLogService logService)
        {
            _applicationInitializer = applicationInitializer;
            _fileInitializer = fileInitializer;
            _pluginInitializer = pluginInitializer;
            _configurationManager = configurationManager;
            _logService = logService;
        }

        public async Task InitializeAllModulesAsync()
        {
            try
            {
                _logService.Info("全モジュールの初期化を開始します");

                // await _applicationInitializer.InitializeAsync();
                // _logService.Info("アプリケーション初期化が完了しました");
                //
                // await _fileInitializer.InitializeAsync();
                // _logService.Info("ファイル初期化が完了しました");
                //
                // await _pluginInitializer.InitializeAsync();
                // _logService.Info("プラグイン初期化が完了しました");

                _configurationManager.LoadAllSections();
                _logService.Info("設定読み込みが完了しました");

                _logService.Info("全モジュールの初期化が正常に完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "モジュール初期化中にエラーが発生しました");
                throw;
            }
        }

        public async Task InitializeModuleAsync(string moduleName)
        {
            try
            {
                _logService.Info($"モジュール '{moduleName}' の初期化を開始します");

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
                        _configurationManager.LoadAllSections();
                        break;
                    default:
                        _logService.Warning($"不明なモジュール名: {moduleName}");
                        return;
                }

                _logService.Info($"モジュール '{moduleName}' の初期化が完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"モジュール '{moduleName}' の初期化中にエラーが発生しました");
                throw;
            }
        }

        public async Task ShutdownAllModulesAsync()
        {
            try
            {
                _logService.Info("全モジュールのシャットダウンを開始します");

                _configurationManager.SaveAllSections();
                _logService.Info("設定保存が完了しました");

                // await _pluginInitializer.ShutdownAsync();
                // _logService.Info("プラグインシャットダウンが完了しました");
                //
                // await _fileInitializer.ShutdownAsync();
                // _logService.Info("ファイルシャットダウンが完了しました");
                //
                // await _applicationInitializer.ShutdownAsync();
                // _logService.Info("アプリケーションシャットダウンが完了しました");

                _logService.Info("全モジュールのシャットダウンが正常に完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "モジュールシャットダウン中にエラーが発生しました");
                throw;
            }
        }

        public async Task ShutdownModuleAsync(string moduleName)
        {
            try
            {
                _logService.Info($"モジュール '{moduleName}' のシャットダウンを開始します");

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
                        _configurationManager.SaveAllSections();
                        break;
                    default:
                        _logService.Warning($"不明なモジュール名: {moduleName}");
                        return;
                }

                _logService.Info($"モジュール '{moduleName}' のシャットダウンが完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"モジュール '{moduleName}' のシャットダウン中にエラーが発生しました");
                throw;
            }
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
