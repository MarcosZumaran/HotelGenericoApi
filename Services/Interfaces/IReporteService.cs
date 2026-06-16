using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces
{
    public interface IReporteService
    {
        Task<List<VCierreCajaDiario>> GetCierreCajaAsync(DateOnly fecha);
        Task<List<VEstadoHabitacion>> GetEstadoHabitacionesAsync();
        Task<List<VOcupacionDiaria>> GetOcupacionDiariaAsync(DateOnly fecha);
        Task<List<TopProductoDto>> GetTopProductosAsync(int dias);
    Task<List<PrevisionOcupacionDto>> GetPrevisionOcupacionAsync(int dias);
    Task<TiempoMedioLimpiezaDto> GetTiempoMedioLimpiezaAsync();
    Task<TasaCancelacionDto> GetTasaCancelacionesAsync(int meses);
    Task<List<ParStockItemDto>> GetParStockAsync();
    Task<List<StockCriticoDto>> GetStockCriticoAsync();
    Task<GastoAmenitiesResponseDto> GetGastoAmenitiesAsync(int dias);
    Task<List<GastoAmenitiesDiarioDto>> GetGastoAmenitiesDiarioAsync(int dias);
    }
}
