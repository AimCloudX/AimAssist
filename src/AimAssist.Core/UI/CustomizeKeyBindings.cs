using AimAssist.Core.Commands;
using System.Windows;
using System.Windows.Input;

namespace AimAssist.Core.UI
{
    public class CustomizeKeyBindings : KeyBinding
    {
        public string CommandName
        {
            get => (string)GetValue(CommandNameProperty);
            set => SetValue(CommandNameProperty, value);
        }

        public static readonly DependencyProperty CommandNameProperty =
            DependencyProperty.Register(nameof(CommandName), typeof(string), typeof(CustomizeKeyBindings),
                    new PropertyMetadata(null));

        public sealed override InputGesture Gesture
        {
            get
            {
                if (CommandService.TryGetKeyGesutre(CommandName, out var command, out var keyGesture))
                {
                    return keyGesture;

                }
                else
                {
                    return null;
                }
            }
        }
    }
}
