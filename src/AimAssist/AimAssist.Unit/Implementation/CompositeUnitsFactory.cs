using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Factories;

namespace AimAssist.Units.Implementation
{
    public interface ICompositeUnitsFactory
    {
        IEnumerable<IUnit> GetUnits();
    }

    public class CompositeUnitsFactory : ICompositeUnitsFactory
    {
        private readonly IWorkToolsUnitsFactory workToolsFactory;
        private readonly ISnippetUnitsFactory snippetFactory;
        private readonly IKnowledgeUnitsFactory knowledgeFactory;
        private readonly ICheatSheetUnitsFactory cheatSheetFactory;
        private readonly IOptionUnitsFactory optionFactory;
        private readonly ICoreUnitsFactory coreFactory;

        public CompositeUnitsFactory(
            IWorkToolsUnitsFactory workToolsFactory,
            ISnippetUnitsFactory snippetFactory,
            IKnowledgeUnitsFactory knowledgeFactory,
            ICheatSheetUnitsFactory cheatSheetFactory,
            IOptionUnitsFactory optionFactory,
            ICoreUnitsFactory coreFactory)
        {
            this.workToolsFactory = workToolsFactory;
            this.snippetFactory = snippetFactory;
            this.knowledgeFactory = knowledgeFactory;
            this.cheatSheetFactory = cheatSheetFactory;
            this.optionFactory = optionFactory;
            this.coreFactory = coreFactory;
        }

        public IEnumerable<IUnit> GetUnits()
        {
            var allUnits = new List<IUnit>();

            allUnits.AddRange(workToolsFactory.CreateUnits());
            allUnits.AddRange(snippetFactory.CreateUnits());
            allUnits.AddRange(knowledgeFactory.CreateUnits());
            allUnits.AddRange(cheatSheetFactory.CreateUnits());
            allUnits.AddRange(optionFactory.CreateUnits());
            allUnits.AddRange(coreFactory.CreateUnits());

            return allUnits;
        }
    }
}
