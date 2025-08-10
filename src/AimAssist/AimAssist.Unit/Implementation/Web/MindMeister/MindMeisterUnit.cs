using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Web.MindMeister
{
    [AutoRegisterUnit("Web", Priority = 75)]
    public class MindMeisterUnit : IUnit
    {
        public MindMeisterUnit() : this("最近開いたMap", "https://www.mindmeister.com/app/maps/recent")
        {
        }

        public MindMeisterUnit(string name, string path)
        {
            Name = name;
            Path = path;
        }
        
        public string Name { get; }

        public string Path { get; }

        public IMode Mode => MindMeisterMode.Instance;

        public string Description => string.Empty;

        public string Category => "MindMeister";
    }
}
