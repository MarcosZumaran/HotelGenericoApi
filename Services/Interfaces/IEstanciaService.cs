using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces;

public interface IEstanciaService
{
    // Consultas
    Task<List<Estancia>> GetAllAsync();
    Task<Estancia?> GetByIdAsync(int id);
    Task<List<ItemConsumoResponseDto>> GetConsumosAsync(int idEstancia);
    Task<List<ReservaResponseDto>> GetReservasByHabitacionAsync(int idHabitacion);

    // Operaciones principales
    Task<Estancia> CheckinAsync(CheckinCreateDto dto, int idUsuario);
    Task<CheckoutResultDto> RealizarCheckoutAsync(int estanciaId, int idUsuario);

    // Salidas temporales
    Task RegistrarSalidaTemporalAsync(int estanciaId, bool llavesDejadas);
    Task RegistrarRegresoAsync(int estanciaId);

    // Huéspedes adicionales
    Task<Huesped> AgregarHuespedCompletoAsync(int estanciaId, AgregarHuespedDto dto);

    // Consumos
    Task<bool> AddConsumoAsync(int idEstancia, ItemEstancia item);
    Task<bool> UpdateConsumoAsync(int idItem, int cantidad);
    Task<bool> DeleteConsumoAsync(int idItem);

    // Reservas
    Task<Reserva> CreateReservaAsync(ReservaCreateDto dto, int idUsuario);
    Task<bool> CancelarReservaAsync(int idReserva);

    // traslado
    Task<TrasladoResultDto> TrasladarHabitacionAsync(int estanciaId, TrasladarEstanciaDto dto, int idUsuario);
}
