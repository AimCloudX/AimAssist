using AimAssist.Core.Interfaces;
using Common.UI.Commands.Shortcus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AimAssist.Services
{
    public interface IApplicationService
    {
        Task InitializeAsync();
        Task ShutdownAsync();
    }

    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationLifecycleService _lifecycleService;
        private readonly IModuleInitializationService _moduleInitializationService;
        private readonly IConfigurationManagerService _configurationManager;
        private readonly ICommandService _commandService;
        private readonly IApplicationLogService _logService;

        public ApplicationService(
            IApplicationLifecycleService lifecycleService,
            IModuleInitializationService moduleInitializationService,
            IConfigurationManagerService configurationManager,
            ICommandService commandService,
            IApplicationLogService logService)
        {
            _lifecycleService = lifecycleService;
            _moduleInitializationService = moduleInitializationService;
            _configurationManager = configurationManager;
            _commandService = commandService;
            _logService = logService;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logService.Info("アプリケーションサービスの初期化を開始します");

                await _lifecycleService.StartupAsync();
                await _moduleInitializationService.InitializeAllModulesAsync();

                var settings = _configurationManager.GetConfiguration<Dictionary<string, KeySequence>>("Keymap", "AllSettings", new Dictionary<string, KeySequence>());
                if (settings != null && settings.Count > 0)
                {
                    var nullableSettings = settings.ToDictionary(kvp => kvp.Key, kvp => (KeySequence?)kvp.Value);
                    _commandService.SetKeymap(nullableSettings);
                }

                _logService.Info("アプリケーションサービスの初期化が正常に完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "アプリケーションサービス初期化中に重大なエラーが発生しました");
                throw;
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                _logService.Info("アプリケーションサービスのシャットダウンを開始します");

                await _moduleInitializationService.ShutdownAllModulesAsync();
                await _lifecycleService.ShutdownAsync();

                _logService.Info("アプリケーションサービスのシャットダウンが完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "アプリケーションサービスシャットダウン中にエラーが発生しました");
            }
        }
    }
}
