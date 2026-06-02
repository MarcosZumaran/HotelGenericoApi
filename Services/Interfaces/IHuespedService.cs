using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces;

public interface IHuespedService
{
    Task<Huesped> AgregarHuespedCompletoAsync(int estanciaId, AgregarHuespedDto dto);
}
