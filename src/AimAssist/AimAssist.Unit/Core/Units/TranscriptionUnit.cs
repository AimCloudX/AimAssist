using AimAssist.Core.Units;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Core.Units
{
    public class TranscriptionUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;

        public string Name => "Transcription";

        public string Description => "文字起こし";

        public string Category => string.Empty;
    }
}
