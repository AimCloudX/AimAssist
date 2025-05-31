using AimAssist.Core.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Services;
using AimAssist.Middlewares;
using AimAssist.Plugins;
using AimAssist.Service;
using AimAssist.Services;
using AimAssist.Units.Implementation;
using AimAssist.Units.Implementation.Factories;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.WorkTools;
using AimAssist.ViewModels;
using Library.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI
{
    public static class ServiceRegistration
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.RegisterCoreServices();
            services.RegisterApplicationServices();
            services.RegisterUIServices();
            services.RegisterOptionServices();
            services.RegisterPluginServices();
            services.RegisterFactoryServices();
            return services;
        }

        private static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<IUnitManagementService, UnitManagementService>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<INavigationService, NavigationService>();
            return services;
        }

        private static IServiceCollection RegisterCoreServices(this IServiceCollection services)
        {
            services.AddSingleton<IUnitsService, UnitsService>();
            services.AddSingleton<ICommandService, CommandService>();
            services.AddSingleton<ISettingManager, SettingManager>();
            services.AddSingleton<IKeySequenceManager, KeySequenceManager>();
            services.AddSingleton<IApplicationLogService, ApplicationLogService>();
            services.AddSingleton<IPickerService, PickerService>();
            services.AddSingleton<IWindowHandleService, WindowHandleService>();
            services.AddSingleton<IErrorHandlingMiddleware, ErrorHandlingMiddleware>();

            services.AddSingleton<IAppCommands>(provider => new AppCommands(
                provider.GetRequiredService<IWindowHandleService>(),
                provider.GetRequiredService<IPickerService>()
            ));

            services.AddSingleton<ICheatSheetController>(provider =>
                new CheatSheet.Services.CheatSheetController(
                    System.Windows.Threading.Dispatcher.CurrentDispatcher,
                    provider.GetRequiredService<IWindowHandleService>()
                ));

            return services;
        }

        private static IServiceCollection RegisterUIServices(this IServiceCollection services)
        {
            services.AddSingleton<AimAssist.UI.SystemTray.SystemTrayRegister>(provider =>
                new AimAssist.UI.SystemTray.SystemTrayRegister(provider.GetRequiredService<IAppCommands>())
            );

            services.AddSingleton<AimAssist.UI.UnitContentsView.UnitViewFactory>(provider => new AimAssist.UI.UnitContentsView.UnitViewFactory(
                provider.GetRequiredService<ICommandService>(),
                provider.GetRequiredService<IEditorOptionService>()
            ));

            services.AddTransient<MainWindowViewModel>();

            services.AddTransient<AimAssist.UI.MainWindows.MainWindow>(provider => new AimAssist.UI.MainWindows.MainWindow(
                provider.GetRequiredService<MainWindowViewModel>(),
                provider.GetRequiredService<IApplicationLogService>(),
                (KeySequenceManager)provider.GetRequiredService<IKeySequenceManager>(),
                provider.GetRequiredService<IUnitsService>(),
                provider.GetRequiredService<ICommandService>()
            ));

            services.AddTransient<AimAssist.UI.Tools.HotKeys.WaitHotKeysWindow>(provider => new AimAssist.UI.Tools.HotKeys.WaitHotKeysWindow(
                provider.GetRequiredService<ICommandService>(),
                provider.GetRequiredService<IAppCommands>()
            ));

            return services;
        }

        private static IServiceCollection RegisterOptionServices(this IServiceCollection services)
        {
            services.AddSingleton<IEditorOptionService, EditorOptionService>();
            services.AddSingleton<ISnippetOptionService, SnippetOptionService>();
            services.AddSingleton<IWorkItemOptionService, WorkItemOptionService>();
            return services;
        }

        private static IServiceCollection RegisterPluginServices(this IServiceCollection services)
        {
            services.AddSingleton<IPluginsService>(provider => new PluginsService(
                provider.GetRequiredService<IEditorOptionService>()
            ));
            return services;
        }

        private static IServiceCollection RegisterFactoryServices(this IServiceCollection services)
        {
            services.AddSingleton<IWorkToolsUnitsFactory, WorkToolsUnitsFactory>();
            services.AddSingleton<ISnippetUnitsFactory, SnippetUnitsFactory>();
            services.AddSingleton<IKnowledgeUnitsFactory, KnowledgeUnitsFactory>();
            services.AddSingleton<ICheatSheetUnitsFactory, CheatSheetUnitsFactory>();
            services.AddSingleton<IOptionUnitsFactory, OptionUnitsFactory>();
            services.AddSingleton<ICoreUnitsFactory, CoreUnitsFactory>();
            services.AddSingleton<ICompositeUnitsFactory, CompositeUnitsFactory>();
            
            return services;
        }

        public static IServiceCollection RegisterInitializer(this IServiceCollection services)
        {
            services.AddSingleton<Initializer>(provider => new Initializer(
                provider.GetRequiredService<IUnitsService>(),
                provider.GetRequiredService<ICommandService>(),
                provider,
                provider.GetRequiredService<IWindowHandleService>(),
                provider.GetRequiredService<IPickerService>(),
                provider.GetRequiredService<IAppCommands>(),
                provider.GetRequiredService<IEditorOptionService>(),
                provider.GetRequiredService<ISnippetOptionService>(),
                provider.GetRequiredService<IWorkItemOptionService>(),
                provider.GetRequiredService<IPluginsService>(),
                provider.GetRequiredService<IApplicationLogService>()
            ));
            return services;
        }
    }
}
