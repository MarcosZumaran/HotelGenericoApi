namespace HotelGenericoApi.DTOs.Response;

public class PrevisionOcupacionDto
{
    public DateOnly Fecha { get; set; }
    public int Ocupadas { get; set; }
    public int TotalHabitaciones { get; set; }
    public decimal Porcentaje { get; set; }
}
