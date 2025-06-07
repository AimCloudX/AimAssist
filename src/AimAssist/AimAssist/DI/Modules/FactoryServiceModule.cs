using AimAssist.Plugins;
using AimAssist.Units.Implementation;
using AimAssist.Units.Implementation.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI.Modules
{
    public class FactoryServiceModule : IServiceModule
    {
        public IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IUnitsFactoryManager, UnitsFactoryManager>();
            services.AddSingleton<IPluginUnitsFactory, PluginUnitsFactory>();
            services.AddSingleton<ICompositeUnitsFactory, CompositeUnitsFactory>();
            
            services.AddSingleton<IWorkToolsUnitsFactory, WorkToolsUnitsFactory>();
            services.AddSingleton<ISnippetUnitsFactory, SnippetUnitsFactory>();
            services.AddSingleton<IKnowledgeUnitsFactory, KnowledgeUnitsFactory>();
            services.AddSingleton<ICheatSheetUnitsFactory, CheatSheetUnitsFactory>();
            services.AddSingleton<IOptionUnitsFactory, OptionUnitsFactory>();
            // CoreUnitsFactory削除 - 属性ベース自動登録に移行
            services.AddSingleton<AutoDiscoveryUnitsFactory>();
            services.AddSingleton<ReflectionBasedUnitsFactory>();
            
            return services;
        }
    }
}
