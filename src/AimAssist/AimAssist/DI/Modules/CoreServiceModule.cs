using AimAssist.Core.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Services;
using AimAssist.Middlewares;
using AimAssist.Service;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI.Modules
{
    public class CoreServiceModule : IServiceModule
    {
        public IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IUnitsService, UnitsService>();
            services.AddSingleton<ICommandService, CommandService>();
            services.AddSingleton<ISettingManager, SettingManager>();
            services.AddSingleton<IKeySequenceManager, KeySequenceManager>();
            services.AddSingleton<IApplicationLogService, ApplicationLogService>();
            services.AddSingleton<IWindowHandleService, WindowHandleService>();
            services.AddSingleton<IErrorHandlingMiddleware, ErrorHandlingMiddleware>();
            
            return services;
        }
    }
}
