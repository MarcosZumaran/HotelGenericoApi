using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces
{
    public interface IHabitacionService
    {
        Task<List<Habitacion>> GetAllAsync();
        Task<PagedResult<Habitacion>> GetPagedAsync(int page, int pageSize);
        Task<Habitacion?> GetByIdAsync(int id);
        Task<Habitacion> CreateAsync(Habitacion habitacion);
        Task<Habitacion?> UpdateAsync(int id, Habitacion habitacionActualizada);
        Task<bool> DeleteAsync(int id);
        Task<bool> CambiarEstadoAsync(int idHabitacion, int idNuevoEstado, int idUsuario, string? observacion = null);
        Task<List<HabitacionEstadoActualDto>> GetEstadoActualAsync();
        Task<List<HabitacionEstadoActualDto>> GetDisponiblesAsync(DateTime fechaEntrada, DateTime fechaSalida);
        Task<List<HabitacionAmenidad>> GetAmenidadesPorHabitacionAsync(int idHabitacion);
        Task<bool> ActualizarAmenidadesAsync(int idHabitacion, List<HabitacionAmenidadDto> amenidades);
        Task<Dictionary<string, bool>?> GetCaracteristicasAsync(int idHabitacion);
        Task<bool> ActualizarCaracteristicasAsync(int idHabitacion, Dictionary<string, bool> caracteristicas);
    }
}
