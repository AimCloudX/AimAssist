using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace AimAssist.Behaviors
{
    public class KeyboardNavigationBehavior : Behavior<System.Windows.Controls.TextBox>
    {
        public static readonly DependencyProperty TargetListBoxProperty =
            DependencyProperty.Register(nameof(TargetListBox), typeof(System.Windows.Controls.ListBox), typeof(KeyboardNavigationBehavior));

        public System.Windows.Controls.ListBox TargetListBox
        {
            get => (System.Windows.Controls.ListBox)GetValue(TargetListBoxProperty);
            set => SetValue(TargetListBoxProperty, value);
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
                case Key.Up:
                    NavigateUp();
                    e.Handled = true;
                    break;
                case Key.Down:
                    NavigateDown();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    ClearFilter();
                    e.Handled = true;
                    break;
            }
        }

        private void NavigateUp()
        {
            var index = TargetListBox.SelectedIndex;
            if (index > 0)
            {
                TargetListBox.SelectedIndex = index - 1;
                TargetListBox.ScrollIntoView(TargetListBox.SelectedItem);
            }
        }

        private void NavigateDown()
        {
            var index = TargetListBox.SelectedIndex;
            if (index < TargetListBox.Items.Count - 1)
            {
                TargetListBox.SelectedIndex = index + 1;
                TargetListBox.ScrollIntoView(TargetListBox.SelectedItem);
            }
        }

        private void ClearFilter()
        {
            AssociatedObject.Text = string.Empty;
        }
    }
}
