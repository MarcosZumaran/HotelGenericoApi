using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces;

public interface IConsumoEstanciaService
{
    Task<bool> AddConsumoAsync(int idEstancia, ItemEstancia item);
    Task<bool> AddConsumoBatchAsync(int idEstancia, List<ItemEstancia> items, int idUsuario);
    Task<bool> UpdateConsumoAsync(int idItem, int cantidad);
    Task<bool> DeleteConsumoAsync(int idItem);
}
