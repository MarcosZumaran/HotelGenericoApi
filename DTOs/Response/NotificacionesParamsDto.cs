namespace HotelGenericoApi.DTOs.Response;

public class NotificacionesParamsDto
{
    public string EmailNotificaciones { get; set; } = "";
    public string NotificarCheckin { get; set; } = "true";
    public string NotificarCheckout { get; set; } = "true";
    public string NotificarIncidentes { get; set; } = "false";
    public string NotificarStockBajo { get; set; } = "true";
}

public class NotificacionesParamsUpdateDto
{
    public string? EmailNotificaciones { get; set; }
    public string? NotificarCheckin { get; set; }
    public string? NotificarCheckout { get; set; }
    public string? NotificarIncidentes { get; set; }
    public string? NotificarStockBajo { get; set; }
}
