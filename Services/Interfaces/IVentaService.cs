using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces
{
    public interface IVentaService
    {
        Task<List<VentaResponseDto>> GetAllAsync();
        Task<PagedResult<VentaResponseDto>> GetPagedAsync(int page, int pageSize);
        Task<VentaResponseDto?> GetByIdAsync(int id);
        Task<VentaResponseDto> CreateAsync(VentaCreateDto dto, int idUsuario);
        Task<bool> DeleteAsync(int id);
    }
}
