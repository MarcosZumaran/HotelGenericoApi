using Microsoft.AspNetCore.Mvc;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ConfiguracionHotelController : ControllerBase
{
    private readonly IConfiguracionHotelService _service;

    public ConfiguracionHotelController(IConfiguracionHotelService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetConfiguracionAsync();
        return Ok(result);
    }
}