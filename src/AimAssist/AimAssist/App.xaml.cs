using AimAssist.Core.Commands;
using AimAssist.Service;
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
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            mutex = new Mutex(true, appName, out var createdNew);
            if (createdNew)
            {
                new Initializer().Initialize();
                var settings = new SettingManager().LoadSettings();
                CommandService.SetKeymap(settings);

                ThreadPool.QueueUserWorkItem(WaitCallActivate);// 名前付きパイプサーバーを起動 別プロセスからのActivate用

                AppCommands.ToggleMainWindow.Execute(this);
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
            var settings = CommandService.GetKeymap();
            new SettingManager().SaveSettings(settings);
            EditorOptionService.SaveOption();
            mutex.ReleaseMutex();

        }
    }
}
