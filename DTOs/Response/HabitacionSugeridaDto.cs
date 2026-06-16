namespace HotelGenericoApi.DTOs.Response;

public sealed record HabitacionSugeridaDto(
    int IdHabitacion,
    string Numero,
    string NombreTipo,
    int Piso,
    decimal PrecioNoche,
    int Capacidad
);
