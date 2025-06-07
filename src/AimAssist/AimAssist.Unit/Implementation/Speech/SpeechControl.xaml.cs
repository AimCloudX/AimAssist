using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System.Windows.Controls;
using Vosk;
using AimAssist.Core.Attributes;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Speech
{
    [AutoDataTemplate(typeof(TranscriptionUnit))]
    public partial class SpeechControl : UserControl
    {
        public SpeechControl()
        {
            InitializeComponent();
        }
    }
}
