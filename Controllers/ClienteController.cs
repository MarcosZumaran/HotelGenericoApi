using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Administrador,Recepcionista")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class ClienteController : ControllerBase
{
    private readonly IClienteService _service;
    private readonly IReniecService _reniecService;

    public ClienteController(IClienteService service, IReniecService reniecService)
    {
        _service = service;
        _reniecService = reniecService;
    }

    /// <summary>Obtiene todos los clientes registrados con paginación.</summary>
    /// <param name="page">Número de página (por defecto 1).</param>
    /// <param name="pageSize">Tamaño de página (por defecto 10).</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetPagedAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Obtiene un cliente por su ID.</summary>
    /// <param name="id">ID del cliente.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is not null ? Ok(result) : NotFound();
    }

    /// <summary>Obtiene un cliente por tipo y número de documento.</summary>
    /// <param name="tipo">Tipo de documento (1=DNI, 6=RUC, etc.).</param>
    /// <param name="documento">Número de documento.</param>
    [HttpGet("documento/{tipo}/{documento}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDocumento(string tipo, string documento)
    {
        var result = await _service.GetByDocumentoAsync(tipo, documento);
        return result is not null ? Ok(result) : NotFound();
    }

    /// <summary>Crea un nuevo cliente.</summary>
    /// <param name="dto">Datos del cliente.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(ClienteCreateDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.IdCliente }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    /// <summary>Actualiza los datos de un cliente existente.</summary>
    /// <param name="id">ID del cliente.</param>
    /// <param name="dto">Datos actualizados.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ClienteUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Elimina un cliente por su ID.</summary>
    /// <param name="id">ID del cliente.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Consulta los datos de un DNI en RENIEC (VerificaPE).</summary>
    /// <param name="dni">Número de DNI (8 dígitos).</param>
    /// <response code="200">Datos encontrados correctamente.</response>
    /// <response code="502">Error al contactar con el servicio RENIEC.</response>
    [HttpGet("reniec/{dni}")]
    [EnableRateLimiting("reniec")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ConsultarReniec(string dni)
    {
        var result = await _reniecService.ConsultarDniAsync(dni);
        if (result is null)
            return NotFound(new { mensaje = "DNI no encontrado en RENIEC." });
        return Ok(result);
    }

    /// <summary>Consulta los datos de un RUC en SUNAT (VerificaPE).</summary>
    /// <param name="ruc">Número de RUC (11 dígitos).</param>
    /// <response code="200">Datos encontrados correctamente.</response>
    /// <response code="400">Formato de RUC inválido.</response>
    /// <response code="404">RUC no encontrado en SUNAT.</response>
    /// <response code="502">Error al contactar con el servicio SUNAT.</response>
    [HttpGet("consultar-ruc")]
    [EnableRateLimiting("reniec")]
    [ProducesResponseType(typeof(ReniecRucResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ConsultarRuc([FromQuery] string ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc) || ruc.Length != 11 || !ruc.All(char.IsDigit))
            return BadRequest(new { mensaje = "El RUC debe tener 11 dígitos numéricos." });

        var result = await _reniecService.ConsultarRucAsync(ruc);
        if (result is null)
            return NotFound(new { mensaje = "RUC no encontrado en SUNAT." });
        return Ok(result);
    }

    /// <summary>Busca clientes por nombre, documento u otros criterios.</summary>
    /// <param name="termino">Término de búsqueda (mínimo 2 caracteres).</param>
    [HttpGet("buscar")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarClientes([FromQuery] string termino)
    {
        if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
            return Ok(Array.Empty<ClienteResponseDto>());

        var resultados = await _service.BuscarAsync(termino, 5);
        return Ok(resultados);
    }
}
