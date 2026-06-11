using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IReservaQueryService
{
    Task<List<ReservaResponseDto>> GetAllAsync(string? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idHabitacion = null, string? cliente = null, string? tipo = null);
    Task<PagedResult<ReservaResponseDto>> GetPagedAsync(int page, int pageSize);
    Task<List<FechaOcupadaDto>> GetFechasOcupadasAsync(int? idHabitacion = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
}
