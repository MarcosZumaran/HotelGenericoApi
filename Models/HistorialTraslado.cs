namespace HotelGenericoApi.Models;

public class HistorialTraslado
{
    public int IdTraslado { get; set; }
    public int IdEstancia { get; set; }
    public int IdHabitacionOrigen { get; set; }
    public int IdHabitacionDestino { get; set; }
    public string? Motivo { get; set; }
    public DateTime? FechaTraslado { get; set; }
    public int UsuarioId { get; set; }
    public decimal? AjusteMonto { get; set; }

    // Navegación
    public Estancia? Estancia { get; set; }
    public Habitacion? HabitacionOrigen { get; set; }
    public Habitacion? HabitacionDestino { get; set; }
    public Usuario? Usuario { get; set; }
}
