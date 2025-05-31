using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.Web.MindMeister
{
    public class MindMeisterUnit : IUnit
    {
        public MindMeisterUnit(string name, string path)
        {
            Name = name;
            Path = path;
        }
        public string Name { get; }

        public string Path { get; }


        public IMode Mode => MindMeisterMode.Instance;

        public string Description => string.Empty;

        public string Category =>string.Empty;
    }

}
