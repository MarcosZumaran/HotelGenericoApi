namespace HotelGenericoApi.DTOs.Response;

public class ReservaCorporativaResponseDto
{
    public int IdReservaCorporativa { get; set; }
    public int IdClienteEmpresa { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string RucEmpresa { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int NumeroHabitaciones { get; set; }
    public int HabitacionesOcupadas { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal TotalAcumulado { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaRegistro { get; set; }
}
