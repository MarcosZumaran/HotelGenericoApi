namespace HotelGenericoApi.Models;

public class StockHabitacion
{
    public int IdStock { get; set; }
    public int IdHabitacion { get; set; }
    public int IdProducto { get; set; }
    public int CantidadActual { get; set; }

    // Navegación
    public Habitacion? Habitacion { get; set; }
    public Producto? Producto { get; set; }
}
