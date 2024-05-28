using AimPicker.Combos.Mode.Snippet;
using AimPicker.Service;
using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;

namespace AimPicker.Unit.Implementation.Snippets
{
    public class SnippetUnitsFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => SnippetMode.Instance;

        public bool IsShowInStnadard => true;

        public IEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                yield return new SnippetUnit("クリップボード", System.Windows.Clipboard.GetText());
            }

            foreach (var unit in UnitService.UnitDictionary[SnippetMode.Instance])
            {
                yield return unit;
            }
        }
    }
}
