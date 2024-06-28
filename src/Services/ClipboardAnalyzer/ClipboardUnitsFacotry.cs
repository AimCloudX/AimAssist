using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace ClipboardAnalyzer
{
    public class ClipboardUnitsFacotry : IUnitsFacotry
    {
        public IMode TargetMode => WorkToolsMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            var text = System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : string.Empty;
            yield return new Unit(WorkToolsMode.Instance,"ClipboardAnalyzer", text, new ClipboardItem());
        }
    }

    public class ClipboardItem : IUnitContent
    {
    }
}
