using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AimAssist.Services
{
    public interface IUnitManagementService
    {
        IEnumerable<IUnit> GetAllUnits();
        IEnumerable<IUnit> GetFilteredUnits(string filter);
        IUnit GetUnitById(string id);
        void RefreshUnits();
    }

    public class UnitManagementService : IUnitManagementService
    {
        private readonly IUnitsService _unitsService;
        private readonly IApplicationLogService _logService;

        public UnitManagementService(
            IUnitsService unitsService,
            IApplicationLogService logService)
        {
            _unitsService = unitsService;
            _logService = logService;
        }

        public IEnumerable<IUnit> GetAllUnits()
        {
            try
            {
                return _unitsService.GetAllUnits();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "全ユニット取得中にエラーが発生しました");
                return Enumerable.Empty<IUnit>();
            }
        }

        public IEnumerable<IUnit> GetFilteredUnits(string filter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filter))
                {
                    return GetAllUnits();
                }

                return GetAllUnits().Where(unit => 
                    unit.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    unit.Description.Contains(filter, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"ユニットフィルタリング中にエラーが発生しました。フィルタ: {filter}");
                return Enumerable.Empty<IUnit>();
            }
        }

        public IUnit GetUnitById(string id)
        {
            try
            {
                return GetAllUnits().FirstOrDefault(unit => unit.Name == id);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"ユニット取得中にエラーが発生しました。ID: {id}");
                return null;
            }
        }

        public void RefreshUnits()
        {
            try
            {
                _logService.Info("ユニットの再読み込みを開始します");
                _unitsService.RefreshUnits();
                _logService.Info("ユニットの再読み込みが完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "ユニット再読み込み中にエラーが発生しました");
            }
        }
    }
}
