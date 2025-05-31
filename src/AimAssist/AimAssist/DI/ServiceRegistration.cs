using AimAssist.DI.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI
{
    public static class ServiceRegistration
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            var modules = new IServiceModule[]
            {
                new CoreServiceModule(),
                new ApplicationServiceModule(),
                new OptionServiceModule(),
                new FactoryServiceModule(),
                new PluginServiceModule(),
                new UiServiceModule(),
                new InitializationServiceModule()
            };

            foreach (var module in modules)
            {
                module.RegisterServices(services);
            }

            return services;
        }
    }
}
