using System.ComponentModel;

namespace AimAssist.Units.Implementation.Terminal
{
    public enum ShellType
    {
        [Description("PowerShell Core")]
        PowerShell,
        
        [Description("Command Prompt")]
        Cmd,
        
        [Description("Git Bash")]
        GitBash,
        
        [Description("Windows Subsystem for Linux")]
        Wsl
    }
    
    public static class ShellTypeExtensions
    {
        public static string GetExecutablePath(this ShellType shellType)
        {
            return shellType switch
            {
                ShellType.PowerShell => "pwsh.exe",
                ShellType.Cmd => "cmd.exe",
                ShellType.GitBash => @"C:\Program Files\Git\bin\bash.exe",
                ShellType.Wsl => "wsl.exe",
                _ => "pwsh.exe"
            };
        }
        
        public static string GetDisplayName(this ShellType shellType)
        {
            return shellType switch
            {
                ShellType.PowerShell => "PowerShell",
                ShellType.Cmd => "Command Prompt",
                ShellType.GitBash => "Git Bash",
                ShellType.Wsl => "WSL",
                _ => "PowerShell"
            };
        }
        
        public static string[] GetArguments(this ShellType shellType)
        {
            return shellType switch
            {
                ShellType.PowerShell => new string[] { },
                ShellType.Cmd => new string[] { },
                ShellType.GitBash => new string[] { "--login", "-i" },
                ShellType.Wsl => new string[] { },
                _ => new string[] { }
            };
        }
    }
}