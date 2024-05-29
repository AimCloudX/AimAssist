using AimPicker.Combos.Mode.WorkFlows;
using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Web;

namespace AimPicker.Unit.Implementation.WorkFlows
{
    public class ChatGPTUnitsFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => WorkFlowMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new WorkFlowUnit("ChatGPT", "https://chatgpt.com/", (unit) => new WebViewPreviewFactory().Create(unit.Text));
        }
    }
}
