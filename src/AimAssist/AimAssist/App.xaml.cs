using AimAssist.Core.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Plugins;
using AimAssist.Service;
using Library.Options;
using Microsoft.Extensions.DependencyInjection;
using AimAssist.Units.Implementation.Snippets;
using System.IO;
using System.IO.Pipes;
using AimAssist.Units.Implementation.WorkTools;
using System.Windows.Forms;

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
                try
                {
                    var initializer = _serviceProvider.GetRequiredService<Initializer>();
                    initializer.Initialize();
                    
                    var settingManager = _serviceProvider.GetRequiredService<ISettingManager>();
                    var settings = settingManager.LoadSettings();
                    var commandService = _serviceProvider.GetRequiredService<ICommandService>();
                    commandService.SetKeymap(settings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"アプリケーションの初期化中にエラーが発生しました。\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                ThreadPool.QueueUserWorkItem(WaitCallActivate);// 名前付きパイプサーバーを起動 別プロセスからのActivate用

                var appCommands = _serviceProvider.GetRequiredService<IAppCommands>();
                appCommands.ToggleMainWindow.Execute(this);
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
            services.AddSingleton<ISettingManager, SettingManager>();
            services.AddSingleton<IKeySequenceManager, KeySequenceManager>();
            services.AddSingleton<ISnippetOptionService, SnippetOptionService>();
            services.AddSingleton<IWorkItemOptionService, WorkItemOptionService>();
            services.AddSingleton<IPluginsService>(provider => new PluginsService(
                provider.GetRequiredService<IEditorOptionService>()
            ));
            services.AddSingleton<IPickerService, PickerService>();
            services.AddSingleton<IWindowHandleService, WindowHandleService>();
            services.AddSingleton<IAppCommands>(provider => new AppCommands(
                provider.GetRequiredService<IWindowHandleService>(),
                provider.GetRequiredService<IPickerService>()
            ));
            services.AddSingleton<UI.SystemTray.SystemTrayRegister>(provider => 
                new UI.SystemTray.SystemTrayRegister(provider.GetRequiredService<IAppCommands>())
            );
            services.AddSingleton<ICheatSheetController>(provider =>
                new CheatSheet.Services.CheatSheetController(
                    System.Windows.Threading.Dispatcher.CurrentDispatcher, 
                    provider.GetRequiredService<IWindowHandleService>()
                ));
            services.AddSingleton<UI.UnitContentsView.UnitViewFactory>(provider => new UI.UnitContentsView.UnitViewFactory(
                provider.GetRequiredService<ICommandService>(),
                provider.GetRequiredService<IEditorOptionService>()
            ));
            services.AddTransient<UI.MainWindows.MainWindow>();
            services.AddTransient<UI.Tools.HotKeys.WaitHotKeysWindow>(provider => new UI.Tools.HotKeys.WaitHotKeysWindow(
                provider.GetRequiredService<ICommandService>(),
                provider.GetRequiredService<IAppCommands>()
            ));
            
            services.AddSingleton<IEditorOptionService, EditorOptionService>();
            
            // ファクトリーパターンでInitializerを登録
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
                provider.GetRequiredService<IPluginsService>()
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
                    Dispatcher.Invoke(() => 
                    {
                        var appCommands = _serviceProvider.GetRequiredService<IAppCommands>();
                        appCommands.ToggleMainWindow.Execute(this);
                    });
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

            _serviceProvider.GetRequiredService<ISettingManager>().SaveSettings(settings);
            _serviceProvider.GetRequiredService<IEditorOptionService>().SaveOption();
            _serviceProvider.GetRequiredService<ISnippetOptionService>().SaveOption();
            _serviceProvider.GetRequiredService<IWorkItemOptionService>().SaveOption();
            mutex.ReleaseMutex();
        }
    }
}
