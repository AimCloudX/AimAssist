using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Options
{
    public class OptionUnit : IUnit
    {
        public OptionUnit(string name, IEnumerable<string> optionFilePaths)
        {
            Name = name;
            OptionFilePaths = optionFilePaths;
        }

        public IMode Mode => OptionMode.Instance;

        public string Name { get; }

        public string Description => "";

        public string Category => "";

        public IEnumerable<string> OptionFilePaths { get; }
    }
}
