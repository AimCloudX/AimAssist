using AimAssist.Combos.Mode.WorkTools;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.WorkTools;

namespace ClipboardAnalyzer
{
    public class ClipboardUnitsFacotry : IUnitsFacotry
    {
        public IMode TargetMode => WorkToolsMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            var text = System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : string.Empty;
            yield return new WorkToolUnit("ClipboardAnalyzer", text, (unit) => new ClipboardList());
        }
    }
}
