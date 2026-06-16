namespace HotelGenericoApi.DTOs.Request;

public sealed record CheckinCreateDto
{
    public int IdHabitacion { get; init; }
    public int? IdReserva { get; init; }
    public DateTime FechaCheckoutPrevista { get; init; }
    public string TipoDocumento { get; init; } = "1";
    public string Documento { get; init; } = string.Empty;
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string MetodoPago { get; init; } = Constants.MetodoPagoCodigo.Efectivo;
    public bool UsarClienteAnonimo { get; init; } = false;
    public bool GuardarCliente { get; set; } = true;
    public int? IdClienteExistente { get; set; }
    public DateTime? FechaEntradaPrevista { get; init; }
    public DateTime? FechaSalidaPrevista { get; init; }
    public int? IdReservaCorporativa { get; set; }
    public bool EsPorHoras { get; init; } = false;
    public bool AplicarDepositoGarantia { get; init; } = true;
    public List<HuespedDto>? Huespedes { get; init; }
}
