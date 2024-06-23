using AimAssist.Plugins;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Commands;
using AimAssist.Unit.Implementation.Knoledges;
using AimAssist.Unit.Implementation.Options;
using AimAssist.Unit.Implementation.Snippets;
using AimAssist.Unit.Implementation.Speech;
using AimAssist.Unit.Implementation.Standard;
using AimAssist.Unit.Implementation.Web.Bookmarks;
using AimAssist.Unit.Implementation.Web.BookSearch;
using AimAssist.Unit.Implementation.Web.Rss;
using AimAssist.Unit.Implementation.Web.Urls;
using AimAssist.Unit.Implementation.WorkTools;

namespace AimAssist.Service
{
    public class UnitsService
    {
        private static UnitsService? instance;

        public static UnitsService Instnace
        {
            get
            {
                if (instance == null)
                {
                    var factory = new UnitsService();
                    instance = factory;
                }

                return instance;
            }
        }

        public void Initialize()
        {
            Instnace.RegisterFactory(new ChatGPTUnitsFactory());
            Instnace.RegisterFactory(new SpeechUnitFactory());

            Instnace.RegisterFactory(new KnowledgeUnitsFactory());
            Instnace.RegisterFactory(new SnippetUnitsFactory());

            Instnace.RegisterFactory(new UrlUnitsFacotry());

            Instnace.RegisterFactory(new BookSearchUnitsFactory());
            Instnace.RegisterFactory(new RssUnitsFactory());
            Instnace.RegisterFactory(new BookmarkUnitsFacotry());

            Instnace.RegisterFactory(new AppCommandUnitFactory());
            Instnace.RegisterFactory(new OptionUnitsFactory());

            var pluginService = new PluginsService();
            pluginService.LoadCommandPlugins();
            var facotries = pluginService.GetFactories();
            foreach (var item in facotries)
            {
                Instnace.RegisterFactory(item);
            }
        }

        private IList<IUnitsFacotry> factories = new List<IUnitsFacotry>();

        public void RegisterFactory (IUnitsFacotry factory)
        {
            factories.Add(factory);
        }

        public IEnumerable<IPickerMode> AllMode()
        {
            return this.factories.Select(x=>x.TargetMode).Distinct().ToList();
        }
        public IPickerMode GetModeFromText(string text)
        {
            return StandardMode.Instance;
        }

        public async IAsyncEnumerable<IUnit> CreateUnits(IPickerMode mode, string inputText)
        {
            var paramter = new UnitsFactoryParameter(inputText);
            switch (mode)
            {
                case StandardMode:
                    foreach (var factory in this.factories.Where(x=>x.IsShowInStnadard))
                    {
                        await foreach (var units in factory.GetUnits(paramter))
                        {
                            yield return units;
                        }
                    }

                    break;
                default:
                    foreach (var factory in this.factories.Where(x=>x.TargetMode == mode))
                    {
                        var units = factory.GetUnits(paramter);
                        await foreach (var unit in units)
                        {
                            yield return unit;
                        }
                    }
                    break;
            }
        }

        public void Dispose()
        {
            foreach (var disposable in factories.OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            factories.Clear();
        }
    }
}
