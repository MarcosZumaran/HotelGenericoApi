namespace HotelGenericoApi.Models;

public class Producto
{
    public int IdProducto { get; set; }
    public string? CodigoSunat { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string IdAfectacionIgv { get; set; } = null!; // FK
    public int? IdCategoria { get; set; }
    public int? Stock { get; set; }
    public int? StockMinimo { get; set; }
    public string? UnidadMedida { get; set; }
    public string? ImagenUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool EsAmenidad { get; set; }
    public bool EsVendibleEnTienda { get; set; }
    public int? StockPorHabitacion { get; set; }

    // Navegación
    public AfectacionIgv? AfectacionIgv { get; set; }
    public CategoriaProducto? Categoria { get; set; }
}
