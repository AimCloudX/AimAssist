using AimAssist.Commands;
using AimAssist.Plugins;
using AimAssist.Service;
using AimAssist.UI.Tools.HotKeys;
using AimAssist.Unit.Implementation.Knoledges;
using AimAssist.Unit.Implementation.Snippets;
using AimAssist.Unit.Implementation.Standard;
using AimAssist.Unit.Implementation.Web.Bookmarks;
using AimAssist.Unit.Implementation.Web.BookSearch;
using AimAssist.Unit.Implementation.Web.Urls;
using AimAssist.Unit.Implementation.WorkFlows;
using Common.UI.Editor;
using System.Windows;

namespace AimAssist
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            RegisterUnitsFactory();

            GenelateNotifyIcon();

            var window = new HowKeysWindow();
            window.Show();

            var aa = new Window();
            aa.Content = new MonacoEditor();
            aa.Width = 400;
            aa.Height = 400;
            aa.Show();
        }

        private void GenelateNotifyIcon()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Show PickerWindow", null, Show_Click);
            menu.Items.Add("Quit AimAssist", null, Exit_Click);
            var icon = GetResourceStream(new Uri("Resources/Icons/AimAssist.ico", UriKind.Relative)).Stream;
            var notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = new Icon(icon),
                Text = "AimAssist",
                ContextMenuStrip = menu,
            };
            notifyIcon.MouseClick += new MouseEventHandler(NotifyIcon_Click);
        }

        private static void RegisterUnitsFactory()
        {
            // TODO 場所の移動
            var factory = UnitsService.Instnace;
            factory.RegisterFactory(new ModeChangeUnitsFacotry());
            factory.RegisterFactory(new SnippetUnitsFactory());
            factory.RegisterFactory(new ChatGPTUnitsFactory());
            factory.RegisterFactory(new BookSearchUnitsFactory());
            factory.RegisterFactory(new KnowledgeUnitsFactory());
            factory.RegisterFactory(new BookmarkUnitsFacotry());
            factory.RegisterFactory(new UrlUnitsFacotry());

            var pluginService = new PluginsService();
            pluginService.LoadCommandPlugins();
            var facotries = pluginService.GetFactories();
            foreach (var item in facotries)
            {
                factory.RegisterFactory(item);
            }
        }

        private void Show_Click(object? sender, EventArgs e)
        {
            PickerCommands.ShowWindowCommand.Execute(e);
        }

        private void NotifyIcon_Click(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PickerCommands.ShowWindowCommand.Execute(e);
            }
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            Shutdown();
        }
    }

}
