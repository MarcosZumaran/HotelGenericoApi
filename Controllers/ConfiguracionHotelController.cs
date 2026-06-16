using Microsoft.AspNetCore.Mvc;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ConfiguracionHotelController : ControllerBase
{
    private readonly IConfiguracionHotelService _service;
    private readonly IWebHostEnvironment _env;

    public ConfiguracionHotelController(IConfiguracionHotelService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    [HttpGet]
    [ProducesResponseType(typeof(HotelGenericoApi.DTOs.Response.ConfiguracionHotelResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetConfiguracionAsync();
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update([FromBody] ConfiguracionGeneralUpdateDto dto)
    {
        if (!HasAnyValue(dto))
            return BadRequest(new { mensaje = "Debe enviar al menos un campo para actualizar" });

        await _service.UpdateConfiguracionAsync(dto);
        return NoContent();
    }

    [HttpPost("logo")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { mensaje = "Debe seleccionar un archivo" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            return BadRequest(new { mensaje = "Solo se permiten archivos PNG, JPG o JPEG" });

        if (file.Length > 500 * 1024)
            return BadRequest(new { mensaje = "El archivo no debe superar los 500 KB" });

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"logo{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"uploads/{fileName}";
        await _service.UpdateLogoAsync(relativePath);

        return Ok(new { logoUrl = relativePath });
    }

    private static bool HasAnyValue(ConfiguracionGeneralUpdateDto dto)
    {
        return typeof(ConfiguracionGeneralUpdateDto).GetProperties()
            .Any(p => p.GetValue(dto) != null);
    }
}
