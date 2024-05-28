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

            foreach (var combo in UnitService.UnitDictionary[SnippetMode.Instance])
            {
                if (combo is SnippetUnit snippet)
                {
                    yield return new SnippetUnit(snippet.Name, snippet.Text);
                }
            }
        }
    }
}
