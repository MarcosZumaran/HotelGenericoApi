using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces;

public interface IReservaCommandService
{
    Task<Reserva> CreateReservaAsync(ReservaCreateDto dto, int idUsuario);
    Task<bool> CancelarReservaAsync(int idReserva);
}
