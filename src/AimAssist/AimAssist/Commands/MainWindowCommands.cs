namespace AimAssist.Commands
{
    public class MainWindowCommands
    {
        public static Common.UI.Commands.RelayCommand FocusPreview { get; set; } = null!;
        public static Common.UI.Commands.RelayCommand FocusFilterTextBox { get; set; } = null!;
        public static Common.UI.Commands.RelayCommand NextMode { get; set; } = null!;
        public static Common.UI.Commands.RelayCommand PreviousMode { get; set; } = null!;
        public static Common.UI.Commands.RelayCommand NextUnit { get; set; } = null!;
        public static Common.UI.Commands.RelayCommand PreviousUnit { get; set; } = null!;
        public static Common.UI.Commands.RelayCommand FocusContent { get; set; } = null!;
        public static Common.UI.Commands.RelayCommand FocusUnits { get; set; } = null!;
    }
}
