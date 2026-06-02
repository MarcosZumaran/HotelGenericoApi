namespace HotelGenericoApi.DTOs.Request;

public sealed record HabitacionUpdateDto
{
    public int? Piso { get; init; }
    public string? Descripcion { get; init; }
    public int? IdTipo { get; init; }
    public decimal? PrecioNoche { get; init; }
    public int? IdEstado { get; init; }
    public string? NumeroHabitacion { get; init; }
    public Dictionary<string, bool>? Caracteristicas { get; init; }
    public List<HabitacionAmenidadDto>? Amenidades { get; init; }
}
