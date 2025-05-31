using AimAssist.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
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
        private readonly IApplicationLogService _logService;
        private readonly ISettingManager _settingManager;
        private readonly ICommandService _commandService;
        private readonly IServiceProvider _serviceProvider;

        public ApplicationService(
            IApplicationLogService logService,
            ISettingManager settingManager,
            ICommandService commandService,
            IServiceProvider serviceProvider)
        {
            _logService = logService;
            _settingManager = settingManager;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logService.Info("アプリケーションの初期化を開始します");
                
                var initializer = _serviceProvider.GetRequiredService<Initializer>();
                initializer.Initialize();
                
                _logService.Info("設定情報を読み込みます");
                var settings = _settingManager.LoadSettings();
                _commandService.SetKeymap(settings);
                
                _logService.Info("アプリケーションの初期化が正常に完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "アプリケーション初期化中に重大なエラーが発生しました");
                throw;
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                _logService.Info("アプリケーションのシャットダウンを開始します");
                
                var settings = _commandService.GetKeymap();
                var noneSettingsKeys = settings.Where(x => x.Value.FirstModifiers == 0).Select(y => y.Key);
                foreach (var key in noneSettingsKeys)
                {
                    settings.Remove(key);
                }

                _settingManager.SaveSettings(settings);
                _serviceProvider.GetRequiredService<IEditorOptionService>().SaveOption();
                _serviceProvider.GetRequiredService<ISnippetOptionService>().SaveOption();
                _serviceProvider.GetRequiredService<IWorkItemOptionService>().SaveOption();
                
                _logService.Info("アプリケーションのシャットダウンが完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "アプリケーションシャットダウン中にエラーが発生しました");
            }
        }
    }
}
