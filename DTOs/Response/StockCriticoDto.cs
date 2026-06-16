namespace HotelGenericoApi.DTOs.Response;

public sealed record StockCriticoDto(
    int IdProducto,
    string Nombre,
    int Stock,
    int StockMinimo
);
