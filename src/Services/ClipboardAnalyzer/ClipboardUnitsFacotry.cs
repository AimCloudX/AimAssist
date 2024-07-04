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
            yield return new ClipboardUnit();
        }
    }

    public class ClipboardUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;

        public string Name => "ClipboardAnalyzer";

        public string Description => System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : string.Empty;

        public string Category => string.Empty;
    }
}
