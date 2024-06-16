using AimAssist.Core.Commands;
using AimAssist.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Service
{
    internal static class CommandService
    {
        public static void Initialize()
        {
            AppCommands.AimAssistShutdown = new RelayCommand(App.Current.Shutdown);
        }
    }
}
