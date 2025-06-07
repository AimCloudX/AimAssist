using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Factories;
using AimAssist.Units.Implementation.ClipboardAnalyzer;

namespace AimAssist.Units.Implementation.Factories
{
    public class AutoDiscoveryUnitsFactory : AbstractUnitsFactory
    {
        private readonly ICoreUnitsFactory coreUnitsFactory;
        private readonly IKnowledgeUnitsFactory knowledgeUnitsFactory;
        private readonly IWorkToolsUnitsFactory workToolsUnitsFactory;
        private readonly ISnippetUnitsFactory snippetUnitsFactory;
        private readonly ICheatSheetUnitsFactory cheatSheetUnitsFactory;
        private readonly IOptionUnitsFactory optionUnitsFactory;

        public AutoDiscoveryUnitsFactory(
            ICoreUnitsFactory coreUnitsFactory,
            IKnowledgeUnitsFactory knowledgeUnitsFactory,
            IWorkToolsUnitsFactory workToolsUnitsFactory,
            ISnippetUnitsFactory snippetUnitsFactory,
            ICheatSheetUnitsFactory cheatSheetUnitsFactory,
            IOptionUnitsFactory optionUnitsFactory)
            : base("AutoDiscovery", priority: 1000)
        {
            this.coreUnitsFactory = coreUnitsFactory;
            this.knowledgeUnitsFactory = knowledgeUnitsFactory;
            this.workToolsUnitsFactory = workToolsUnitsFactory;
            this.snippetUnitsFactory = snippetUnitsFactory;
            this.cheatSheetUnitsFactory = cheatSheetUnitsFactory;
            this.optionUnitsFactory = optionUnitsFactory;
        }

        public override IEnumerable<IUnit> CreateUnits()
        {
            System.Diagnostics.Debug.WriteLine("AutoDiscoveryUnitsFactory.CreateUnits() started");
            var count = 0;
            
            foreach (var unit in coreUnitsFactory.CreateUnits())
            {
                count++;
                System.Diagnostics.Debug.WriteLine($"  Core unit #{count}: {unit.GetType().Name} - {unit.Name}");
                yield return unit;
            }

            foreach (var unit in knowledgeUnitsFactory.CreateUnits())
            {
                count++;
                System.Diagnostics.Debug.WriteLine($"  Knowledge unit #{count}: {unit.GetType().Name} - {unit.Name}");
                yield return unit;
            }

            foreach (var unit in workToolsUnitsFactory.CreateUnits())
            {
                count++;
                System.Diagnostics.Debug.WriteLine($"  WorkTools unit #{count}: {unit.GetType().Name} - {unit.Name}");
                yield return unit;
            }

            foreach (var unit in snippetUnitsFactory.CreateUnits())
            {
                count++;
                System.Diagnostics.Debug.WriteLine($"  Snippet unit #{count}: {unit.GetType().Name} - {unit.Name}");
                yield return unit;
            }

            foreach (var unit in cheatSheetUnitsFactory.CreateUnits())
            {
                count++;
                System.Diagnostics.Debug.WriteLine($"  CheatSheet unit #{count}: {unit.GetType().Name} - {unit.Name}");
                yield return unit;
            }

            count++;
            System.Diagnostics.Debug.WriteLine($"  Direct unit #{count}: ClipboardUnit");
            yield return new ClipboardUnit();

            foreach (var unit in optionUnitsFactory.CreateUnits())
            {
                count++;
                System.Diagnostics.Debug.WriteLine($"  Option unit #{count}: {unit.GetType().Name} - {unit.Name}");
                yield return unit;
            }
            
            System.Diagnostics.Debug.WriteLine($"AutoDiscoveryUnitsFactory.CreateUnits() completed. Total: {count} units");
        }
    }
}
