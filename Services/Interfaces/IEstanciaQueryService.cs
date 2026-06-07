using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces;

public interface IEstanciaQueryService
{
    Task<List<Estancia>> GetAllAsync();
    Task<PagedResult<Estancia>> GetPagedAsync(int page, int pageSize);
    Task<List<Estancia>> GetActivasAsync();
    Task<Estancia?> GetByIdAsync(int id);
    Task<List<ItemConsumoResponseDto>> GetConsumosAsync(int idEstancia);
    Task<List<ReservaResponseDto>> GetReservasByHabitacionAsync(int idHabitacion);
}
