using Common.UI.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.UI.PickerWindows
{
    /// <summary>
    /// エディターのキャッシュを管理するクラス
    /// </summary>
    internal class EditorCache
    {
        /// <summary>
        /// エディターインスタンスを取得または設定します
        /// </summary>
        public static MonacoEditor Editor { get; set; }
    }
}
