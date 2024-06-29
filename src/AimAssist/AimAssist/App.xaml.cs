using AimAssist.Core.Commands;
using AimAssist.Service;
using AimAssist.UI.SystemTray;
using AimAssist.UI.Tools.HotKeys;
using Common.Commands.Shortcus;
using Library.Options;
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
        public static SettingManager SettingsManager { get; private set; }

        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            mutex = new Mutex(true, appName, out var createdNew);
            if (createdNew)
            {
                Initialize();

                ThreadPool.QueueUserWorkItem(WaitCallActivate);// 名前付きパイプサーバーを起動 別プロセスからのActivate用

                Exit += (object _, System.Windows.ExitEventArgs _) => {
                    EditorOptionService.SaveOption();
                    mutex.ReleaseMutex();
                };

                AppCommands.ToggleMainWindow.Execute();
            }
            else
            {
                ActivateAimAssistAnotherProcess();
                Shutdown();
            }
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

        private static void Initialize()
        {
            EditorOptionService.LoadOption();
            SystemTrayRegister.Register();
            UnitsService.Instnace.Initialize();
            CommandService.Register(AppCommands.ToggleMainWindow, new KeySequence(System.Windows.Input.Key.A, System.Windows.Input.ModifierKeys.Alt));
            CommandService.Register(AppCommands.ShowPickerWindow, new KeySequence(System.Windows.Input.Key.P, System.Windows.Input.ModifierKeys.Alt));
            CommandService.Register(AppCommands.ShutdownAimAssist, new KeySequence(System.Windows.Input.Key.D, System.Windows.Input.ModifierKeys.Control));
            SettingsManager = new SettingManager();
            var settings = SettingsManager.LoadSettings();
            CommandService.SetKeymap(settings);

            new WaitHowKeysWindow().Show();
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
                    Dispatcher.Invoke(() => AppCommands.ToggleMainWindow.Execute());
                }
            }
        }

        private void Application_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            var settings = CommandService.GetKeymap();
            SettingsManager.SaveSettings(settings);
        }
    }
}
