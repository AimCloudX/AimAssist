using AimPicker.Service;
using AimPicker.UI;

namespace AimPicker.Commands
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

