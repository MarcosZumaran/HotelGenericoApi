namespace HotelGenericoApi.DTOs.Response;

public class CheckoutParamsDto
{
    public string CheckoutHoraLimite { get; set; } = "12:00";
    public string CheckoutCargoPorHora { get; set; } = "50.00";
    public string CheckoutGraciaMinutos { get; set; } = "30";
}

public class CheckoutParamsUpdateDto
{
    public string? CheckoutHoraLimite { get; set; }
    public string? CheckoutCargoPorHora { get; set; }
    public string? CheckoutGraciaMinutos { get; set; }
}
