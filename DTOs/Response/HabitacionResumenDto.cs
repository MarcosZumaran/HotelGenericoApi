namespace HotelGenericoApi.DTOs.Response;

public class HabitacionResumenDto
{
    public int IdHabitacion { get; set; }
    public string NumeroHabitacion { get; set; } = string.Empty;
    public string TipoNombre { get; set; } = string.Empty;
    public decimal PrecioNoche { get; set; }
    public string Estado { get; set; } = string.Empty; // "Reservada", "Ocupada", "Disponible"
    public int? IdEstanciaActiva { get; set; }
    public int? IdReserva { get; set; }
}
