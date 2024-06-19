
using AimAssist.UI;
using System.Windows.Input;

namespace AimAssist.Core.Events
{
    public class EventPublisher
{
    private static KeyGesutureUpdatedEventPublisher keyUpdateEventPublisher;
    public static KeyGesutureUpdatedEventPublisher KeyUpdateEventPublisher => keyUpdateEventPublisher ??= new KeyGesutureUpdatedEventPublisher();
}
    public class KeyGestureUpdatedEventArgs : EventArgs
    {
        public KeyGestureUpdatedEventArgs(RelayCommand command, KeyGesture before,KeyGesture after)
        {
            Command = command;
            Before = before;
            this.after = after;
        }

        public RelayCommand Command { get; }
        public KeyGesture Before { get; }
        public KeyGesture after { get; }
    }

    public class KeyGesutureUpdatedEventPublisher
    {
        // Declare the delegate (if using non-generic pattern).
        public delegate void KeyGesutureUpdatedEventHandler(object sender, KeyGestureUpdatedEventArgs e);

        // Declare the event.
        public event KeyGesutureUpdatedEventHandler UpdateKeyGestureEventHandler;

        // Wrap the event in a protected virtual method
        // to enable derived classes to raise the event.
        public void RaiseEvent(RelayCommand command, KeyGesture before, KeyGesture after)
        {
            // Raise the event in a thread-safe manner using the ?. operator.
            this.UpdateKeyGestureEventHandler?.Invoke(this, new KeyGestureUpdatedEventArgs(command, before, after));
        }
    }
}
