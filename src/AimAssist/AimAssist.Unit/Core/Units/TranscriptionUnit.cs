using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Core.Units
{
    [AutoRegisterUnit("Audio", Priority = 100)]
    public class TranscriptionUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;

        public string Name => "Transcription";

        public string Description => "文字起こし";

        public string Category => string.Empty;
    }
}
