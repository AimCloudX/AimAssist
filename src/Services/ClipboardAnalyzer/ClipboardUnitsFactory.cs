using AimAssist.Core.Units;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace ClipboardAnalyzer
{
    public class ClipboardUnitsFactory : IUnitsFactory
    {
        public IEnumerable<IUnit> GetUnits()
        {
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
