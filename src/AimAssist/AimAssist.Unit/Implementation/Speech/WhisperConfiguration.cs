using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AimAssist.Units.Implementation.Speech
{
    public class WhisperConfiguration : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string modelPath = "ggml-base.bin";
        public string ModelPath
        {
            get => modelPath;
            set
            {
                if (modelPath != value)
                {
                    modelPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SpeechLanguage = "ja";

        private bool isRunning;
        public bool IsRunning
        {
            get => isRunning;
            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool canRun;
        public bool CanRun
        {
            get => canRun;
            set
            {
                if (canRun != value)
                {
                    canRun = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
