namespace HotelGenericoApi.DTOs.Request;

public class HuespedDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = "0";
    public string Documento { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public bool EsTitular { get; set; } = false;
    public bool EsAnonimo { get; set; } = false;
}
