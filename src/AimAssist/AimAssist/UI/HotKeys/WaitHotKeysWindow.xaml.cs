
using AimAssist.Core.Commands;
using AimAssist.Core.Events;
using AimAssist.Core.Interfaces;
using AimAssist.HotKeys;
using System.Windows;

namespace AimAssist.UI.Tools.HotKeys
{
    public partial class WaitHotKeysWindow : Window
    {
        private HotKeyController hotkeyController;
        private readonly ICommandService _commandService;

        public WaitHotKeysWindow(ICommandService commandService)
        {
            _commandService = commandService;
            this.InitializeComponent();
            this.Visibility = Visibility.Hidden;
            this.ShowInTaskbar = false;

            // HotKeyの登録
            this.hotkeyController = new HotKeyController(this);
            RegisterHotKey(AppCommands.ToggleMainWindow.CommandName);
            RegisterHotKey(AppCommands.ShowPickerWindow.CommandName);

            EventPublisher.KeyUpdateEventPublisher.UpdateKeyGestureEventHandler
                 += KeyGesutureUpdatedEventPublisher_UpdateKeyGestureEventHandler; ;
        }

        private void KeyGesutureUpdatedEventPublisher_UpdateKeyGestureEventHandler(object sender, KeyGestureUpdatedEventArgs e)
        {
            this.hotkeyController.Unregister(e.Before.FirstModifiers, e.Before.FirstKey);
            this.hotkeyController.Register(e.after.FirstModifiers, e.after.FirstKey, e.Command);
        }

        private void RegisterHotKey(string commandName)
        {
            if (_commandService.TryGetKeyGesutre(commandName, out var command, out var keyGesture))
            {
                this.hotkeyController.Register(keyGesture.FirstModifiers,
                                      keyGesture.FirstKey,
                                          command
                                      );
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // HotKeyの登録解除
            this.hotkeyController.Dispose();
        }
    }
}