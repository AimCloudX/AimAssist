using AimAssist.Core.Helpers;
using AimAssist.Core.Interfaces;
using AimAssist.DI;
using AimAssist.Middlewares;
using AimAssist.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AimAssist
{
    public partial class App : System.Windows.Application
    {
        private static Mutex? mutex;
        private const string appName = "AimAssist";
        private const string PipeName = "AimAssist";
        public ServiceProvider _serviceProvider { get; private set; } = null!;

        private void Application_Startup(object? sender, System.Windows.StartupEventArgs e)
        {
            ConfigureServices();

            var errorHandler = _serviceProvider.GetRequiredService<IErrorHandlingMiddleware>();
            errorHandler.RegisterGlobalHandlers();

            mutex = new Mutex(true, appName, out var createdNew);
            if (createdNew)
            {
                var logService = _serviceProvider.GetRequiredService<IApplicationLogService>();
                ErrorHandlingHelper.SetLogService(logService);
                
                try
                {
                    var initializer = _serviceProvider.GetRequiredService<Initializer>();
                    initializer.Initialize();
                }
                catch (Exception ex)
                {
                    errorHandler.HandleException(ex, "アプリケーション初期化エラー");
                    return;
                }

                ThreadPool.QueueUserWorkItem(WaitCallActivate);

                var appCommands = _serviceProvider.GetRequiredService<IAppCommands>();
                appCommands.ToggleMainWindow.Execute(this);
            }
            else
            {
                ActivateAimAssistAnotherProcess();
                Shutdown();
            }
        }
        
        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            
            services.RegisterServices();
            services.AddSingleton<Initializer>();
            
            _serviceProvider = services.BuildServiceProvider();
            
            // Make service provider available to other components
            this.Properties["ServiceProvider"] = _serviceProvider;
        }

        private static void ActivateAimAssistAnotherProcess()
        {
            using (var client = new NamedPipeClientStream(PipeName))
            {
                try
                {
                    client.Connect(1000);
                    using var writer = new StreamWriter(client);
                    writer.WriteLine(PipeName);
                    writer.Flush();
                }
                catch (TimeoutException)
                {
                }
            }
        }

        private void WaitCallActivate(object? state)
        {
            while (true)
            {
                using var server = new NamedPipeServerStream(PipeName);
                server.WaitForConnection();
                using var reader = new StreamReader(server);
                if (reader.ReadLine() == PipeName)
                {
                    Dispatcher.Invoke(() => 
                    {
                        var appCommands = _serviceProvider.GetRequiredService<IAppCommands>();
                        appCommands.ToggleMainWindow.Execute(this);
                    });
                }
            }
        }

        private void Application_Exit(object? sender, System.Windows.ExitEventArgs e)
        {
            try
            {
                var applicationService = _serviceProvider.GetRequiredService<IApplicationService>();
                // Use Task.Run to avoid potential deadlocks in exit handler
                Task.Run(async () => await applicationService.ShutdownAsync()).Wait(TimeSpan.FromSeconds(5));
                mutex?.ReleaseMutex();
            }
            catch (Exception ex)
            {
                var errorHandler = _serviceProvider.GetRequiredService<IErrorHandlingMiddleware>();
                errorHandler.HandleException(ex, "アプリケーション終了エラー");
            }
        }
    }
}
