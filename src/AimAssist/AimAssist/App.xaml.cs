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
using AimAssist.Services.Initialization;

namespace AimAssist
{
    public partial class App : System.Windows.Application
    {
        private static Mutex? mutex;
        private const string appName = "AimAssist";
        private const string PipeName = "AimAssist";
        public ServiceProvider _serviceProvider { get; private set; } = null!;

        private async void Application_Startup(object? sender, System.Windows.StartupEventArgs e)
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
                    // 非同期初期化の開始
                    var initializer = _serviceProvider.GetRequiredService<IApplicationInitializationService>();
                    await InitializeApplicationAsync(initializer, errorHandler);
                }
                catch (Exception ex)
                {
                    errorHandler.HandleException(ex, "アプリケーション初期化エラー");
                    Shutdown();
                    return;
                }

                ThreadPool.QueueUserWorkItem(WaitCallActivate);
            }
            else
            {
                ActivateAimAssistAnotherProcess();
                Shutdown();
            }
        }

        private async Task InitializeApplicationAsync(IApplicationInitializationService initializer, IErrorHandlingMiddleware errorHandler)
        {
            try
            {
                // 初期化処理を非同期で実行
                await initializer.InitializeAsync();

                // 初期化完了後にMainWindowを表示
                await Dispatcher.InvokeAsync(() =>
                {
                    var appCommands = _serviceProvider.GetRequiredService<IAppCommands>();
                    appCommands.ToggleMainWindow.Execute(this);
                });
            }
            catch (Exception ex)
            {
                errorHandler.HandleException(ex, "非同期初期化中にエラーが発生しました");
                throw;
            }
        }
        
        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            services.RegisterServices();
            
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
