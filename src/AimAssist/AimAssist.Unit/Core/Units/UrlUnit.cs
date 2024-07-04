using AimAssist.Units.Core.Mode;

namespace AimAssist.Units.Core.Units
{
    public class UrlUnit(IMode mode, string name, string url, string category ="" ) : IUnit
    {
        public string Url { get; } = url;

        public IMode Mode { get; } = mode;

        public string Name { get; } = name;

        public string Description => this.Url;

        public string Category { get; } = category;
    }
}
