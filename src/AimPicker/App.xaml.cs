using AimPicker.Plugins;
using AimPicker.Service;
using AimPicker.UI.Tools.HotKeys;
using AimPicker.Unit.Implementation.Knoledges;
using AimPicker.Unit.Implementation.Snippets;
using AimPicker.Unit.Implementation.Standard;
using AimPicker.Unit.Implementation.Web.Bookmarks;
using AimPicker.Unit.Implementation.Web.BookSearch;
using AimPicker.Unit.Implementation.Web.Urls;
using AimPicker.Unit.Implementation.WorkFlows;

namespace AimPicker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
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

            var window = new HowKeysWindow();
            window.Show();
        }
    }

}
