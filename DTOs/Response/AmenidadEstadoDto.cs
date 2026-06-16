namespace HotelGenericoApi.DTOs.Response;

public class AmenidadEstadoDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadBase { get; set; }
    public int CantidadActual { get; set; }
    public int Diferencia { get; set; }
    public bool EsAmenidad { get; set; }
}
