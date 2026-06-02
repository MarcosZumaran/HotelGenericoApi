using System.ComponentModel.DataAnnotations;

namespace HotelGenericoApi.DTOs.Request;

public sealed record HabitacionCreateDto
{
    [Required(ErrorMessage = "El número de habitación es obligatorio.")]
    [StringLength(10, MinimumLength = 1)]
    public string NumeroHabitacion { get; init; } = string.Empty;

    [Range(1, 99, ErrorMessage = "El piso debe estar entre 1 y 99.")]
    public int? Piso { get; init; }

    [StringLength(200)]
    public string? Descripcion { get; init; }

    [Required(ErrorMessage = "El tipo de habitación es obligatorio.")]
    [Range(1, int.MaxValue)]
    public int IdTipo { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio por noche debe ser mayor a 0.")]
    public decimal PrecioNoche { get; init; }

    public int? IdEstado { get; init; }
    public Dictionary<string, bool>? Caracteristicas { get; init; }
    public List<HabitacionAmenidadDto>? Amenidades { get; init; }
}

public record HabitacionAmenidadDto
{
    public int IdProducto { get; init; }
    public int CantidadBase { get; init; }
}
