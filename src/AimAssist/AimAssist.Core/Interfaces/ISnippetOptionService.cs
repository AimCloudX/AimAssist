using System;
using AimAssist.Core.Model;

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
