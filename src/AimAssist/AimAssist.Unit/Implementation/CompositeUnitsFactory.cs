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
        private readonly IWorkToolsUnitsFactory _workToolsFactory;
        private readonly ISnippetUnitsFactory _snippetFactory;
        private readonly IKnowledgeUnitsFactory _knowledgeFactory;
        private readonly ICheatSheetUnitsFactory _cheatSheetFactory;
        private readonly IOptionUnitsFactory _optionFactory;
        private readonly ICoreUnitsFactory _coreFactory;

        public CompositeUnitsFactory(
            IWorkToolsUnitsFactory workToolsFactory,
            ISnippetUnitsFactory snippetFactory,
            IKnowledgeUnitsFactory knowledgeFactory,
            ICheatSheetUnitsFactory cheatSheetFactory,
            IOptionUnitsFactory optionFactory,
            ICoreUnitsFactory coreFactory)
        {
            _workToolsFactory = workToolsFactory;
            _snippetFactory = snippetFactory;
            _knowledgeFactory = knowledgeFactory;
            _cheatSheetFactory = cheatSheetFactory;
            _optionFactory = optionFactory;
            _coreFactory = coreFactory;
        }

        public IEnumerable<IUnit> GetUnits()
        {
            var allUnits = new List<IUnit>();

            allUnits.AddRange(_workToolsFactory.CreateUnits());
            allUnits.AddRange(_snippetFactory.CreateUnits());
            allUnits.AddRange(_knowledgeFactory.CreateUnits());
            allUnits.AddRange(_cheatSheetFactory.CreateUnits());
            allUnits.AddRange(_optionFactory.CreateUnits());
            allUnits.AddRange(_coreFactory.CreateUnits());

            return allUnits;
        }
    }
}
