using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IProductoService
{
    Task<IEnumerable<ProductoResponseDto>> GetAllAsync();
    Task<PagedResult<ProductoResponseDto>> GetPagedAsync(int page, int pageSize);
    Task<ProductoResponseDto?> GetByIdAsync(int id);
    Task<ProductoResponseDto> CreateAsync(ProductoCreateDto dto, IFormFile? file);
    Task<bool> UpdateAsync(int id, ProductoUpdateDto dto, IFormFile? file);
    Task<bool> DeleteAsync(int id);
    Task<bool> AddStockAsync(int id, int cantidad);
    Task<bool> SetImagenUrlAsync(int id, string url);
}
