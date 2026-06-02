using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IReservaCorporativaService
{
    Task<IEnumerable<ReservaCorporativaResponseDto>> GetAllAsync();
    Task<ReservaCorporativaResponseDto?> GetByIdAsync(int id);
    Task<ReservaCorporativaResponseDto> CreateAsync(ReservaCorporativaCreateDto dto, int idUsuario);
    Task<bool> UpdateAsync(int id, ReservaCorporativaCreateDto dto, int idUsuario);
    Task<bool> DeleteAsync(int id);
    Task<ReservaCorporativaResponseDto> FinalizarAsync(int id, int idUsuario);
    Task<bool> ValidarYAsignarHabitacionAsync(int idReservaCorporativa);
}
