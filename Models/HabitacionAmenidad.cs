namespace HotelGenericoApi.Models;

public class HabitacionAmenidad
{
    public int IdHabitacionAmenidad { get; set; }
    public int IdHabitacion { get; set; }
    public int IdProducto { get; set; }
    public int CantidadBase { get; set; }

    // Navegación
    public Habitacion? Habitacion { get; set; }
    public Producto? Producto { get; set; }
}
