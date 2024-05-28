using AimPicker.Unit.Core.Mode;

namespace AimPicker.Unit.Core
{
    public interface IUnitsFacotry
    {
        IPickerMode TargetMode { get; }
        IEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater);
        bool IsShowInStnadard { get; }
    }

    public class UnitsFactoryParameter
    {
        public UnitsFactoryParameter(string inputText) {
            InputText = inputText;
        }

        public string InputText { get; }
    }
}
