using AimAssist.Core.Interfaces;
using AimAssist.Units.Implementation;
using System;
using AimAssist.Core.Units;
using AimAssist.Units.UnitFactories;

namespace AimAssist.Services.Initialization
{
    public interface IFactoryInitializationService
    {
        void InitializeFactories();
        void RegisterAutoDiscoveryFactory();
        void RegisterReflectionBasedFactory();
    }

    public class FactoryInitializationService : IFactoryInitializationService
    {
        private readonly IUnitsFactoryManager factoryManager;
        private readonly IUnitsService unitsService;
        private readonly ICompositeUnitsFactory compositeFactory;
        private readonly AutoDiscoveryUnitsFactory autoDiscoveryFactory;
        private readonly ReflectionBasedUnitsFactory reflectionBasedFactory;
        private readonly ISupportUnitsFactory snippetSupportUnitsFactory;
        private readonly IWorkToolsUnitsFactory workToolsUnitsFactory;
        private readonly IApplicationLogService logService;

        public FactoryInitializationService(
            IUnitsFactoryManager factoryManager,
            IUnitsService unitsService,
            ICompositeUnitsFactory compositeFactory,
            AutoDiscoveryUnitsFactory autoDiscoveryFactory,
            ReflectionBasedUnitsFactory reflectionBasedFactory,
            SnippetSupportUnitsFactory snippetSupportUnitsFactory,
            IWorkToolsUnitsFactory workToolsUnitsFactory,
            IApplicationLogService logService)
        {
            this.factoryManager = factoryManager;
            this.unitsService = unitsService;
            this.compositeFactory = compositeFactory;
            this.autoDiscoveryFactory = autoDiscoveryFactory;
            this.reflectionBasedFactory = reflectionBasedFactory;
            this.snippetSupportUnitsFactory = snippetSupportUnitsFactory;
            this.workToolsUnitsFactory = workToolsUnitsFactory;
            this.logService = logService;
        }

        public void InitializeFactories()
        {
            try
            {
                logService.Info("Factoryシステムの初期化を開始します");

                // ReflectionBasedUnitsFactory（属性ベース自動登録）を最優先で登録
                factoryManager.RegisterFactory(reflectionBasedFactory);
                logService.Info("ReflectionBasedUnitsFactoryを登録しました");

                // AutoDiscoveryUnitsFactory（従来の個別Factory統合）をバックアップとして登録
                factoryManager.RegisterFactory(autoDiscoveryFactory);
                logService.Info("AutoDiscoveryUnitsFactoryを登録しました");

                // CompositeUnitsFactoryをUnitsServiceに登録
                unitsService.RegisterUnits(compositeFactory);
                
                unitsService.RegisterFeatures(snippetSupportUnitsFactory);
                unitsService.RegisterFeatures(workToolsUnitsFactory);
                logService.Info("CompositeUnitsFactoryをUnitsServiceに登録しました");

                logService.Info("Factoryシステムの初期化が完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "Factoryシステム初期化中にエラーが発生しました");
                throw;
            }
        }

        public void RegisterReflectionBasedFactory()
        {
            try
            {
                logService.Info("ReflectionBasedUnitsFactoryの登録を開始します");
                factoryManager.RegisterFactory(reflectionBasedFactory);
                logService.Info("ReflectionBasedUnitsFactoryの登録が完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "ReflectionBasedUnitsFactory登録中にエラーが発生しました");
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
