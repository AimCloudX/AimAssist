using AimAssist.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Service;
using AimAssist.UI.HotKeys;
using AimAssist.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI.Modules
{
    public class UIServiceModule : IServiceModule
    {
        public IServiceCollection RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IAppCommands>(provider => new AppCommands(
                provider.GetRequiredService<IWindowHandleService>(),
                provider.GetRequiredService<IPickerService>()
            ));

            services.AddSingleton<ICheatSheetController>(provider =>
                new CheatSheetController(
                    System.Windows.Threading.Dispatcher.CurrentDispatcher,
                    provider.GetRequiredService<IWindowHandleService>()
                ));

            services.AddSingleton<IPickerService>(provider => new PickerService(
                provider.GetRequiredService<ICommandService>(),
                provider.GetRequiredService<IUnitsService>(),
                provider.GetRequiredService<IEditorOptionService>(),
                provider.GetRequiredService<IApplicationLogService>()
            ));

            services.AddSingleton<AimAssist.UI.SystemTray.SystemTrayRegister>(provider =>
                new AimAssist.UI.SystemTray.SystemTrayRegister(provider.GetRequiredService<IAppCommands>())
            );

            services.AddSingleton<AimAssist.UI.UnitContentsView.UnitViewFactory>(provider => new AimAssist.UI.UnitContentsView.UnitViewFactory(
                provider.GetRequiredService<ICommandService>(),
                provider.GetRequiredService<IEditorOptionService>()
            ));

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<PickerWindowViewModel>();

            services.AddTransient<AimAssist.UI.MainWindows.MainWindow>(provider => new AimAssist.UI.MainWindows.MainWindow(
                provider.GetRequiredService<MainWindowViewModel>(),
                provider.GetRequiredService<IApplicationLogService>(),
                (KeySequenceManager)provider.GetRequiredService<IKeySequenceManager>(),
                provider.GetRequiredService<IUnitsService>(),
                provider.GetRequiredService<ICommandService>()
            ));

            services.AddTransient<WaitHotKeysWindow>(provider => new WaitHotKeysWindow(
                provider.GetRequiredService<ICommandService>(),
                provider.GetRequiredService<IAppCommands>()
            ));

            return services;
        }
    }
}
