namespace HotelGenericoApi.DTOs.Response;

public class StockHabitacionDto
{
    public int IdStock { get; set; }
    public int IdHabitacion { get; set; }
    public string NumeroHabitacion { get; set; } = string.Empty;
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int CantidadActual { get; set; }
    public int? StockBase { get; set; }
    public bool EsAmenidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Categoria { get; set; }
}
