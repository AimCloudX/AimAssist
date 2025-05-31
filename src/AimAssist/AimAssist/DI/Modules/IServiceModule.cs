using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI.Modules
{
    public interface IServiceModule
    {
        IServiceCollection RegisterServices(IServiceCollection services);
    }
}
