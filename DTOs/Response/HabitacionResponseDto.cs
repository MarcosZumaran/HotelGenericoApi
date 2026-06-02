namespace HotelGenericoApi.DTOs.Response;

public sealed record HabitacionResponseDto(
    int IdHabitacion,
    string NumeroHabitacion,
    int? Piso,
    string? Descripcion,
    int IdTipo,
    string NombreTipo,
    decimal PrecioNoche,
    int? IdEstado,
    string? NombreEstado,
    DateTime? FechaUltimoCambio,
    int? UsuarioCambio,
    Dictionary<string, bool>? Caracteristicas,
    List<HabitacionAmenidadResponseDto>? Amenidades
);

public record HabitacionAmenidadResponseDto
{
    public int IdProducto { get; init; }
    public string NombreProducto { get; init; } = string.Empty;
    public int CantidadBase { get; init; }
}
