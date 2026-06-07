using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IReservaQueryService
{
    Task<List<ReservaResponseDto>> GetAllAsync();
    Task<PagedResult<ReservaResponseDto>> GetPagedAsync(int page, int pageSize);
}
