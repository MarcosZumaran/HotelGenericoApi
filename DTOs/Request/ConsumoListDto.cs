namespace HotelGenericoApi.DTOs.Request;

public sealed record ConsumoListItemDto
{
    public int IdProducto { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
}

public sealed record ConsumoListDto
{
    public List<ConsumoListItemDto> Consumos { get; init; } = new();
}
