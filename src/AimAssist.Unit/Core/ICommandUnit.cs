using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Unit.Core
{
    public interface ICommandUnit :IUnit
    {
        void Execute();
    }
}
