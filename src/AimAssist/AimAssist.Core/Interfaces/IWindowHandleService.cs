using System;
using System.Collections.Generic;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// ウィンドウハンドル操作サービスのインターフェース
    /// </summary>
    public interface IWindowHandleService
    {
        /// <summary>
        /// アクティブウィンドウのプロセスIDを取得
        /// </summary>
        /// <returns>プロセスID</returns>
        int GetActiveProcessId();

        /// <summary>
        /// アクティブウィンドウのプロセス名を取得
        /// </summary>
        /// <returns>プロセス名</returns>
        string GetActiveProcessName();

        /// <summary>
        /// アクティブウィンドウのタイトルを取得
        /// </summary>
        /// <returns>ウィンドウタイトル</returns>
        string GetActiveWindowTitle();

        /// <summary>
        /// 指定したプロセス名のウィンドウを取得
        /// </summary>
        /// <param name="processName">プロセス名</param>
        /// <returns>ウィンドウハンドル一覧</returns>
        IEnumerable<IntPtr> GetProcessWindows(string processName);

        /// <summary>
        /// ウィンドウをアクティブにする
        /// </summary>
        /// <param name="hwnd">ウィンドウハンドル</param>
        void ActivateWindow(IntPtr hwnd);

        void ToggleMainWindow();

    }
}
