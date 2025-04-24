using AimAssist.Core.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Service;
using Library.Options;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.IO.Pipes;

namespace AimAssist
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static Mutex mutex;
        private const string appName = "AimAssist";
        private const string PipeName = "AimAssist";
        public ServiceProvider _serviceProvider { get; private set; }

        /// <summary>
        /// アプリケーションの起動時に呼び出されるメソッド
        /// </summary>
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            // DI コンテナを設定
            ConfigureServices();

            mutex = new Mutex(true, appName, out var createdNew);
            if (createdNew)
            {
                // DIコンテナからInitializerを取得して使用
                var initializer = _serviceProvider.GetRequiredService<Initializer>();
                initializer.Initialize();
                
                var settings = new SettingManager().LoadSettings();
                var commandService = _serviceProvider.GetRequiredService<ICommandService>();
                commandService.SetKeymap(settings);

                ThreadPool.QueueUserWorkItem(WaitCallActivate);// 名前付きパイプサーバーを起動 別プロセスからのActivate用

                AppCommands.ToggleMainWindow.Execute(this);
            }
            else
            {
                ActivateAimAssistAnotherProcess();
                Shutdown();
            }
        }
        
        /// <summary>
        /// DIコンテナの設定
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            
            // サービスを登録
            services.AddSingleton<IUnitsService, UnitsService>();
            services.AddSingleton<ICommandService, CommandService>();
            services.AddSingleton<IApplicationLogService, ApplicationLogService>();
            services.AddSingleton<PickerService>();
            services.AddSingleton<WindowHandleService>();
            //services.AddSingleton<CheatSheet.Services.CheatSheetController>(provider =>
            //    new CheatSheet.Services.CheatSheetController(
            //        Dispatcher.CurrentDispatcher, 
            //        provider.GetRequiredService<WindowHandleService>()
            //    ));
            services.AddSingleton<UI.UnitContentsView.UnitViewFactory>();
            services.AddTransient<UI.MainWindows.MainWindow>();
            services.AddTransient<UI.Tools.HotKeys.WaitHotKeysWindow>();
            
            // 他のサービスを登録
            
            // ファクトリーパターンでInitializerを登録
            services.AddSingleton<Initializer>(provider => new Initializer(
                provider.GetRequiredService<IUnitsService>(),
                provider.GetRequiredService<ICommandService>(),
                provider.GetRequiredService<IApplicationLogService>(),
                provider,
                provider.GetRequiredService<WindowHandleService>(),
                provider.GetRequiredService<PickerService>()
            ));
            
            _serviceProvider = services.BuildServiceProvider();
        }

        private static void ActivateAimAssistAnotherProcess()
        {
            using (var client = new NamedPipeClientStream(PipeName))
            {
                try
                {
                    client.Connect(1000); // 1秒待機
                    using var writer = new StreamWriter(client);
                    writer.WriteLine(PipeName);
                    writer.Flush();
                }
                catch (TimeoutException)
                {
                    // クライアントが接続できなかった場合の処理
                }
            }
        }


        private void WaitCallActivate(object state)
        {
            while (true)
            {
                using var server = new NamedPipeServerStream(PipeName);
                server.WaitForConnection();
                using var reader = new StreamReader(server);
                if (reader.ReadLine() == PipeName)
                {
                    Dispatcher.Invoke(() => AppCommands.ToggleMainWindow.Execute(this));
                }
            }
        }

        private void Application_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            var commandService = _serviceProvider.GetRequiredService<ICommandService>();
            var settings = commandService.GetKeymap();
            var noneSettingsKeys = settings.Where(x => x.Value.FirstModifiers == 0).Select(y=>y.Key);
            foreach (var key in noneSettingsKeys)
            {
                settings.Remove(key);
            }

            new SettingManager().SaveSettings(settings);
            EditorOptionService.SaveOption();
            mutex.ReleaseMutex();
        }
    }
}
