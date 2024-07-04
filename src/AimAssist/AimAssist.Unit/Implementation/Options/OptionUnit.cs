using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Options
{
    public class OptionUnit : IUnit
    {
        public IMode Mode => OptionMode.Instance;

        public string Name => "Option";

        public string Description => "";

        public string Category => "";
    }
}
