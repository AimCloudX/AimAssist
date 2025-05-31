using System.Windows.Input;

namespace AimAssist.Units.Implementation.CodeGenarator
{
    public static class Commands
    {
        public static ICommand AddFileCommand { get; set; } = new RoutedCommand(nameof(AddFileCommand), typeof(CodeGeneratorControl));
    }
}
