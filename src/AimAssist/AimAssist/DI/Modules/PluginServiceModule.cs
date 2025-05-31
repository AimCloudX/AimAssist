using AimAssist.Core.Interfaces;
using AimAssist.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI.Modules
{
    public class PluginServiceModule : IServiceModule
    {
        public IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IPluginsService>(provider => new PluginsService(
                provider.GetRequiredService<IEditorOptionService>()
            ));
            
            return services;
        }
    }
}
