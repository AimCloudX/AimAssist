using NAudio.Wave;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Whisper.net;
using Whisper.net.Ggml;

namespace AimAssist.Unit.Implementation.Speech
{
    /// <summary>
    /// WhisperControl.xaml の相互作用ロジック
    /// </summary>
    public partial class WhisperControl : UserControl, INotifyPropertyChanged
    {
        private WhisperProcessor processor;
        private WaveFileWriter writer;
        private WaveInEvent waveIn;

        public event PropertyChangedEventHandler? PropertyChanged;

        public WhisperControl()
        {
            InitializeComponent();
            this.DataContext = this;
            UpdateProcessor();
        }

        private async void UpdateProcessor()
        {
            if (this.IsRunning)
            {
                this.waveIn.StopRecording();
            }


            this.CanRun = false;
            this.OnPropertyChanged(nameof(this.CanRun));
            var modelPath = ModelPath.Text;
            var language = Language.Text;



            Task.Run(() =>
            {
                if (File.Exists(modelPath))
                {
                    var whisperFactory = WhisperFactory.FromPath(modelPath); // Whisperモデルのパスを指定
                    processor = whisperFactory.CreateBuilder().WithLanguage(language).Build(); // 日本語を指定
                    Dispatcher.Invoke(() =>
                    {
                        this.TextBox.Text = string.Empty;
                        this.CanRun = true;
                        this.OnPropertyChanged(nameof(this.CanRun));
                    });
                }
                else
                {

                    Dispatcher.Invoke(() =>
                    {
                        this.TextBox.Text = "Whisper Modelが見つかりませんでした";
                    });
                }
            });
        }

        public bool IsRunning { get; set; }
        public bool CanRun { get; set; }

        private async void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (writer == null) return;
            writer.Write(e.Buffer, 0, e.BytesRecorded);
            writer.Flush();
        }
        private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            try
            {
                if (waveIn != null)
                {
                    waveIn.Dispose();
                    waveIn = null;
                }

                if (writer != null)
                {
                    writer.Dispose();
                    writer = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"録音の停止中にエラーが発生しました: {ex.Message}");
            }
        }

        private async Task WhisperRun(string filePath)
        {
            this.CanRun = false;
            this.OnPropertyChanged(nameof(this.CanRun));

            try
            {
                using var fileStream = File.OpenRead(filePath);

                await foreach (var result in processor.ProcessAsync(fileStream))
                {
                    Dispatcher.Invoke(() => TextBox.Text += result.Text + "\n");
                }

                this.CanRun = true;
                this.OnPropertyChanged(nameof(this.CanRun));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    TextBox.Text += $"Error: {ex.Message}\n";
                    this.CanRun = true;
                    this.OnPropertyChanged(nameof(this.CanRun));
                });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            waveIn.StopRecording();
            processor.Dispose();
        }

        private void Storp_Recording(object sender, RoutedEventArgs e)
        {
            waveIn.StopRecording();
            waveIn.Dispose();
            writer.Dispose();

            IsRunning = false;
            this.OnPropertyChanged(nameof(this.IsRunning));

            WhisperRun(PathTextBox.Text);
        }

        private void Start_Recording(object sender, RoutedEventArgs e)
        {
            waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(16000, 16, 1);

            waveIn.DataAvailable += OnDataAvailable;

            writer = new WaveFileWriter(PathTextBox.Text, waveIn.WaveFormat);

            waveIn.StartRecording();
            IsRunning = true;
            CanRun = false;
            this.OnPropertyChanged(nameof(this.IsRunning));
            this.OnPropertyChanged(nameof(this.CanRun));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.TextBox.Text = string.Empty;
        }

        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            this.UpdateProcessor();
        }

        private async void BaseModelDownload(object sender, RoutedEventArgs e)
        {
            var  tinyPath = "ggml-base.bin";
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
            using var fileWriter = File.OpenWrite(tinyPath);
            await modelStream.CopyToAsync(fileWriter);
        }
    }
}
