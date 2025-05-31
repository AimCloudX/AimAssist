using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Snippets;
using Library.Options;

namespace AimAssist.Units.Implementation.Factories
{
    public interface ISnippetUnitsFactory
    {
        IEnumerable<IUnit> CreateUnits();
    }

    public class SnippetUnitsFactory : ISnippetUnitsFactory
    {
        private readonly ISnippetOptionService _snippetOptionService;

        public SnippetUnitsFactory(ISnippetOptionService snippetOptionService)
        {
            _snippetOptionService = snippetOptionService;
        }

        public IEnumerable<IUnit> CreateUnits()
        {
            var parser = new SnippetParser();
            foreach (var path in _snippetOptionService.Option.ItemPaths)
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
