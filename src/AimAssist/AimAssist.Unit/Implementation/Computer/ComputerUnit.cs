using AimAssist.Core.Units;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.WorkTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.Computer
{
    public class ComputerUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;

        public string Name => "PC情報";

        public string Description => "PC情報";

        public string Category => string.Empty;
    }
}
