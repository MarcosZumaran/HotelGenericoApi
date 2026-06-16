namespace HotelGenericoApi.DTOs.Response;

public class LlegadaHoyDto
{
    public int IdReserva { get; set; }
    public string? ClienteNombre { get; set; }
    public string? DocumentoCliente { get; set; }
    public int? IdHabitacion { get; set; }
    public string? NumeroHabitacion { get; set; }
    public string? TipoHabitacion { get; set; }
    public DateTime FechaEntradaPrevista { get; set; }
    public DateTime FechaSalidaPrevista { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? EmpresaCorporativa { get; set; }
    public bool EsReservaCorporativa { get; set; }
    public bool EsNoShow { get; set; }
}
