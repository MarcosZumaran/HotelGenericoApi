namespace HotelGenericoApi.DTOs.Request;

public class TrasladarEstanciaDto
{
    public int NuevaHabitacionId { get; set; }
    public string? Motivo { get; set; }
    public bool CobrarDiferencia { get; set; } = true;
}
