namespace HotelGenericoApi.Models;

public class ObjetoPerdido
{
    public int IdObjeto { get; set; }
    public int? IdHabitacion { get; set; }
    public int? IdEstancia { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime? FechaHallazgo { get; set; }
    public string Estado { get; set; } = "pendiente"; // pendiente, entregado, desechado
    public string? EntregadoA { get; set; }
    public DateTime? FechaEntregado { get; set; }

    // Navegación
    public Habitacion? Habitacion { get; set; }
    public Estancia? Estancia { get; set; }
}
