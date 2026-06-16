namespace HotelGenericoApi.DTOs.Response;

public sealed record GastoAmenitiesDetalleDto(
    int IdProducto,
    string Nombre,
    int CantidadTotal,
    decimal CostoUnitario,
    decimal CostoTotal
);
