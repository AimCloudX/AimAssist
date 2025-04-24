using AimAssist.Units.Implementation.WorkTools;
using System;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// 作業項目設定サービスのインターフェース
    /// </summary>
    public interface IWorkItemOptionService
    {
        /// <summary>
        /// 作業項目設定
        /// </summary>
        ConfigModel Option { get; }

        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        string OptionPath { get; }

        /// <summary>
        /// 設定をロードする
        /// </summary>
        void LoadOption();

        /// <summary>
        /// 設定を保存する
        /// </summary>
        void SaveOption();
    }
}
