namespace HotelGenericoApi.DTOs.Response;

public class TasaCancelacionDto
{
    public int TotalReservas { get; set; }
    public int Canceladas { get; set; }
    public decimal Tasa { get; set; }
}
