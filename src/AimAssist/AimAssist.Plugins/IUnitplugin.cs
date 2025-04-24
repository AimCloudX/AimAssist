using AimAssist.Core.Units;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Units;
using System.Windows;

namespace AimAssist.Plugins
{
    /// <summary>
    /// ユニットプラグインのインターフェース
    /// </summary>
    public interface IUnitPlugin
    {
        /// <summary>
        /// ユニットファクトリーを取得します
        /// </summary>
        /// <returns>ユニットファクトリーのコレクション</returns>
        IEnumerable<IUnitsFactory> GetUnitsFactory();

        /// <summary>
        /// UIエレメントコンバーターを取得します
        /// </summary>
        /// <returns>UIエレメントコンバーターのディクショナリ</returns>
        Dictionary<Type, Func<IUnit, UIElement>> GetUIElementConverters();
    }
}
