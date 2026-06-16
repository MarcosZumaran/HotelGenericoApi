namespace HotelGenericoApi.DTOs.Response;

public class LimpiezaParamsDto
{
    public string LimpiezaSalidaTiempo { get; set; } = "40";
    public string LimpiezaEstanciaTiempo { get; set; } = "20";
    public string LimpiezaFrecuenciaHoras { get; set; } = "24";
    public string LimpiezaHorarioInicio { get; set; } = "08:00";
    public string LimpiezaHorarioFin { get; set; } = "14:00";
}

public class LimpiezaParamsUpdateDto
{
    public string? LimpiezaSalidaTiempo { get; set; }
    public string? LimpiezaEstanciaTiempo { get; set; }
    public string? LimpiezaFrecuenciaHoras { get; set; }
    public string? LimpiezaHorarioInicio { get; set; }
    public string? LimpiezaHorarioFin { get; set; }
}
