using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.Speech
{
    public class SpeechUnitFactory : IUnitsFacotry
    {
        public IMode TargetMode => WorkToolsMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            yield return new Unit(WorkToolsMode.Instance, "Transcription", new SpeechModel());
        }
    }
}
