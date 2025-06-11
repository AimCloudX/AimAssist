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

        void RegisterUnits(IUnitsFactory factory);
        void RegisterFeatures(IFeaturesFactory factory);
        
        IEnumerable<IItem> CreateUnits(IMode mode);

        /// <summary>
        /// ユニットを再読み込みします
        /// </summary>
        void RefreshUnits();

        int GetModeDisplayOrder(IMode mode);
    }
}
