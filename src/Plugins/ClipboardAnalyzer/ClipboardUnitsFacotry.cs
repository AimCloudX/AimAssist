using AimAssist.Combos.Mode.WorkFlows;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.WorkFlows;

namespace ClipboardAnalyzer
{
    public class ClipboardUnitsFacotry : IUnitsFacotry
    {
        public IPickerMode TargetMode => WorkFlowMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            var text = System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : string.Empty;
            yield return new WorkFlowUnit("ClipboardAnalyzer", text, (unit) => new ClipboardList());
        }
    }
}
