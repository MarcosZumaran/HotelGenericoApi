using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces;

public interface IEstanciaService
{
    Task<List<Estancia>> GetAllAsync();
    Task<Estancia?> GetByIdAsync(int id);
    Task<Estancia> CreateAsync(Estancia estancia);
    Task<Estancia?> CheckoutAsync(int idEstancia, int idUsuario);
    Task<bool> AddHuespedAsync(int idEstancia, Huesped huesped);
    Task<bool> AddConsumoAsync(int idEstancia, ItemEstancia item);
    Task<bool> UpdateConsumoAsync(int idItem, int cantidad);
    Task<bool> DeleteConsumoAsync(int idItem);
    Task<List<ItemConsumoResponseDto>> GetConsumosAsync(int idEstancia);
    Task<List<ReservaResponseDto>> GetReservasByHabitacionAsync(int idHabitacion);
    Task<Estancia> CheckinAsync(CheckinCreateDto dto, int idUsuario);
    Task<Reserva> CreateReservaAsync(ReservaCreateDto dto, int idUsuario);
    Task<bool> CancelarReservaAsync(int idReserva);
    Task RegistrarSalidaTemporalAsync(int estanciaId, bool llavesDejadas);
    Task RegistrarRegresoAsync(int estanciaId);
    Task<Huesped> AgregarHuespedCompletoAsync(int estanciaId, AgregarHuespedDto dto);
    Task<CheckoutResultDto> RealizarCheckoutCompletoAsync(int estanciaId, int idUsuario);
}
