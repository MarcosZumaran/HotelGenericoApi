namespace HotelGenericoApi.Models;

public class Habitacion
{
    public int IdHabitacion { get; set; }
    public string NumeroHabitacion { get; set; } = null!;
    public int? Piso { get; set; }
    public string? Descripcion { get; set; }
    public int IdTipo { get; set; }
    public decimal PrecioNoche { get; set; }
    public int IdEstado { get; set; }
    public DateTime? FechaUltimoCambio { get; set; }
    public int? UsuarioCambio { get; set; }
    public string? Caracteristicas { get; set; }

    // Navegación
    public TipoHabitacion? Tipo { get; set; }
    public EstadoHabitacion? Estado { get; set; }
    public Usuario? Usuario { get; set; }
    public ICollection<Estancia> Estancias { get; set; } = new List<Estancia>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<HabitacionAmenidad> HabitacionAmenidades { get; set; } = new List<HabitacionAmenidad>();
}
