using AimAssist.Service;
using AimAssist.UI;

namespace AimAssist.Commands
{
    public static class AimAssistCommands
    {
        public static RelayCommand ToggleAssistWindowCommand = new(WindowHandleService.ToggleMainWindow) { };
        public static RelayCommand ShowPickerWIndowCommand = new(PickerService.ShowPickerWindow) { };
    }
}

