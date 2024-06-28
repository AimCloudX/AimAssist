using AimAssist.Service;
using Common.Commands;

namespace AimAssist.Core.Commands
{
    public class AppCommands
    {
        public static RelayCommand ShutdownAimAssist => new RelayCommand(nameof(ShutdownAimAssist), App.Current.Shutdown);
        public static RelayCommand ToggleMainWindow => new HotkeyCommand(nameof(ToggleMainWindow), WindowHandleService.ToggleMainWindow);
        public static RelayCommand ShowPickerWindow => new HotkeyCommand(nameof(ShowPickerWindow), PickerService.ShowPickerWindow);

    }
}
