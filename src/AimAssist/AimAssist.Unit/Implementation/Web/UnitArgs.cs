using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.Web
{
    public class UnitsArgs
    {
        public UnitsArgs(IMode mode, IEnumerable<IUnit> units, bool needSetMode)
        {
            Mode = mode;
            Units = units;
            NeedSetMode = needSetMode;
        }

        public IMode Mode { get; }
        public IEnumerable<IUnit> Units { get; }
        public bool NeedSetMode { get; }
    }
}
