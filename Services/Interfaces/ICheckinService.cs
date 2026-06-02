using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces;

public interface ICheckinService
{
    Task<Estancia> CheckinAsync(CheckinCreateDto dto, int idUsuario);
}
