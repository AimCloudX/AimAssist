using AimAssist.Core.Interfaces;
using AimAssist.Services.Options;
using AimAssist.Units.Implementation.WorkTools;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI.Modules
{
    public class OptionServiceModule : IServiceModule
    {
        public IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IEditorOptionService, EditorOptionService>();
            services.AddSingleton<IWorkItemOptionService, WorkItemOptionService>();
            
            return services;
        }
    }
}
