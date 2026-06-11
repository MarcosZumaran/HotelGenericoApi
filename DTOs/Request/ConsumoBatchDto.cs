namespace HotelGenericoApi.DTOs.Request;

public sealed record ConsumoBatchItemDto
{
    public int IdProducto { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
}

public sealed record ConsumoBatchDto
{
    public List<ConsumoBatchItemDto> Items { get; init; } = new();
}
