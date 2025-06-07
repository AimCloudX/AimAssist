using Common.UI.Commands;
using Common.UI.Commands.Shortcus;

namespace AimAssist.Core.Events
{
    public class EventPublisher
{
    private static KeyGesutureUpdatedEventPublisher? keyUpdateEventPublisher;
    public static KeyGesutureUpdatedEventPublisher KeyUpdateEventPublisher => keyUpdateEventPublisher ??= new KeyGesutureUpdatedEventPublisher();
}
    public class KeyGestureUpdatedEventArgs : EventArgs
    {
        public KeyGestureUpdatedEventArgs(RelayCommand? command, KeySequence before,KeySequence? after)
        {
            Command = command;
            Before = before;
            this.After = after;
        }

        public RelayCommand? Command { get; }
        public KeySequence Before { get; }
        public KeySequence? After { get; }
    }

    public class KeyGesutureUpdatedEventPublisher
    {
        // Declare the delegate (if using non-generic pattern).
        public delegate void KeyGesutureUpdatedEventHandler(object sender, KeyGestureUpdatedEventArgs e);

        // Declare the event.
        public event KeyGesutureUpdatedEventHandler? UpdateKeyGestureEventHandler;

        // Wrap the event in a protected virtual method
        // to enable derived classes to raise the event.
        public void RaiseEvent(RelayCommand? command, KeySequence before, KeySequence? after)
        {
            // Raise the event in a thread-safe manner using the ?. operator.
            this.UpdateKeyGestureEventHandler?.Invoke(this, new KeyGestureUpdatedEventArgs(command, before, after));
        }
    }
}
