namespace HotelGenericoApi.DTOs.Response;

public sealed record GastoAmenitiesResponseDto(
    decimal CostoTotal,
    int Dias,
    List<GastoAmenitiesDetalleDto> Detalle
);
