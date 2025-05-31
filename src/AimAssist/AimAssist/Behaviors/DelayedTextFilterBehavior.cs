using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TextBox = System.Windows.Controls.TextBox;

namespace AimAssist.Behaviors
{
    public class DelayedTextFilterBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register(nameof(Delay), typeof(TimeSpan), typeof(DelayedTextFilterBehavior),
                new PropertyMetadata(TimeSpan.FromMilliseconds(100)));

        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.Register(nameof(FilterText), typeof(string), typeof(DelayedTextFilterBehavior),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private DispatcherTimer? _timer;
        private string _previousText = string.Empty;

        public TimeSpan Delay
        {
            get => (TimeSpan)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        public string FilterText
        {
            get => (string)GetValue(FilterTextProperty);
            set => SetValue(FilterTextProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += OnTextChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.TextChanged -= OnTextChanged;
            _timer?.Stop();
            _timer = null;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer
                {
                    Interval = Delay
                };
                _timer.Tick += OnTimerTick;
            }

            _timer.Stop();
            _timer.Start();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _timer?.Stop();

            var currentText = AssociatedObject.Text;
            if (_previousText != currentText)
            {
                FilterText = currentText;
                _previousText = currentText;
            }
        }
    }
}
