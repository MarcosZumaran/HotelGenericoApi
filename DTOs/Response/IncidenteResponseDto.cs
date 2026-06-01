namespace HotelGenericoApi.DTOs.Response;

public class IncidenteResponseDto
{
    public int IdIncidente { get; set; }
    public int? IdEstancia { get; set; }
    public int IdHabitacion { get; set; }
    public string NumeroHabitacion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal? CostoEstimado { get; set; }
    public bool CobradoAlCliente { get; set; }
    public bool Resuelto { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public string? ReportadoPorNombre { get; set; }
}
