using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Factories;
using AimAssist.Units.Implementation.ClipboardAnalyzer;

namespace AimAssist.Units.Implementation.Factories
{
    public class AutoDiscoveryUnitsFactory : AbstractUnitsFactory
    {
        private readonly IKnowledgeUnitsFactory knowledgeUnitsFactory;
        private readonly IWorkToolsUnitsFactory workToolsUnitsFactory;
        private readonly ISnippetUnitsFactory snippetUnitsFactory;
        private readonly ICheatSheetUnitsFactory cheatSheetUnitsFactory;
        private readonly IOptionUnitsFactory optionUnitsFactory;

        public AutoDiscoveryUnitsFactory(
            IKnowledgeUnitsFactory knowledgeUnitsFactory,
            IWorkToolsUnitsFactory workToolsUnitsFactory,
            ISnippetUnitsFactory snippetUnitsFactory,
            ICheatSheetUnitsFactory cheatSheetUnitsFactory,
            IOptionUnitsFactory optionUnitsFactory)
            : base("AutoDiscovery", priority: 1000)
        {
            this.knowledgeUnitsFactory = knowledgeUnitsFactory;
            this.workToolsUnitsFactory = workToolsUnitsFactory;
            this.snippetUnitsFactory = snippetUnitsFactory;
            this.cheatSheetUnitsFactory = cheatSheetUnitsFactory;
            this.optionUnitsFactory = optionUnitsFactory;
        }

        public override IEnumerable<IUnit> CreateUnits()
        {
            System.Diagnostics.Debug.WriteLine("AutoDiscoveryUnitsFactory.CreateUnits() - 動的生成系のみ");
            var count = 0;

            // 動的ファイル読み込み系のみ残す
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

            // OptionUnitsFactoryには依存関係があるOptionUnitがある
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
