using AimAssist.Core.Interfaces;
using System.Windows.Threading;

namespace AimAssist.Middlewares
{
    public interface IErrorHandlingMiddleware
    {
        void RegisterGlobalHandlers();
        void HandleException(Exception exception, string context = "");
        void HandleUnhandledException(Exception exception);
    }

    public class ErrorHandlingMiddleware : IErrorHandlingMiddleware
    {
        private readonly IApplicationLogService logService;

        public ErrorHandlingMiddleware(IApplicationLogService logService)
        {
            this.logService = logService;
        }

        public void RegisterGlobalHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
            }
            
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        public void HandleException(Exception exception, string context = "")
        {
            if (exception == null) return;

            try
            {
                var fullContext = string.IsNullOrEmpty(context) ? "予期しないエラー" : context;
                logService?.LogException(exception, fullContext);

                var errorMessage = @$"{fullContext}

エラー詳細:
{exception.Message}";
                
                if (exception.InnerException != null)
                {
                    errorMessage += @$"

内部エラー:
{exception.InnerException.Message}";
                }

                ShowErrorMessage(errorMessage, "エラー");
            }
            catch (Exception logException)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"エラーハンドリング中に例外が発生: {logException}");
                    ShowErrorMessage(@$"重大なエラーが発生しました。
{exception.Message}", "致命的エラー");
                }
                catch
                {
                    Console.WriteLine($"Fatal error: {exception}");
                }
            }
        }

        public void HandleUnhandledException(Exception exception)
        {
            HandleException(exception, "予期しない致命的エラーが発生しました");
        }

        private void ShowErrorMessage(string message, string title)
        {
            if (string.IsNullOrEmpty(message)) return;

            try
            {
                var current = System.Windows.Application.Current;
                if (current?.Dispatcher != null)
                {
                    if (current.Dispatcher.CheckAccess())
                    {
                        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                }
                else
                {
                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch
            {
                try
                {
                    Console.WriteLine($"{title}: {message}");
                }
                catch
                {
                }
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e?.ExceptionObject is Exception exception)
            {
                HandleUnhandledException(exception);
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e?.Exception != null)
            {
                HandleUnhandledException(e.Exception);
                e.Handled = true;
            }
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e?.Exception != null)
            {
                HandleUnhandledException(e.Exception);
                e.SetObserved();
            }
        }
    }
}
