namespace HotelGenericoApi.Models;

public class Reserva
{
    public int IdReserva { get; set; }
    public int IdCliente { get; set; }
    public int IdHabitacion { get; set; }
    public int IdUsuario { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public DateTime FechaEntradaPrevista { get; set; }
    public DateTime FechaSalidaPrevista { get; set; }
    public decimal MontoTotal { get; set; }
    public string? Estado { get; set; }
    public string? Observaciones { get; set; }
    public bool EsNoShow { get; set; }
    public int? IdReservaCorporativa { get; set; }

    // Navegación
    public Cliente? Cliente { get; set; }
    public Habitacion? Habitacion { get; set; }
    public Usuario? Usuario { get; set; }
}
