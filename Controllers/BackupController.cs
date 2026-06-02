using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Administrador")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;

    public BackupController(IBackupService backupService) => _backupService = backupService;

    [HttpPost("full")]
    public async Task<IActionResult> CreateFullBackup()
    {
        try
        {
            var filePath = await _backupService.CreateBackupAsync("Full");
            await Task.Delay(500);
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            return File(bytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = $"Error al crear el backup: {ex.Message}" });
        }
    }

    [HttpPost("differential")]
    public async Task<IActionResult> CreateDifferentialBackup()
    {
        try
        {
            var filePath = await _backupService.CreateBackupAsync("Differential");
            await Task.Delay(500);
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            return File(bytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = $"Error al crear el backup diferencial: {ex.Message}" });
        }
    }

    [HttpPost("log")]
    public async Task<IActionResult> CreateLogBackup()
    {
        try
        {
            var filePath = await _backupService.CreateBackupAsync("Log");
            await Task.Delay(500);
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            return File(bytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = $"Error al crear el backup de log: {ex.Message}" });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetBackupHistory()
    {
        try
        {
            var history = await _backupService.GetBackupHistoryAsync();
            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = $"Error al obtener el historial: {ex.Message}" });
        }
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadBackup(string fileName, [FromQuery] string? originalPath = null)
    {
        try
        {
            // Intentar con la ruta original (si se proporciona y el archivo existe)
            if (!string.IsNullOrEmpty(originalPath) && System.IO.File.Exists(originalPath))
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(originalPath);
                return File(bytes, "application/octet-stream", fileName);
            }

            // Buscar en la carpeta local de backups
            var filePath = await _backupService.GetBackupFilePathAsync(fileName);
            if (filePath is null || !System.IO.File.Exists(filePath))
                return NotFound(new { mensaje = "Archivo de backup no encontrado." });

            var bytesLocal = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytesLocal, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = $"Error al descargar el backup: {ex.Message}" });
        }
    }
}
