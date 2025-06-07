using System.IO;
using Whisper.net;
using Whisper.net.Ggml;

namespace AimAssist.Units.Implementation.Speech
{
    public class WhisperAudioProcessor : IDisposable
    {
        private WhisperProcessor? processor;
        private bool disposed;

        public Task<bool> InitializeAsync(string modelPath, string language)
        {
            if (IsRunning || disposed)
                return Task.FromResult(false);

            try
            {
                DisposeProcessor();

                if (!File.Exists(modelPath))
                    return Task.FromResult(false);

                var whisperFactory = WhisperFactory.FromPath(modelPath);
                processor = whisperFactory.CreateBuilder().WithLanguage(language).Build();
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public async Task<string> ProcessAudioAsync(string audioFilePath)
        {
            if (processor == null || !File.Exists(audioFilePath))
                return "エラー: プロセッサまたは音声ファイルが見つかりません。";

            try
            {
                var resultText = string.Empty;
                using var fileStream = File.OpenRead(audioFilePath);

                await foreach (var result in processor.ProcessAsync(fileStream))
                {
                    resultText += result.Text + Environment.NewLine;
                }

                return resultText;
            }
            catch (Exception ex)
            {
                return $"エラー: {ex.Message}";
            }
        }

        public async Task DownloadModelAsync(string modelPath)
        {
            if (File.Exists(modelPath))
                throw new InvalidOperationException("モデルファイルは既に存在します。");

            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
            using var fileWriter = File.OpenWrite(modelPath);
            await modelStream.CopyToAsync(fileWriter);
        }

        public bool IsRunning => processor != null;

        private void DisposeProcessor()
        {
            processor?.Dispose();
            processor = null;
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
                    DisposeProcessor();
                }
                disposed = true;
            }
        }
    }
}
