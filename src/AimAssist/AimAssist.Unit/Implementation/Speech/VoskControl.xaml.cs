using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Vosk;

namespace AimAssist.Units.Implementation.Speech
{
    /// <summary>
    /// VoskControl.xaml の相互作用ロジック
    /// </summary>
    public partial class VoskControl
    {
        public VoskControl(Model? model)
        {
            this.model = model;
            InitializeComponent();
            Task.Run(async () => {
                await InitializeVoskAsync();
                await Dispatcher.InvokeAsync(() => {
                    InitializeMicrophone();
                });
            });
        }

        private WaveInEvent? waveIn;
        private Model? model;
        private VoskRecognizer? recognizer;

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (recognizer != null && recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                var result = recognizer.Result();
                var text = ExtractTextFromJson(result);
                if (!string.IsNullOrEmpty(text))
                {
                    Dispatcher.BeginInvoke(() => TextBox.Text += text + "\n");
                }
            }
        }

        private static string ExtractTextFromJson(string json)
        {
            var jObject = JObject.Parse(json);
            return jObject["text"]?.ToString()?? string.Empty;
        }

        private async Task InitializeVoskAsync()
        {
            Vosk.Vosk.SetLogLevel(0);
            const string modelPath = "Resources/vosk-model-small-ja-0.22";
            model = new Model(modelPath);
            recognizer = new VoskRecognizer(model, 16000.0f);
        }

        private void InitializeMicrophone()
        {
            waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.DataAvailable += OnDataAvailable;
            //waveIn.RecordingStopped += OnRecordingStopped;
            waveIn.StartRecording();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.TextBox.Text = string.Empty;
        }
    }
}
