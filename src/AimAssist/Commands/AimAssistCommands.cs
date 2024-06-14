using AimAssist.Service;
using AimAssist.UI;

namespace AimAssist.Commands
{
    public static class AimAssistCommands
    {
        public static RelayCommand ToggleAssistWindowCommand = new(WindowHandleService.ToggleMainWindow) { };
    }
}

