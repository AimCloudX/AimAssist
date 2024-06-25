using AimAssist.UI;
using Common;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AimAssist.Unit.Core.Mode
{
    public abstract class ModeBase : IMode
    {
        protected ModeBase(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string ImplementationClassName => GetType().Name;

        public virtual RelayCommand ModeChangeCommand { get; private set; }

        public virtual KeySequence DefaultKeySequence => KeySequence.None;

        public virtual string Description => string.Empty;

        public virtual bool IsApplyFiter => true;

        public abstract Control Icon { get; }

        public virtual void SetModeChangeCommandAction(Action action)
        {
            Debug.Assert(this.ModeChangeCommand == null, "Command登録済み");
            var commandName = $"{GetImplementationClassName()}.ChangeMode";
            this.ModeChangeCommand = new RelayCommand(commandName, action);
        }

        public override bool Equals(object? obj)
        {
            if (obj is IMode pickerMode)
            {
                return Name.Equals(pickerMode.Name, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected string GetImplementationClassName()
        {
            return this.GetType().Name;
        }


        protected PackIcon CreateIcon(PackIconKind kind)
        {
            var packIcon = new PackIcon();
            packIcon.Kind = kind;
            packIcon.Width = 30;
            packIcon.Height = 30;

            var toolTip = new CustomToolTip
            {
                Content = this.Name,
                IsOpen = false,
                StaysOpen = true,
                Placement = System.Windows.Controls.Primitives.PlacementMode.Right,
                Margin = new Thickness(10,0,0,0),
            };

            //// ToolTipの表示を制御するカスタムロジック
            ToolTipService.SetShowDuration(packIcon, 3000);
            ToolTipService.SetInitialShowDelay(packIcon, 0);
            packIcon.ToolTip = toolTip;

            return packIcon;

        }
    }

    public class CustomToolTip : ToolTip
    {
        public CustomToolTip()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (PlacementTarget is FrameworkElement target)
            {
                target.LayoutUpdated += OnTargetLayoutUpdated;
            }
        }

        private void OnTargetLayoutUpdated(object sender, EventArgs e)
        {
            if (IsOpen && PlacementTarget is FrameworkElement target)
            {
                UpdatePosition(target);
            }
        }

        private void UpdatePosition(FrameworkElement target)
        {
            var targetPoint = target.PointToScreen(new Point(target.ActualWidth, 0));
            var popupRoot = this.GetVisualAncestor<PackIcon>();
            if (popupRoot != null)
            {
                var transformToPopup = target.TransformToVisual(popupRoot);
                var targetPositionInPopup = transformToPopup.Transform(new Point(target.ActualWidth, 0));

                // ツールチップを右側に配置
                HorizontalOffset = 5; // 右側の余白
                VerticalOffset = -this.ActualHeight / 2 + target.ActualHeight / 2; // 垂直中央に配置

                // 画面端でのはみ出しを防ぐ
                var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(Window.GetWindow(this)).Handle);
                if (targetPoint.X + this.ActualWidth > screen.WorkingArea.Right)
                {
                    // 右側がはみ出る場合は左側に配置
                    HorizontalOffset = -this.ActualWidth - 5;
                }
            }
        }

    }

    public static class VisualTreeHelperExtensions
    {
        public static T GetVisualAncestor<T>(this DependencyObject element) where T : DependencyObject
        {
            while (element != null && !(element is T))
            {
                element = VisualTreeHelper.GetParent(element);
            }
            return element as T;
        }

        // 他の便利な拡張メソッドもここに追加できます
        public static T GetVisualDescendant<T>(this DependencyObject element) where T : DependencyObject
        {
            if (element == null)
                return null;

            if (element is T)
                return (T)element;

            T foundElement = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(element);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                foundElement = GetVisualDescendant<T>(child);
                if (foundElement != null)
                    break;
            }

            return foundElement;
        }
    }
}
