using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Standard;

namespace AimPicker.Service
{
    public class UnitsService
    {
        private static UnitsService? instance;

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

        private IList<IUnitsFacotry> factories = new List<IUnitsFacotry>();

        public void RegisterFactory (IUnitsFacotry factory)
        {
            factories.Add(factory);
        }

        public IEnumerable<IPickerMode> AllMode()
        {
            return this.factories.Select(x=>x.TargetMode).Distinct().ToList();
        }
        public IPickerMode GetModeFromText(string text)
        {
            foreach (var mode in AllMode().Where(x => !string.IsNullOrEmpty(x.Prefix)))
            {
                if (text.StartsWith(mode.Prefix))
                {
                    return mode;
                }
            }

            return StandardMode.Instance;
        }

        public async IAsyncEnumerable<IUnit> CreateUnits(IPickerMode mode, string inputText)
        {
            var paramter = new UnitsFactoryParameter(inputText);
            switch (mode)
            {
                case StandardMode:
                    foreach (var factory in this.factories.Where(x=>x.IsShowInStnadard))
                    {
                        await foreach (var units in factory.GetUnits(paramter))
                        {
                            yield return units;
                        }
                    }

                    break;
                default:
                    foreach (var factory in this.factories.Where(x=>x.TargetMode == mode))
                    {
                        await foreach (var units in factory.GetUnits(paramter))
                        {
                            yield return units;
                        }
                    }
                    break;
            }
        }

        public void Dispose()
        {
            foreach (var disposable in factories.OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            factories.Clear();
        }
    }
}
