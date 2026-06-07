using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using Microsoft.AspNetCore.Http;

namespace HotelGenericoApi.Services.Interfaces;

public interface IIncidenteService
{
    // Incidentes
    Task<IEnumerable<IncidenteResponseDto>> GetAllIncidentesAsync();
    Task<PagedResult<IncidenteResponseDto>> GetPagedIncidentesAsync(int page, int pageSize);
    Task<IncidenteResponseDto?> GetIncidenteByIdAsync(int id);
    Task<IEnumerable<IncidenteResponseDto>> GetIncidentesByHabitacionAsync(int idHabitacion);
    Task<IncidenteResponseDto> CreateIncidenteAsync(IncidenteCreateDto dto, int idUsuario, IFormFile? imagen);
    Task<bool> ResolverIncidenteAsync(int id);
    Task<bool> MarcarCobradoAsync(int id, bool cobrado);

    // Objetos Perdidos
    Task<IEnumerable<ObjetoPerdidoResponseDto>> GetAllObjetosPerdidosAsync();
    Task<PagedResult<ObjetoPerdidoResponseDto>> GetPagedObjetosPerdidosAsync(int page, int pageSize);
    Task<ObjetoPerdidoResponseDto?> GetObjetoPerdidoByIdAsync(int id);
    Task<IEnumerable<ObjetoPerdidoResponseDto>> GetObjetosPendientesAsync();
    Task<ObjetoPerdidoResponseDto> CreateObjetoPerdidoAsync(ObjetoPerdidoCreateDto dto, IFormFile? imagen);
    Task<bool> EntregarObjetoAsync(int id, string entregadoA);
    Task<bool> DesecharObjetoAsync(int id);
}
