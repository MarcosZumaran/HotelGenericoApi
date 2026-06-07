using System.ComponentModel.DataAnnotations;

namespace HotelGenericoApi.DTOs.Request;

public sealed record VentaCreateDto
{
    public int? IdCliente { get; init; }

    [Required(ErrorMessage = "El método de pago es obligatorio.")]
    [StringLength(3, MinimumLength = 3)]
    public string MetodoPago { get; init; } = Constants.MetodoPagoCodigo.Efectivo;

    [MinLength(1, ErrorMessage = "Debe incluir al menos un producto.")]
    public List<ItemVentaCreateDto> Items { get; init; } = new();
}

public sealed record ItemVentaCreateDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int IdProducto { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; init; }
}
