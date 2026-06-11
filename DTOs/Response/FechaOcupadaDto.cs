namespace HotelGenericoApi.DTOs.Response;

public sealed record FechaOcupadaDto(
    int IdHabitacion,
    DateTime Desde,
    DateTime Hasta
);
