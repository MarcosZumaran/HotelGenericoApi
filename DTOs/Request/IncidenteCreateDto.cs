namespace HotelGenericoApi.DTOs.Request;

public class IncidenteCreateDto
{
    public int? IdEstancia { get; set; }
    public int IdHabitacion { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal? CostoEstimado { get; set; }
    public bool CobradoAlCliente { get; set; } = false;
}
