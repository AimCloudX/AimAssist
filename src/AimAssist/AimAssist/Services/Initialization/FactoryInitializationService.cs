using AimAssist.Core.Interfaces;
using AimAssist.Units.Implementation;
using AimAssist.Units.Implementation.Factories;
using System;

namespace AimAssist.Services.Initialization
{
    public interface IFactoryInitializationService
    {
        void InitializeFactories();
        void RegisterAutoDiscoveryFactory();
    }

    public class FactoryInitializationService : IFactoryInitializationService
    {
        private readonly IUnitsFactoryManager factoryManager;
        private readonly IUnitsService unitsService;
        private readonly ICompositeUnitsFactory compositeFactory;
        private readonly AutoDiscoveryUnitsFactory autoDiscoveryFactory;
        private readonly IApplicationLogService logService;

        public FactoryInitializationService(
            IUnitsFactoryManager factoryManager,
            IUnitsService unitsService,
            ICompositeUnitsFactory compositeFactory,
            AutoDiscoveryUnitsFactory autoDiscoveryFactory,
            IApplicationLogService logService)
        {
            this.factoryManager = factoryManager;
            this.unitsService = unitsService;
            this.compositeFactory = compositeFactory;
            this.autoDiscoveryFactory = autoDiscoveryFactory;
            this.logService = logService;
        }

        public void InitializeFactories()
        {
            try
            {
                logService.Info("Factoryシステムの初期化を開始します");

                // AutoDiscoveryUnitsFactoryをFactoryManagerに登録
                factoryManager.RegisterFactory(autoDiscoveryFactory);
                logService.Info("AutoDiscoveryUnitsFactoryを登録しました");

                // CompositeUnitsFactoryをUnitsServiceに登録
                unitsService.RegisterUnits(compositeFactory);
                logService.Info("CompositeUnitsFactoryをUnitsServiceに登録しました");

                logService.Info("Factoryシステムの初期化が完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "Factoryシステム初期化中にエラーが発生しました");
                throw;
            }
        }

        public void RegisterAutoDiscoveryFactory()
        {
            try
            {
                logService.Info("AutoDiscoveryFactoryの登録を開始します");
                factoryManager.RegisterFactory(autoDiscoveryFactory);
                logService.Info("AutoDiscoveryFactoryの登録が完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "AutoDiscoveryFactory登録中にエラーが発生しました");
                throw;
            }
        }
    }
}
