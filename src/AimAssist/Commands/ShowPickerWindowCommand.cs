using AimAssist.Service;
using AimAssist.UI;

namespace AimAssist.Commands
{
    public static class PickerCommands
    {
        private static bool isPickerServiceActivated;
        public static RelayCommand ShowWindowCommand = new RelayCommand((o) =>
        {
            if (isPickerServiceActivated)
            {
                return;
            }

            isPickerServiceActivated = true;

            PickerService.Run();

            isPickerServiceActivated = false;
        })
        {
        };
    }
}

