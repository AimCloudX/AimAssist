using System.IO;

namespace AimAssist.Units.Implementation.Computer.Helpers
{
    public static class DiskInfoHelper
    {
        public static string GetDiskInfo()
        {
            string diskInfo = string.Empty;
            
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                
                foreach (DriveInfo drive in allDrives)
                {
                    if (drive.IsReady)
                    {
                        double totalSizeGb = drive.TotalSize / (1024.0 * 1024 * 1024);
                        double freeSpaceGb = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                        double usedSpaceGb = totalSizeGb - freeSpaceGb;
                        
                        diskInfo += $"ドライブ {drive.Name} ({drive.DriveType}): 総容量 {totalSizeGb:F2} GB, 使用済み {usedSpaceGb:F2} GB, 空き {freeSpaceGb:F2} GB\n";
                    }
                }
            }
            catch
            {
                diskInfo = "情報を取得できませんでした";
            }
            
            return diskInfo;
        }
    }
}
