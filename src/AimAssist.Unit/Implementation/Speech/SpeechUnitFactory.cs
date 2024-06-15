using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.WorkFlows;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Speech
{
    public class SpeechUnitFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => WorkFlowMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new SpeechUnit();
        }
    }

    public class SpeechUnit : IUnit
    {
        public BitmapImage Icon => new BitmapImage();

        public IPickerMode Mode => WorkFlowMode.Instance;

        public string Category => "";

        public string Name => "Real-time Transcription";

        public string Text => "文字起こし";

        public UIElement GetUiElement()
        {
            return new SpeechControl();

        }
    }
}
