using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace AimAssist.Behaviors
{
    public class PickerWindowKeyboardBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty NavigateUpCommandProperty =
            DependencyProperty.Register(nameof(NavigateUpCommand), typeof(ICommand), typeof(PickerWindowKeyboardBehavior));

        public static readonly DependencyProperty NavigateDownCommandProperty =
            DependencyProperty.Register(nameof(NavigateDownCommand), typeof(ICommand), typeof(PickerWindowKeyboardBehavior));

        public static readonly DependencyProperty ExecuteCommandProperty =
            DependencyProperty.Register(nameof(ExecuteCommand), typeof(ICommand), typeof(PickerWindowKeyboardBehavior));

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(PickerWindowKeyboardBehavior));

        public ICommand NavigateUpCommand
        {
            get => (ICommand)GetValue(NavigateUpCommandProperty);
            set => SetValue(NavigateUpCommandProperty, value);
        }

        public ICommand NavigateDownCommand
        {
            get => (ICommand)GetValue(NavigateDownCommandProperty);
            set => SetValue(NavigateDownCommandProperty, value);
        }

        public ICommand ExecuteCommand
        {
            get => (ICommand)GetValue(ExecuteCommandProperty);
            set => SetValue(ExecuteCommandProperty, value);
        }

        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    ExecuteCommand.Execute(null);
                    break;

                case Key.Escape:
                    e.Handled = true;
                    CloseCommand.Execute(null);
                    break;

                case Key.Up:
                    e.Handled = true;
                    NavigateUpCommand.Execute(null);
                    break;

                case Key.Down:
                    e.Handled = true;
                    NavigateDownCommand.Execute(null);
                    break;
            }
        }
    }
}
