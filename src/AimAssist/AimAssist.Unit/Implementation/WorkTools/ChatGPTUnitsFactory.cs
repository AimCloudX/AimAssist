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
            yield return new Unit(WorkToolsMode.Instance,"ChatGPT", new UrlPath("https://chatgpt.com/"));
            yield return new Unit(WorkToolsMode.Instance,"Claude", new UrlPath("https://claude.ai/"));
        }
    }
}
