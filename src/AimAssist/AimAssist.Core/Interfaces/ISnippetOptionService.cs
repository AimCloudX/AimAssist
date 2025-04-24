using AimAssist.Units.Implementation.WorkTools;
using System;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// スニペット設定サービスのインターフェース
    /// </summary>
    public interface ISnippetOptionService
    {
        /// <summary>
        /// スニペット設定
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
        /// <returns>保存が成功したかどうか</returns>
        bool SaveOption();
    }
}
