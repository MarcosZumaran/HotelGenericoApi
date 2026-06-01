namespace HotelGenericoApi.DTOs.Request;

public class ObjetoPerdidoCreateDto
{
    public int? IdHabitacion { get; set; }
    public int? IdEstancia { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
}
