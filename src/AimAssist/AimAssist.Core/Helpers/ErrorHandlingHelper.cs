using AimAssist.Core.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace AimAssist.Core.Helpers
{
    /// <summary>
    /// エラーハンドリングを支援するヘルパークラス
    /// </summary>
    public static class ErrorHandlingHelper
    {
        private static IApplicationLogService _logService;

        /// <summary>
        /// ログサービスを設定します
        /// </summary>
        /// <param name="logService">ログサービス</param>
        public static void SetLogService(IApplicationLogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// アクションを実行し、例外をキャッチして処理します
        /// </summary>
        /// <param name="action">実行するアクション</param>
        /// <param name="errorMessage">エラーメッセージ</param>
        /// <param name="showMessageBox">エラーメッセージボックスを表示するかどうか</param>
        /// <returns>処理が成功したかどうか</returns>
        public static bool SafeExecute(Action action, string errorMessage = null, bool showMessageBox = false)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex, errorMessage, showMessageBox);
                return false;
            }
        }

        /// <summary>
        /// 非同期アクションを実行し、例外をキャッチして処理します
        /// </summary>
        /// <param name="asyncAction">実行する非同期アクション</param>
        /// <param name="errorMessage">エラーメッセージ</param>
        /// <param name="showMessageBox">エラーメッセージボックスを表示するかどうか</param>
        /// <returns>処理が成功したかどうかを示すTask</returns>
        public static async Task<bool> SafeExecuteAsync(Func<Task> asyncAction, string errorMessage = null, bool showMessageBox = false)
        {
            try
            {
                await asyncAction();
                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex, errorMessage, showMessageBox);
                return false;
            }
        }

        /// <summary>
        /// 戻り値のある関数を実行し、例外をキャッチして処理します
        /// </summary>
        /// <typeparam name="T">戻り値の型</typeparam>
        /// <param name="func">実行する関数</param>
        /// <param name="defaultValue">例外発生時のデフォルト値</param>
        /// <param name="errorMessage">エラーメッセージ</param>
        /// <param name="showMessageBox">エラーメッセージボックスを表示するかどうか</param>
        /// <returns>関数の戻り値、または例外発生時のデフォルト値</returns>
        public static T SafeExecute<T>(Func<T> func, T defaultValue = default, string errorMessage = null, bool showMessageBox = false)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                HandleException(ex, errorMessage, showMessageBox);
                return defaultValue;
            }
        }

        /// <summary>
        /// 戻り値のある非同期関数を実行し、例外をキャッチして処理します
        /// </summary>
        /// <typeparam name="T">戻り値の型</typeparam>
        /// <param name="asyncFunc">実行する非同期関数</param>
        /// <param name="defaultValue">例外発生時のデフォルト値</param>
        /// <param name="errorMessage">エラーメッセージ</param>
        /// <param name="showMessageBox">エラーメッセージボックスを表示するかどうか</param>
        /// <returns>関数の戻り値、または例外発生時のデフォルト値を含むTask</returns>
        public static async Task<T> SafeExecuteAsync<T>(Func<Task<T>> asyncFunc, T defaultValue = default, string errorMessage = null, bool showMessageBox = false)
        {
            try
            {
                return await asyncFunc();
            }
            catch (Exception ex)
            {
                HandleException(ex, errorMessage, showMessageBox);
                return defaultValue;
            }
        }

        /// <summary>
        /// 例外を処理します
        /// </summary>
        /// <param name="ex">発生した例外</param>
        /// <param name="message">エラーメッセージ</param>
        /// <param name="showMessageBox">エラーメッセージボックスを表示するかどうか</param>
        private static void HandleException(Exception ex, string message = null, bool showMessageBox = false)
        {
            string errorMessage = string.IsNullOrEmpty(message)
                ? $"エラーが発生しました: {ex.Message}"
                : $"{message}: {ex.Message}";

            // ログに記録
            _logService?.LogException(ex, message);

            // UIスレッドでメッセージボックスを表示
            if (showMessageBox)
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    MessageBox.Show(
                        errorMessage,
                        "エラー",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
        }
    }
}
