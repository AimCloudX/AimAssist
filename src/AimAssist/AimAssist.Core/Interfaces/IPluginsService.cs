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
        IEnumerable<IUnitsFacotry> GetFactories();

        /// <summary>
        /// UIエレメントコンバーターを取得します
        /// </summary>
        /// <returns>UIエレメントコンバーターのコレクション</returns>
        Dictionary<Type, Func<IUnit, UIElement>> GetConverters();
    }
}
