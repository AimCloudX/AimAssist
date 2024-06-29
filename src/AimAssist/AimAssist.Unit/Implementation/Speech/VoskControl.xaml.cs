using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System.Windows.Controls;
using Vosk;

namespace AimAssist.Units.Implementation.Speech
{
    /// <summary>
    /// VoskControl.xaml の相互作用ロジック
    /// </summary>
    public partial class VoskControl : UserControl
    {
        public VoskControl()
        {
            InitializeComponent();
            Task.Run(() => {
                InitializeVosk();
                InitializeMicrophone();
            });
        }

        private WaveInEvent waveIn;
        private Model model;
        private VoskRecognizer recognizer;

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                var result = recognizer.Result();
                Dispatcher.Invoke(() => TextBox.Text += ExtractTextFromJson(result) + "\n");
            }
        }

        private string ExtractTextFromJson(string json)
        {
            var jObject = JObject.Parse(json);
            return jObject["text"].ToString();
        }

        private void InitializeVosk()
        {
            Vosk.Vosk.SetLogLevel(0);
            string modelPath = "Resources/vosk-model-small-ja-0.22"; // モデルのパスを指定
            model = new Vosk.Model(modelPath);
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

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            waveIn.Dispose();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            waveIn.StopRecording();
            model.Dispose();
            recognizer.Dispose();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.TextBox.Text = string.Empty;
        }
    }
}
