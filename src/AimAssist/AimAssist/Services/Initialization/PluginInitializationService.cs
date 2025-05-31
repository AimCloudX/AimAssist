using AimAssist.Core.Interfaces;
using AimAssist.UI.UnitContentsView;
using AimAssist.Units.Implementation;

namespace AimAssist.Services.Initialization
{
    public interface IPluginInitializationService
    {
        void InitializePlugins();
    }
    
    public class PluginInitializationService : IPluginInitializationService
    {
        private readonly IPluginsService _pluginsService;
        private readonly IUnitsService _unitsService;
        private readonly IApplicationLogService _logService;

        public PluginInitializationService(
            IPluginsService pluginsService,
            IUnitsService unitsService,
            IApplicationLogService logService)
        {
            _pluginsService = pluginsService;
            _unitsService = unitsService;
            _logService = logService;
        }

        public void InitializePlugins()
        {
            try
            {
                _logService.Info("プラグインの読み込みを開始します");
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
                
                _logService.Info("プラグインの読み込みが完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "プラグインの読み込み中にエラーが発生しました");
            }
        }
    }
}
