using AimAssist.Services.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI.Modules
{
    public class InitializationServiceModule : IServiceModule
    {
        public IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IFileInitializationService, FileInitializationService>();
            services.AddSingleton<IPluginInitializationService, PluginInitializationService>();
            services.AddSingleton<IApplicationInitializationService, ApplicationInitializationService>();
            
            return services;
        }
    }
}
