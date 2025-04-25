using System;
using System.Threading.Tasks;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// ピッカーサービスのインターフェース
    /// </summary>
    public interface IPickerService
    {
        /// <summary>
        /// ピッカーウィンドウを表示する
        /// </summary>
        /// <returns>選択された結果</returns>
        Task<string> ShowPicker();

        /// <summary>
        /// ピッカーウィンドウを表示する（カスタム処理付き）
        /// </summary>
        /// <param name="callback">選択後のコールバック</param>
        /// <returns>実行タスク</returns>
        Task ShowPickerWithCallback(Action<string> callback);

        /// <summary>
        /// ピッカーウィンドウを閉じる
        /// </summary>
        void ClosePicker();
    }
}
