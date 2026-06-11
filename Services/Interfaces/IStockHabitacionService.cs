using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IStockHabitacionService
{
    Task<List<StockHabitacionDto>> GetByHabitacionAsync(int idHabitacion);
    Task<bool> ConsumirProductoAsync(int idStock, int cantidad);
}
