using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IFolioService
{
    Task<FolioEstanciaDto?> GetFolioAsync(int idEstancia);
}
