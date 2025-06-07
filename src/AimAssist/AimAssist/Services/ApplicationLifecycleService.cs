using AimAssist.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace AimAssist.Services
{
    public interface IApplicationLifecycleService
    {
        Task StartupAsync();
        Task ShutdownAsync();
        event EventHandler<ApplicationStateChangedEventArgs> StateChanged;
    }

    public class ApplicationStateChangedEventArgs : EventArgs
    {
        public ApplicationState NewState { get; }
        public ApplicationState PreviousState { get; }

        public ApplicationStateChangedEventArgs(ApplicationState newState, ApplicationState previousState)
        {
            NewState = newState;
            PreviousState = previousState;
        }
    }

    public enum ApplicationState
    {
        NotStarted,
        Starting,
        Running,
        Stopping,
        Stopped,
        Error
    }

    public class ApplicationLifecycleService : IApplicationLifecycleService
    {
        private readonly IApplicationLogService _logService;
        private ApplicationState _currentState = ApplicationState.NotStarted;

        public event EventHandler<ApplicationStateChangedEventArgs> StateChanged;

        public ApplicationLifecycleService(IApplicationLogService logService)
        {
            _logService = logService;
        }

        public async Task StartupAsync()
        {
            try
            {
                ChangeState(ApplicationState.Starting);
                _logService.Info("アプリケーションの起動を開始します");

                await Task.Delay(10);

                ChangeState(ApplicationState.Running);
                _logService.Info("アプリケーションが正常に起動しました");
            }
            catch (Exception ex)
            {
                ChangeState(ApplicationState.Error);
                _logService.LogException(ex, "アプリケーション起動中にエラーが発生しました");
                throw;
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                ChangeState(ApplicationState.Stopping);
                _logService.Info("アプリケーションの停止を開始します");

                ChangeState(ApplicationState.Stopped);
                _logService.Info("アプリケーションが正常に停止しました");
            }
            catch (Exception ex)
            {
                ChangeState(ApplicationState.Error);
                _logService.LogException(ex, "アプリケーション停止中にエラーが発生しました");
                throw;
            }
        }

        private void ChangeState(ApplicationState newState)
        {
            var previousState = _currentState;
            _currentState = newState;
            StateChanged?.Invoke(this, new ApplicationStateChangedEventArgs(newState, previousState));
        }
    }
}
