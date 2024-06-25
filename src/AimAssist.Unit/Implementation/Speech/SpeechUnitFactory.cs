using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.WorkTools;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Speech
{
    public class SpeechUnitFactory : IUnitsFacotry
    {
        public IMode TargetMode => WorkToolsMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new SpeechUnit();
        }
    }

    public class SpeechUnit : IUnit
    {
        public BitmapImage Icon => new BitmapImage();

        public IMode Mode => WorkToolsMode.Instance;

        public string Category => "";

        public string Name => "Transcription";

        public string Text => "文字起こし";

        public UIElement GetUiElement()
        {
            return new SpeechControl();

        }
    }
}
