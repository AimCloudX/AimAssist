﻿using System;
using System.Reflection;
using System.Linq;
using AimAssist.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Service;
using AimAssist.UI.HotKeys;
using AimAssist.UI.MainWindows;
using AimAssist.UI.PickerWindows;
using AimAssist.UI.UnitContentsView;
using AimAssist.Units.ViewProviders;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.DI.Modules
{
    public class UiServiceModule : IServiceModule
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

            services.AddSingleton<UI.SystemTray.SystemTrayRegister>(provider =>
                new UI.SystemTray.SystemTrayRegister(provider.GetRequiredService<IAppCommands>())
            );

            RegisterViewProviders(services);

            services.AddSingleton<UI.UnitContentsView.UnitViewFactory>(provider => new UI.UnitContentsView.UnitViewFactory(
                provider.GetRequiredService<ICommandService>(),
                provider.GetRequiredService<IEditorOptionService>(),
                provider,
                provider.GetServices<IViewProvider>()
            ));

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<PickerWindowViewModel>();

            services.AddSingleton<IMainWindow>(provider => new MainWindow(
                provider.GetRequiredService<MainWindowViewModel>(),
                provider.GetRequiredService<IApplicationLogService>(),
                (KeySequenceManager)provider.GetRequiredService<IKeySequenceManager>(),
                provider.GetRequiredService<IUnitsService>(),
                provider.GetRequiredService<ICommandService>()
            ));

            services.AddTransient<WaitHotKeysWindow>(provider => new WaitHotKeysWindow(
                provider.GetRequiredService<ICommandService>(),
                provider.GetRequiredService<IAppCommands>(),
                provider.GetRequiredService<ICheatSheetController>()
            ));

            return services;
        }

        private void RegisterViewProviders(IServiceCollection services)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.Contains("AimAssist") == true)
                .ToList();

            var viewProviderTypes = assemblies
                .SelectMany(assembly => 
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        return Enumerable.Empty<Type>();
                    }
                })
                .Where(type => type.IsClass && 
                              !type.IsAbstract && 
                              typeof(IViewProvider).IsAssignableFrom(type) &&
                              type.GetCustomAttribute<ViewProviderAttribute>() != null)
                .ToList();

            foreach (var type in viewProviderTypes)
            {
                services.AddTransient(typeof(IViewProvider), type);
            }
        }
    }
}
