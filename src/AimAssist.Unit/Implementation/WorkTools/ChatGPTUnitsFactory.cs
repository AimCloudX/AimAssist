using AimAssist.Combos.Mode.WorkTools;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Web;

namespace AimAssist.Unit.Implementation.WorkTools
{
    public class ChatGPTUnitsFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => WorkToolsMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new WorkToolUnit("ChatGPT", "https://chatgpt.com/", (unit) => new WebViewPreviewFactory().Create(unit.Text));
        }
    }
}
