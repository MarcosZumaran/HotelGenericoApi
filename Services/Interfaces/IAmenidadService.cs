using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;

namespace HotelGenericoApi.Services.Interfaces;

public interface IAmenidadService
{
    Task InicializarStockHabitacionAsync(int idHabitacion);
    Task ReponerStockHabitacionAsync(int idHabitacion);
    Task<StockHabitacionDto?> ConsumirAmenidadAsync(int idHabitacion, ConsumirAmenidadDto dto);
    Task<List<StockHabitacionDto>> GetStockHabitacionAsync(int idHabitacion);
    Task<bool> ReponerAmenidadIndividualAsync(int idHabitacion, int idProducto, int cantidad);
    Task<List<AmenidadEstadoDto>> GetAmenidadesEstadoAsync(int idHabitacion);
    Task<int> ReponerAmenidadesHabitacionAsync(int idHabitacion, int idUsuario);
}
