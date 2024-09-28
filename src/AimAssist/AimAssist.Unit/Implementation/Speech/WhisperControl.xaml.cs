using NAudio.Wave;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Whisper.net;
using Whisper.net.Ggml;

namespace AimAssist.Units.Implementation.Speech
{
    /// <summary>
    /// WhisperControl.xaml の相互作用ロジック
    /// </summary>
    public partial class WhisperControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        private WhisperProcessor? processor;
        private WaveFileWriter? writer;
        private WaveInEvent? waveIn;
        private bool disposed = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        private string _modelPath = "ggml-base.bin";
        public string ModelPath
        {
            get => _modelPath;
            set
            {
                if (_modelPath != value)
                {
                    _modelPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _language = "ja";
        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _canRun;
        public bool CanRun
        {
            get => _canRun;
            set
            {
                if (_canRun != value)
                {
                    _canRun = value;
                    OnPropertyChanged();
                }
            }
        }

        public WhisperControl()
        {
            InitializeComponent();
            this.DataContext = this;
            CanRun = true;
            UpdateProcessorAsync();
        }

        /// <summary>
        /// モデルパス選択ダイアログを開きます
        /// </summary>
        private void BrowseModelPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*",
                Title = "Whisperモデルファイルを選択"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ModelPath = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// 保存先パス選択ダイアログを開きます
        /// </summary>
        private void BrowseSavePath_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
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

        /// <summary>
        /// モデルをダウンロードします
        /// </summary>
        private async void BaseModelDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CanRun = false;
                var tinyPath = ModelPath;
                if (File.Exists(tinyPath))
                {
                    MessageBox.Show("モデルファイルは既に存在します。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                    CanRun = true;
                    return;
                }

                using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
                using var fileWriter = File.OpenWrite(tinyPath);
                await modelStream.CopyToAsync(fileWriter);
                MessageBox.Show("モデルのダウンロードが完了しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"モデルのダウンロード中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CanRun = true;
            }
        }

        /// <summary>
        /// 設定を更新します
        /// </summary>
        private async void UpdateClick(object sender, RoutedEventArgs e)
        {
            await UpdateProcessorAsync();
        }

        /// <summary>
        /// Whisperプロセッサを更新します
        /// </summary>
        private async Task UpdateProcessorAsync()
        {
            if (IsRunning)
            {
                StopRecording();
            }

            CanRun = false;
            OnPropertyChanged(nameof(CanRun));

            var modelPath = ModelPath;
            var language = ((ComboBoxItem)LanguageComboBox.SelectedItem)?.Tag?.ToString() ?? "ja";

            if (!File.Exists(modelPath))
            {
                TextBox.Text = "Whisper Modelが見つかりませんでした。モデルをダウンロードしてください。";
                CanRun = true;
                return;
            }

            try
            {
                var whisperFactory = WhisperFactory.FromPath(modelPath); // Whisperモデルのパスを指定
                processor = whisperFactory.CreateBuilder().WithLanguage(language).Build(); // 言語を指定

                TextBox.Text = string.Empty;
                CanRun = true;
            }
            catch (Exception ex)
            {
                TextBox.Text = $"モデルの読み込み中にエラーが発生しました: {ex.Message}";
            }
        }

        /// <summary>
        /// 録音を開始します
        /// </summary>
        private void StartRecording_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 16, 1)
                };
                waveIn.DataAvailable += OnDataAvailable;
                waveIn.RecordingStopped += OnRecordingStopped;

                writer = new WaveFileWriter(PathTextBox.Text, waveIn.WaveFormat);

                waveIn.StartRecording();
                IsRunning = true;
                CanRun = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"録音の開始中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                CanRun = true;
            }
        }

        /// <summary>
        /// 録音を停止します
        /// </summary>
        private void StopRecording_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        /// <summary>
        /// 録音を停止する処理を共通化
        /// </summary>
        private void StopRecording()
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
            }
        }

        /// <summary>
        /// 録音データが利用可能になったときの処理
        /// </summary>
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (writer == null) return;
            writer.Write(e.Buffer, 0, e.BytesRecorded);
        }

        /// <summary>
        /// 録音が停止されたときの処理
        /// </summary>
        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            try
            {
                waveIn?.Dispose();
                writer?.Dispose();
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"録音の停止中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsRunning = false;
                CanRun = true;
                if (e.Exception != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"録音中にエラーが発生しました: {e.Exception.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                else
                {
                    // 録音停止後に文字起こしを実行
                    if (processor != null && File.Exists(PathTextBox.Text))
                    {
                        WhisperRunAsync(PathTextBox.Text);
                    }
                }
            }
        }

        /// <summary>
        /// Whisperを使って文字起こしを実行します
        /// </summary>
        private async Task WhisperRunAsync(string filePath)
        {
            CanRun = false;
            OnPropertyChanged(nameof(CanRun));

            try
            {
                using var fileStream = File.OpenRead(filePath);

                await foreach (var result in processor!.ProcessAsync(fileStream))
                {
                    Dispatcher.Invoke(() => TextBox.AppendText(result.Text + Environment.NewLine));
                }

                Dispatcher.Invoke(() =>
                {
                    TextBox.ScrollToEnd();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    TextBox.AppendText($"Error: {ex.Message}{Environment.NewLine}");
                });
            }
            finally
            {
                CanRun = true;
                OnPropertyChanged(nameof(CanRun));
            }
        }

        /// <summary>
        /// テキストボックスをクリアします
        /// </summary>
        private void ClearText_Click(object sender, RoutedEventArgs e)
        {
            TextBox.Clear();
        }

        /// <summary>
        /// テキストプロパティの変更を通知します
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// リソースの解放
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposeパターンの実装
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    waveIn?.Dispose();
                    writer?.Dispose();
                    processor?.Dispose();
                }
                disposed = true;
            }
        }

        /// <summary>
        /// ユーザーコントロールが破棄される際にリソースを解放します
        /// </summary>
        ~WhisperControl()
        {
            Dispose(false);
        }
    }
}
