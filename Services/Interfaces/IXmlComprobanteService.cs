namespace HotelGenericoApi.Services.Interfaces;

public interface IXmlComprobanteService
{
    Task<string> GenerarXmlComprobanteAsync(int idComprobante);
}
