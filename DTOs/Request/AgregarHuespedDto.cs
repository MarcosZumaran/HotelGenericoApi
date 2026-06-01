namespace HotelGenericoApi.DTOs.Request;

public class AgregarHuespedDto
{
    public string TipoDocumento { get; set; } = "0";
    public string Documento { get; set; } = "";
    public string Nombres { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}
