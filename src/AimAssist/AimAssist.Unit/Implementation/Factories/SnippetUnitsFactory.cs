using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Snippets;

namespace AimAssist.Units.Implementation.Factories
{
    public interface ISnippetUnitsFactory
    {
        IEnumerable<IUnit> CreateUnits();
    }

    public class SnippetUnitsFactory : ISnippetUnitsFactory
    {
        private readonly ISnippetOptionService snippetOptionService;

        public SnippetUnitsFactory(ISnippetOptionService snippetOptionService)
        {
            this.snippetOptionService = snippetOptionService;
        }

        public IEnumerable<IUnit> CreateUnits()
        {
            var parser = new SnippetParser();
            foreach (var path in snippetOptionService.Option.ItemPaths)
            {
                var snippets = parser.ParseMarkdownFile(path.GetActualPath());
                foreach (var snippet in snippets)
                {
                    yield return new SnippetModelUnit(snippet);
                }
            }
        }
    }
}
