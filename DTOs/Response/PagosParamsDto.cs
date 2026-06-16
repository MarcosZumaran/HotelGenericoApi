namespace HotelGenericoApi.DTOs.Response;

public class PagosParamsDto
{
    public string MetodosPagoHabilitados { get; set; } = "Efectivo,Tarjeta,Depósito";
    public string TasaIgvHotel { get; set; } = "10.50";
    public string TasaIgvProductos { get; set; } = "18.00";
}

public class PagosParamsUpdateDto
{
    public string? MetodosPagoHabilitados { get; set; }
    public string? TasaIgvHotel { get; set; }
    public string? TasaIgvProductos { get; set; }
}
