﻿using AimAssist.UI;

namespace AimAssist.Core.Commands
{
    public class MainWindowCommands
    {
        public static RelayCommand FocusPreview { get; set; }
        public static RelayCommand FocusFilterTextBox { get; set; }
    }
        public static class ChangeMode
        {
            public static RelayCommand FavoriteMode { get; set; }
            public static RelayCommand AllInclusiveMode { get; set; }
            public static RelayCommand BookSearchMode { get; set; }
            public static RelayCommand KeyboardShortcut { get; set; }
            public static RelayCommand NextMode { get; set; }
            public static RelayCommand PreviousMode { get; set; }
        }
}