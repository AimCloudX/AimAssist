using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Modes;
using AimAssist.Units.Core.Units;

namespace AimAssist.Service
{
    public class UnitsService
    {
        private static UnitsService? instance;

        private Dictionary<IMode, IList<IUnit>> modeDic = new() {
            { AllInclusiveMode.Instance, new List<IUnit>() },
             };

        public IReadOnlyCollection<IMode> GetAllModes()
        {
            return modeDic.Keys.Where(x => x.IsIncludeAllInclusive).ToList();
        }

        public static UnitsService Instnace
        {
            get
            {
                if (instance == null)
                {
                    var factory = new UnitsService();
                    instance = factory;
                }

                return instance;
            }
        }

        public void RegisterUnits(IUnitsFacotry facotry)
        {
            var units = facotry.GetUnits();
            foreach (var unit in units)
            {
                if (modeDic.TryGetValue(AllInclusiveMode.Instance, out var allUnits))
                {
                    if(unit.Mode.IsIncludeAllInclusive)
                    {
                        allUnits.Add(unit);
                    }
                }

                if (modeDic.TryGetValue(unit.Mode, out var unitLists))
                {
                    unitLists.Add(unit);
                }
                else
                {
                    modeDic.Add(unit.Mode, new List<IUnit>() { unit });
                }
            }
        }

        public IEnumerable<IUnit> CreateUnits(IMode mode)
        {
            if(modeDic.TryGetValue(mode, out var units))
            {
                return units;
            }

            return Enumerable.Empty<IUnit>();
        }
    }
}
