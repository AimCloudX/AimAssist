using System.Windows;

namespace AimAssist.Core.Interfaces
{
    public interface IMainWindow
    {
        void Show();
        void Hide();
        void Close();
        bool? ShowDialog();
        bool Focus();
        WindowState WindowState { get; set; }
        void FocusContent();
        void FocusFilterTextBox();
        event EventHandler Closed;
        Visibility Visibility { get; set; }
    }
}
