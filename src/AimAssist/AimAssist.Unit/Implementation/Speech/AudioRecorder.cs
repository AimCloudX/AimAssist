using NAudio.Wave;
using System;
using System.IO;

namespace AimAssist.Units.Implementation.Speech
{
    public class AudioRecorder : IDisposable
    {
        private WaveFileWriter? writer;
        private WaveInEvent? waveIn;
        private bool disposed = false;

        public event EventHandler<RecordingStoppedEventArgs>? RecordingStopped;

        public bool IsRecording { get; private set; }

        public bool StartRecording(string outputPath)
        {
            if (IsRecording || disposed)
                return false;

            try
            {
                waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 16, 1)
                };
                
                waveIn.DataAvailable += OnDataAvailable;
                waveIn.RecordingStopped += OnRecordingStopped;

                writer = new WaveFileWriter(outputPath, waveIn.WaveFormat);

                waveIn.StartRecording();
                IsRecording = true;
                return true;
            }
            catch (Exception ex)
            {
                CleanupRecording();
                return false;
            }
        }

        public void StopRecording()
        {
            if (waveIn != null && IsRecording)
            {
                waveIn.StopRecording();
            }
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (writer == null) return;
            writer.Write(e.Buffer, 0, e.BytesRecorded);
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            var exception = e.Exception;
            CleanupRecording();
            IsRecording = false;

            RecordingStopped?.Invoke(this, new RecordingStoppedEventArgs(exception == null, exception?.Message));
        }

        private void CleanupRecording()
        {
            try
            {
                waveIn?.Dispose();
                writer?.Dispose();
            }
            catch
            {
                // 無視
            }
            finally
            {
                waveIn = null;
                writer = null;
            }
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
                    CleanupRecording();
                }
                disposed = true;
            }
        }
    }

    public class RecordingStoppedEventArgs : EventArgs
    {
        public bool Success { get; }
        public string? ErrorMessage { get; }

        public RecordingStoppedEventArgs(bool success, string? errorMessage = null)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
}
