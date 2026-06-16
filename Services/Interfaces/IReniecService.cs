using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IReniecService
{
    Task<string?> ConsultarDniAsync(string dni);
    Task<ReniecRucResponseDto?> ConsultarRucAsync(string ruc);
}
