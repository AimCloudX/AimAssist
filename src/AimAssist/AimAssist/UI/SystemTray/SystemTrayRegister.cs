using AimAssist.Core;
using AimAssist.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AimAssist.UI.SystemTray
{
    /// <summary>
    /// システムトレイの登録と管理を行うクラス
    /// </summary>
    public class SystemTrayRegister
    {
        private readonly IAppCommands _appCommands;
        private NotifyIcon _notifyIcon = null!;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="appCommands">アプリケーションコマンド</param>
        public SystemTrayRegister(IAppCommands appCommands)
        {
            _appCommands = appCommands;
        }

        /// <summary>
        /// システムトレイにアイコンを登録します
        /// </summary>
        public void Register() 
        { 
            var menu = new ContextMenuStrip();
            menu.Items.Add("Show PickerWindow", null, Show_Click);
            menu.Items.Add("Quit AimAssist", null, Exit_Click);
            var icon = App.GetResourceStream(new Uri("Resources/Icons/AimAssist.ico", UriKind.Relative)).Stream;
            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = new Icon(icon),
                Text = Constants.AppName,
                ContextMenuStrip = menu,
            };
            _notifyIcon.MouseClick += new MouseEventHandler(NotifyIcon_Click);
        }

        private void Show_Click(object? sender, EventArgs e)
        {
            _appCommands.ToggleMainWindow.Execute(null);
        }

        private void NotifyIcon_Click(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _appCommands.ToggleMainWindow.Execute(null);
            }
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
