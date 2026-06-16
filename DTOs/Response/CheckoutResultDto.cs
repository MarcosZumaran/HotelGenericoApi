namespace HotelGenericoApi.DTOs.Response;

public class CheckoutResultDto
{
    public decimal TotalHabitacion { get; set; }
    public decimal TotalConsumos { get; set; }
    public decimal TotalFinal { get; set; }
    public int ComprobanteId { get; set; }
    public decimal? CargoLateCheckout { get; set; }
    public int? HorasLateCheckout { get; set; }
    public decimal? MontoDepositoGarantia { get; set; }
    public bool? DepositoAplicado { get; set; }
}
