namespace HotelGenericoApi.DTOs.Response;

public class DepositoGarantiaParamsDto
{
    public string DepositoHabilitado { get; set; } = "false";
    public string DepositoMonto { get; set; } = "0";
    public string DepositoPorcentaje { get; set; } = "0";
}

public class DepositoGarantiaParamsUpdateDto
{
    public string? DepositoHabilitado { get; set; }
    public string? DepositoMonto { get; set; }
    public string? DepositoPorcentaje { get; set; }
}
