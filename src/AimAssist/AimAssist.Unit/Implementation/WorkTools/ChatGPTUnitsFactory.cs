using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.WorkTools
{
    public class ChatGPTUnitsFactory : IUnitsFacotry
    {
        public IMode TargetMode => WorkToolsMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            yield return new UrlUnit(WorkToolsMode.Instance, "ChatGPT", "https://chatgpt.com/");
            yield return new UrlUnit(WorkToolsMode.Instance,"Claude", "https://claude.ai/");
        }
    }
}
