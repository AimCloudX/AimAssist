using AimAssist.Core.Units;

namespace AimAssist.Core.Interfaces
{
    /// <summary>
    /// ユニットサービスのインターフェース
    /// </summary>
    public interface IUnitsService
    {
        /// <summary>
        /// 利用可能なすべてのモードを取得します
        /// </summary>
        /// <returns>利用可能なモードのコレクション</returns>
        IReadOnlyCollection<IMode> GetAllModes();

        /// <summary>
        /// ユニットファクトリからユニットを登録します
        /// </summary>
        /// <param name="factory">ユニットファクトリ</param>
        void RegisterUnits(IUnitsFactory factory);

        /// <summary>
        /// 指定したモードのユニットを作成します
        /// </summary>
        /// <param name="mode">モード</param>
        /// <returns>作成されたユニットのコレクション</returns>
        IEnumerable<IUnit> CreateUnits(IMode mode);
    }
}