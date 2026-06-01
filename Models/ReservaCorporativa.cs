namespace HotelGenericoApi.Models;

public class ReservaCorporativa
{
    public int IdReservaCorporativa { get; set; }
    public int IdClienteEmpresa { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int NumeroHabitaciones { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? Observaciones { get; set; }
    public DateTime? FechaRegistro { get; set; }

    // Navegación
    public Cliente? ClienteEmpresa { get; set; }
    public ICollection<Estancia> Estancias { get; set; } = new List<Estancia>();
}
