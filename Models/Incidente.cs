namespace HotelGenericoApi.Models;

public class Incidente
{
    public int IdIncidente { get; set; }
    public int? IdEstancia { get; set; }
    public int IdHabitacion { get; set; }
    public string Tipo { get; set; } = string.Empty; // daño, mancha, rotura
    public string Descripcion { get; set; } = string.Empty;
    public decimal? CostoEstimado { get; set; }
    public bool CobradoAlCliente { get; set; }
    public bool Resuelto { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public int? ReportadoPor { get; set; }

    // Navegación
    public Estancia? Estancia { get; set; }
    public Habitacion? Habitacion { get; set; }
    public Usuario? UsuarioReporte { get; set; }
}
