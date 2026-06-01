namespace HotelGenericoApi.DTOs.Response;

public class CheckoutResultDto
{
    public decimal TotalHabitacion { get; set; }
    public decimal TotalConsumos { get; set; }
    public decimal TotalFinal { get; set; }
    public int ComprobanteId { get; set; }
}
