using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AimAssist.Units.Implementation.Speech
{
    public partial class WhisperControl : IDisposable
    {
        private readonly WhisperProcessor whisperProcessor;
        private readonly AudioRecorder audioRecorder;
        private readonly WhisperConfiguration configuration;
        private bool disposed;

        public WhisperControl()
        {
            InitializeComponent();
            
            whisperProcessor = new WhisperProcessor();
            audioRecorder = new AudioRecorder();
            configuration = new WhisperConfiguration { CanRun = true };
            
            DataContext = configuration;
            
            audioRecorder.RecordingStopped += OnRecordingStopped;
            
            InitializeProcessorAsync();
        }

        private void BrowseModelPath_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*",
                Title = "Whisperモデルファイルを選択"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                configuration.ModelPath = openFileDialog.FileName;
            }
        }

        private void BrowseSavePath_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Wave Files (*.wav)|*.wav|All Files (*.*)|*.*",
                Title = "保存先ファイルを選択",
                FileName = "temp.wav"
            };
            
            if (saveFileDialog.ShowDialog() == true)
            {
                PathTextBox.Text = saveFileDialog.FileName;
            }
        }

        private async void BaseModelDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                configuration.CanRun = false;
                await whisperProcessor.DownloadModelAsync(configuration.ModelPath);
                MessageBox.Show("モデルのダウンロードが完了しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "情報", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"モデルのダウンロード中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                configuration.CanRun = true;
            }
        }

        private async void UpdateClick(object sender, RoutedEventArgs e)
        {
            await UpdateProcessorAsync();
        }

        private async Task UpdateProcessorAsync()
        {
            if (configuration.IsRunning)
            {
                audioRecorder.StopRecording();
            }

            configuration.CanRun = false;

            var language = ((ComboBoxItem)LanguageComboBox.SelectedItem)?.Tag?.ToString() ?? "ja";
            var success = await whisperProcessor.InitializeAsync(configuration.ModelPath, language);

            if (!success)
            {
                TextBox.Text = "Whisper Modelが見つかりませんでした。モデルをダウンロードしてください。";
            }
            else
            {
                TextBox.Text = string.Empty;
            }

            configuration.CanRun = true;
        }

        private void StartRecording_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var success = audioRecorder.StartRecording(PathTextBox.Text);
                if (success)
                {
                    configuration.IsRunning = true;
                    configuration.CanRun = false;
                }
                else
                {
                    MessageBox.Show("録音の開始に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"録音の開始中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                configuration.CanRun = true;
            }
        }

        private void StopRecording_Click(object sender, RoutedEventArgs e)
        {
            audioRecorder.StopRecording();
        }

        private async void OnRecordingStopped(object? sender, RecordingStoppedEventArgs e)
        {
            configuration.IsRunning = false;
            configuration.CanRun = true;

            if (!e.Success && !string.IsNullOrEmpty(e.ErrorMessage))
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"録音中にエラーが発生しました: {e.ErrorMessage}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return;
            }

            if (whisperProcessor.IsRunning && File.Exists(PathTextBox.Text))
            {
                await ProcessTranscriptionAsync(PathTextBox.Text);
            }
        }

        private async Task ProcessTranscriptionAsync(string filePath)
        {
            configuration.CanRun = false;

            try
            {
                var result = await whisperProcessor.ProcessAudioAsync(filePath);
                
                Dispatcher.Invoke(() =>
                {
                    TextBox.AppendText(result);
                    TextBox.ScrollToEnd();
                });
            }
            finally
            {
                configuration.CanRun = true;
            }
        }

        private void ClearText_Click(object sender, RoutedEventArgs e)
        {
            TextBox.Clear();
        }

        private async void InitializeProcessorAsync()
        {
            await UpdateProcessorAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    audioRecorder.Dispose();
                    whisperProcessor.Dispose();
                }
                disposed = true;
            }
        }
    }
}
