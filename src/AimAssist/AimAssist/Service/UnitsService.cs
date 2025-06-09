using AimAssist.Core.Attributes;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Modes;
using AimAssist.Units.Core.Units;

namespace AimAssist.Service
{
    /// <summary>
    /// ユニットサービスの実装
    /// </summary>
    public class UnitsService : IUnitsService
    {
        private Dictionary<IMode, IList<IUnit>> modeDic = new() {
            { AllInclusiveMode.Instance, new List<IUnit>() },
        };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UnitsService()
        {
            // DIでインスタンス化されるように、パブリックコンストラクタを提供
        }

        /// <summary>
        /// 利用可能なすべてのモードを取得します
        /// </summary>
        /// <returns>利用可能なモードのコレクション</returns>
        public IReadOnlyCollection<IMode> GetAllModes()
        {
            var modes = modeDic.Keys.Where(x => x.IsIncludeAllInclusive);
            
            return modes
                .OrderBy(mode => GetModeDisplayOrder(mode))
                .ThenBy(mode => mode.Name)
                .ToList();
        }

        public int GetModeDisplayOrder(IMode mode)
        {
            var attribute = mode.GetType().GetCustomAttributes(typeof(ModeDisplayOrderAttribute), false)
                .FirstOrDefault() as ModeDisplayOrderAttribute;
            
            return attribute?.Order ?? 500;
        }

        /// <summary>
        /// ユニットファクトリからユニットを登録します
        /// </summary>
        /// <param name="factory">ユニットファクトリ</param>
        public void RegisterUnits(IUnitsFactory factory)
        {
            var units = factory.GetUnits();
            foreach (var unit in units)
            {
                if (modeDic.TryGetValue(AllInclusiveMode.Instance, out var allUnits))
                {
                    if(unit.Mode.IsIncludeAllInclusive)
                    {
                        allUnits.Add(unit);
                    }
                }

                if (modeDic.TryGetValue(unit.Mode, out var unitLists))
                {
                    unitLists.Add(unit);
                }
                else
                {
                    modeDic.Add(unit.Mode, new List<IUnit>() { unit });
                }
            }
        }

        /// <summary>
        /// 指定したモードのユニットを作成します
        /// </summary>
        /// <param name="mode">モード</param>
        /// <returns>作成されたユニットのコレクション</returns>
        public IEnumerable<IUnit> CreateUnits(IMode mode)
        {
            if(modeDic.TryGetValue(mode, out var units))
            {
                return units;
            }

            return Enumerable.Empty<IUnit>();
        }

        /// <summary>
        /// すべてのユニットを取得します
        /// </summary>
        /// <returns>すべてのユニット</returns>
        public IEnumerable<IUnit> GetAllUnits()
        {
            return modeDic.Values.SelectMany(units => units);
        }

        /// <summary>
        /// ユニットを再読み込みします
        /// </summary>
        public void RefreshUnits()
        {
            modeDic.Clear();
            modeDic.Add(AllInclusiveMode.Instance, new List<IUnit>());
        }
    }
}
