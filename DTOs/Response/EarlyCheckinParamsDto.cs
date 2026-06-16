namespace HotelGenericoApi.DTOs.Response;

public class EarlyCheckinParamsDto
{
    public string EarlyCheckinHoraLimite { get; set; } = "10:00";
    public string EarlyCheckinCargo { get; set; } = "20.00";
}

public class EarlyCheckinParamsUpdateDto
{
    public string? EarlyCheckinHoraLimite { get; set; }
    public string? EarlyCheckinCargo { get; set; }
}
