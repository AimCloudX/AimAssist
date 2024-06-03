using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Core
{
    public interface IUnitsFacotry
    {
        IPickerMode TargetMode { get; }
        IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater);
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
