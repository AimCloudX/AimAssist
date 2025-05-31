using AimAssist.Services;
using AimAssist.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI.Modules
{
    public class ApplicationServiceModule : IServiceModule
    {
        public IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<IApplicationLifecycleService, ApplicationLifecycleService>();
            services.AddSingleton<IModuleInitializationService, ModuleInitializationService>();
            services.AddSingleton<IConfigurationManagerService, ConfigurationManagerService>();
            services.AddSingleton<IUnitManagementService, UnitManagementService>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<INavigationService, NavigationService>();
            
            return services;
        }
    }
}
