namespace HotelGenericoApi.DTOs.Response;

public sealed record ParStockItemDto(
    int IdProducto,
    string Nombre,
    string? Categoria,
    int Stock,
    int StockMinimo,
    decimal NivelPorcentaje,
    bool EsAmenidad,
    string? UnidadMedida
);
