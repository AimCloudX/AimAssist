﻿using AimAssist.Service;
using AimAssist.UI;

namespace AimAssist.Core.Commands
{
    public class AppCommands
    {
        public static RelayCommand ShutdownAimAssist => new RelayCommand(nameof(CommandNames.ShutdownAimAssist), App.Current.Shutdown);
        public static RelayCommand ToggleMainWindow => new HotkeyCommand(nameof(CommandNames.ToggleMainWindow), WindowHandleService.ToggleMainWindow);
        public static RelayCommand ShowPickerWindow => new HotkeyCommand(nameof(CommandNames.ShowPickerWindow), PickerService.ShowPickerWindow);

    }
}