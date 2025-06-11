using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Services.Markdown;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Options;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.Factories
{
    public interface IWorkToolsUnitsFactory : IUnitsFactory, IFeaturesFactory
    {
        IEnumerable<IUnit> CreateUnits();
    }

    public class WorkToolsUnitsFactory : IWorkToolsUnitsFactory
    {
        private readonly IWorkItemOptionService workItemOptionService;

        public WorkToolsUnitsFactory(IWorkItemOptionService workItemOptionService)
        {
            this.workItemOptionService = workItemOptionService;
        }

        public IEnumerable<IUnit> GetUnits()
        {
            var markdownService = new MarkdownService();
            CategoryOrderManager.ClearCategoryOrder();
            
            foreach (var workItemPath in workItemOptionService.Option.ItemPaths)
            {
                var actualPath = workItemPath.GetActualPath();
                
                var headerOrder = markdownService.GetHeaderOrder(actualPath);
                foreach (var kvp in headerOrder)
                {
                    CategoryOrderManager.SetCategoryOrder(kvp.Key, kvp.Value);
                }
                
                var links = markdownService.GetLinks(actualPath);

                foreach (var link in links)
                {
                    yield return new UrlUnit(WorkToolsMode.Instance, link.Text, link.Url, link.ContainingHeader);
                }
            }
        }

        public IEnumerable<IUnit> CreateUnits()
        {
            return GetUnits();
        }

        public IEnumerable<IFeature> GetFeatures()
        {
            var lists = new List<string>();
            lists.Add(workItemOptionService.OptionPath);
            lists.AddRange(workItemOptionService.Option.ItemPaths.Select(x => x.GetActualPath()));
            
            yield return new OptionFeature(WorkToolsMode.Instance,"WorkTools Option", lists);
        }
    }
}