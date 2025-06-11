using AimAssist.Core.Units;
using System.Windows;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// プラグインサービスのインターフェース
    /// </summary>
    public interface IPluginsService
    {
        /// <summary>
        /// コマンドプラグインをロードします
        /// </summary>
        void LoadCommandPlugins();

        /// <summary>
        /// ユニットファクトリを取得します
        /// </summary>
        /// <returns>ユニットファクトリのコレクション</returns>
        IEnumerable<IUnitsFactory> GetFactories();

        /// <summary>
        /// UIエレメントコンバーターを取得します
        /// </summary>
        /// <returns>UIエレメントコンバーターのコレクション</returns>
        Dictionary<Type, Func<IItem, UIElement>> GetConverters();

        /// <summary>
        /// プラグインが読み込まれているかどうかを取得します
        /// </summary>
        bool IsPluginsLoaded { get; }

        /// <summary>
        /// 読み込まれたプラグインの数を取得します
        /// </summary>
        int PluginsCount { get; }
    }
}
