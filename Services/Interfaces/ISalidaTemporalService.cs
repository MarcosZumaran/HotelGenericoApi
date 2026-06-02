namespace HotelGenericoApi.Services.Interfaces;

public interface ISalidaTemporalService
{
    Task RegistrarSalidaTemporalAsync(int estanciaId, bool llavesDejadas);
    Task RegistrarRegresoAsync(int estanciaId);
}
