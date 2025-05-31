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
            foreach (var workItemPath in _workItemOptionService.Option.ItemPaths)
            {
                var links = new MarkdownService().GetLinks(workItemPath.GetActualPath());

                foreach (var link in links)
                {
                    yield return new UrlUnit(WorkToolsMode.Instance, link.Text, link.Url, link.ContainingHeader);
                }
            }
        }
    }
}
