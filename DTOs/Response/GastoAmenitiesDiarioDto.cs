namespace HotelGenericoApi.DTOs.Response;

public sealed record GastoAmenitiesDiarioDto(
    DateOnly Fecha,
    decimal CostoTotal
);
