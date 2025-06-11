using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Snippets;

namespace AimAssist.Units.Implementation.Factories
{
    public class SnippetFeaturesFactory : IFeaturesFactory
    {
        private readonly ISnippetOptionService snippetOptionService;

        public SnippetFeaturesFactory(ISnippetOptionService snippetOptionService)
        {
            this.snippetOptionService = snippetOptionService;
        }

        public IEnumerable<IFeature> GetFeatures()
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
