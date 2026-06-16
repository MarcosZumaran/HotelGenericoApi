namespace HotelGenericoApi.Services.Interfaces;

public interface IPdfService
{
    Task<byte[]> GenerarPdfComprobanteAsync(int idComprobante);
    Task<byte[]> GenerarPdfVentaAsync(int idVenta);
    Task<byte[]> GenerarPdfEstanciaAsync(int idEstancia);
    Task<byte[]> GenerarPdfCierreCajaAsync(DateOnly fecha);
    Task<byte[]> GenerarPdfEstanciasActivasAsync();
    Task<byte[]> GenerarPdfParStockAsync();
}