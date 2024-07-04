using AimAssist.Units.Core.Mode;

namespace AimAssist.UI.MainWindows
{
    internal static class ModeExtensions
    {
        public static string GetModeChnageCommandName(this IMode mode)
        {
            return mode.Name + ".ChangeMode";
        }
    }
}
