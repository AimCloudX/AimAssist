using AimAssist.Combos.Mode.WorkFlows;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Web;

namespace AimAssist.Unit.Implementation.WorkFlows
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
