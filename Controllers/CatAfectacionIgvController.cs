using Microsoft.AspNetCore.Mvc;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CatAfectacionIgvController : ControllerBase
{
    private readonly ICatAfectacionIgvService _service;

    public CatAfectacionIgvController(ICatAfectacionIgvService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{codigo}")]
    public async Task<IActionResult> GetById(string codigo)
    {
        var result = await _service.GetByIdAsync(codigo);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CatAfectacionIgvCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { codigo = result.Codigo }, result);
    }

    [HttpPut("{codigo}")]
    public async Task<IActionResult> Update(string codigo, CatAfectacionIgvUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(codigo, dto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Delete(string codigo)
    {
        var deleted = await _service.DeleteAsync(codigo);
        return deleted ? NoContent() : NotFound();
    }
}