using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Services.Markdown;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.Factories
{
    public interface IWorkToolsUnitsFactory
    {
        IEnumerable<IUnit> CreateUnits();
    }

    public class WorkToolsUnitsFactory : IWorkToolsUnitsFactory
    {
        private readonly IWorkItemOptionService _workItemOptionService;

        public WorkToolsUnitsFactory(IWorkItemOptionService workItemOptionService)
        {
            _workItemOptionService = workItemOptionService;
        }

        public IEnumerable<IUnit> CreateUnits()
        {
            var markdownService = new MarkdownService();
            CategoryOrderManager.ClearCategoryOrder();
            
            foreach (var workItemPath in _workItemOptionService.Option.ItemPaths)
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
    }
}
