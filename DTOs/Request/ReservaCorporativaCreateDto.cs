namespace HotelGenericoApi.DTOs.Request;

public class ReservaCorporativaCreateDto
{
    public int IdClienteEmpresa { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int NumeroHabitaciones { get; set; }
    public string? Observaciones { get; set; }
    public List<int> HabitacionesIds { get; set; } = new List<int>();
}
