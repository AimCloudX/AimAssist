using AimAssist.Core.Interfaces;

namespace AimAssist.Services
{
    public interface IUnitManagementService
    {
        void RefreshUnits();
    }

    public class UnitManagementService(
        IUnitsService unitsService,
        IApplicationLogService logService)
        : IUnitManagementService
    {
        public void RefreshUnits()
        {
            try
            {
                logService.Info("ユニットの再読み込みを開始します");
                unitsService.RefreshUnits();
                logService.Info("ユニットの再読み込みが完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "ユニット再読み込み中にエラーが発生しました");
            }
        }
    }
}
