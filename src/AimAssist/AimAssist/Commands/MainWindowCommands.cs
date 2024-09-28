using Common.Commands;

namespace AimAssist.Core.Commands
{
    public class MainWindowCommands
    {
        public static RelayCommand FocusPreview { get; set; }
        public static RelayCommand FocusFilterTextBox { get; set; }
        public static RelayCommand NextMode { get; set; }
        public static RelayCommand PreviousMode { get; set; }
        public static RelayCommand NextUnit { get; set; }
        public static RelayCommand PreviousUnit { get; set; }
        public static RelayCommand FocusContent { get; set; }
        public static RelayCommand FocusUnits { get; set; }
    }
}
