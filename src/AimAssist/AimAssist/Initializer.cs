using AimAssist.Services.Initialization;

namespace AimAssist
{
    internal class Initializer
    {
        private readonly IApplicationInitializationService _initializationService;

        public Initializer(IApplicationInitializationService initializationService)
        {
            _initializationService = initializationService;
        }

        public void Initialize()
        {
            _initializationService.Initialize();
        }
    }
}
