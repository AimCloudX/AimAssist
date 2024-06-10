using AimAssist.Service;
using AimAssist.UI;

namespace AimAssist.Commands
{
    public static class PickerCommands
    {
        public static RelayCommand ToggleAssistWindowCommand = new RelayCommand((o) =>
        {
            WindowHandleService.ToggleMainWindow();

        })
        {
        };
    }
}

