using AimAssist.Commands;
using AimAssist.Core.Options;
using AimAssist.Service;
using AimAssist.UI.SystemTray;
using AimAssist.UI.Tools.HotKeys;
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

                AimAssistCommands.ToggleAssistWindowCommand.Execute();
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
            CommandService.Initialize();

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
                    Dispatcher.Invoke(() => AimAssistCommands.ToggleAssistWindowCommand.Execute());
                }
            }
        }
    }
}
