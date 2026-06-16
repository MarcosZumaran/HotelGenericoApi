namespace HotelGenericoApi.DTOs.Response;

public class TrasladoResultDto
{
    public int IdEstancia { get; set; }
    public int HabitacionOrigenId { get; set; }
    public string HabitacionOrigenNumero { get; set; } = string.Empty;
    public int HabitacionDestinoId { get; set; }
    public string HabitacionDestinoNumero { get; set; } = string.Empty;
    public decimal MontoAnterior { get; set; }
    public decimal MontoNuevo { get; set; }
    public decimal Ajuste { get; set; }
    public string? Motivo { get; set; }
    public decimal DiferenciaCobrada { get; set; }
    public int NochesRestantes { get; set; }
}
