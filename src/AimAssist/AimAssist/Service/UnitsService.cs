using AimAssist.Plugins;
using AimAssist.UI.UnitContentsView;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Knoledges;
using AimAssist.Units.Implementation.Knowledge;
using AimAssist.Units.Implementation.Options;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.Speech;
using AimAssist.Units.Implementation.Standard;
using AimAssist.Units.Implementation.Web.BookSearch;
using AimAssist.Units.Implementation.Web.Rss;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Service
{
    public class UnitsService
    {
        private static UnitsService? instance;

        private List<IMode> modes = new List<IMode>();

        private List<IUnitsFacotry> facotry  = new(); 

        public void Register(IMode mode)
        {
            modes.Add(mode);
        }

        public IReadOnlyCollection<IMode> GetAllModes()
        {
            return modes;
        }

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

            Register(AllInclusiveMode.Instance);
            Register(ActiveUnitMode.Instance);
            Register(WorkToolsMode.Instance);
            Register(BookSearchMode.Instance);
            Register(RssMode.Instance);
            Register(SnippetMode.Instance);
            Register(KnowledgeMode.Instance);
            Register(OptionMode.Instance);

            Instnace.facotry.Add(new ChatGPTUnitsFactory());
            Instnace.facotry.Add(new SpeechUnitFactory());
            Instnace.facotry.Add(new KnowledgeUnitsFactory());

            Instnace.facotry.Add(new SnippetUnitsFactory());
            Instnace.facotry.Add(new BookSearchUnitsFactory());
            Instnace.facotry.Add(new RssUnitsFactory());

            Instnace.facotry.Add(new OptionUnitsFactory());
            Instnace.facotry.Add(new ShortcutUnitsFacotry());

            var pluginService = new PluginsService();
            pluginService.LoadCommandPlugins();
            var facotries = pluginService.GetFactories();
            foreach (var item in facotries)
            {
                facotry.Add(item);
            }

            var converters = pluginService.GetConterters();
            foreach (var item in converters)
            {
                UnitViewFactory.UnitToUIElementDicotionary.TryAdd(item.Key, item.Value);
            }
        }

        public async IAsyncEnumerable<IUnit> CreateUnits(IMode mode)
        {
            switch (mode)
            {
                case AllInclusiveMode:
                    foreach (var factory in this.facotry)
                    {
                        await foreach (var units in factory.GetUnits())
                        {
                            yield return units;
                        }
                    }

                    break;
                default:
                    foreach (var fac in this.facotry.Where(x=>x.TargetMode == mode))
                    {
                        var units = fac.GetUnits();
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
            foreach (var disposable in facotry.OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            facotry.Clear();
        }
    }
}
