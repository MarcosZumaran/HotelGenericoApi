namespace HotelGenericoApi.DTOs.Response;

public class ObjetoPerdidoResponseDto
{
    public int IdObjeto { get; set; }
    public int? IdHabitacion { get; set; }
    public string? NumeroHabitacion { get; set; }
    public int? IdEstancia { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime? FechaHallazgo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? EntregadoA { get; set; }
    public DateTime? FechaEntregado { get; set; }
}
