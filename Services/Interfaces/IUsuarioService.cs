using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioResponseDto>> GetAllAsync();
    Task<PagedResult<UsuarioResponseDto>> GetPagedAsync(int page, int pageSize);
    Task<UsuarioResponseDto?> GetByIdAsync(int id);
    Task<UsuarioResponseDto> CreateAsync(UsuarioCreateDto dto);
    Task<bool> UpdateAsync(int id, UsuarioUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<(string token, string refreshToken, UsuarioResponseDto usuario)?> LoginAsync(LoginDto dto, string? ipAddress, string? userAgent);
    Task<(string token, string refreshToken, UsuarioResponseDto usuario)?> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokensAsync(int idUsuario);
}
