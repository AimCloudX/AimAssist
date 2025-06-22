using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;
using System.Windows.Data;

namespace AimAssist.Behaviors
{
    public class DelayedFilterBehavior : Behavior<System.Windows.Controls.TextBox>
    {
        private DispatcherTimer timer = null!;
        private string previousText = string.Empty;


        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register(nameof(Delay), typeof(TimeSpan), typeof(DelayedFilterBehavior),
                new PropertyMetadata(TimeSpan.FromMilliseconds(300)));

        public static readonly DependencyProperty FilterTargetProperty =
            DependencyProperty.Register(nameof(FilterTarget), typeof(System.Windows.Controls.ListBox), typeof(DelayedFilterBehavior));

        public static readonly DependencyProperty FilterMethodProperty =
            DependencyProperty.Register(nameof(FilterMethod), typeof(Predicate<object>), typeof(DelayedFilterBehavior));

        public TimeSpan Delay
        {
            get => (TimeSpan)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        public System.Windows.Controls.ListBox FilterTarget
        {
            get => (System.Windows.Controls.ListBox)GetValue(FilterTargetProperty);
            set => SetValue(FilterTargetProperty, value);
        }

        public Predicate<object> FilterMethod
        {
            get => (Predicate<object>)GetValue(FilterMethodProperty);
            set => SetValue(FilterMethodProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += OnTextChanged;
            InitializeTimer();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.TextChanged -= OnTextChanged;
            timer?.Stop();
            timer = null!;
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = Delay
            };
            timer.Tick += OnTimerTick;
        }

        private void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            timer?.Stop();
            timer?.Start();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            timer?.Stop();

            if (previousText.Equals(AssociatedObject.Text))
                return;

            ApplyFilter();
            previousText = AssociatedObject.Text;

            if (FilterTarget != null)
                FilterTarget.SelectedIndex = 0;
        }

        private void ApplyFilter()
        {
            if (FilterTarget?.ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(FilterTarget.ItemsSource);
            if (view != null)
            {
                view.Filter = FilterMethod;
            }
        }
    }
}
