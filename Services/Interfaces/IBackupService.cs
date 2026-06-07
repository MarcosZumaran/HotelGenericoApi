using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IBackupService
{
    Task<string> CreateBackupAsync(string type = "Full");
    Task<string> CreateBackupAndCopyAsync(string type = "Full");
    Task<List<BackupHistoryDto>> GetBackupHistoryAsync();
    Task<int> LimpiarBackupsAntiguosAsync(int dias);
    Task<string?> GetBackupFilePathAsync(string fileName);
}
