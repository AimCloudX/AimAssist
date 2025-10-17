using System.Threading.Tasks;

namespace AimAssist.Services.Initialization
{
    public interface IApplicationInitializationService
    {
        void Initialize();
        Task InitializeAsync();
    }
}
