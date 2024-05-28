using AimPicker.Service;
using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimPicker.Unit.Implementation.WorkFlows
{
    public class WorkFlowUnitsFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => WorkFlowMode.Instance;

        public bool IsShowInStnadard => true;

        public IEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            var combos = UnitService.UnitDictionary[WorkFlowMode.Instance];
            foreach (var combo in combos)
            {
                yield return combo;
            }
        }
    }
}
