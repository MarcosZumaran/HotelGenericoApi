using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IIncidenteService
{
    Task<IEnumerable<IncidenteResponseDto>> GetAllIncidentesAsync();
    Task<IncidenteResponseDto?> GetIncidenteByIdAsync(int id);
    Task<IEnumerable<IncidenteResponseDto>> GetIncidentesByHabitacionAsync(int idHabitacion);
    Task<IncidenteResponseDto> CreateIncidenteAsync(IncidenteCreateDto dto, int idUsuario);
    Task<bool> ResolverIncidenteAsync(int id);
    Task<bool> MarcarCobradoAsync(int id, bool cobrado);

    Task<IEnumerable<ObjetoPerdidoResponseDto>> GetAllObjetosPerdidosAsync();
    Task<ObjetoPerdidoResponseDto?> GetObjetoPerdidoByIdAsync(int id);
    Task<IEnumerable<ObjetoPerdidoResponseDto>> GetObjetosPendientesAsync();
    Task<ObjetoPerdidoResponseDto> CreateObjetoPerdidoAsync(ObjetoPerdidoCreateDto dto);
    Task<bool> EntregarObjetoAsync(int id, string entregadoA);
    Task<bool> DesecharObjetoAsync(int id);
}
