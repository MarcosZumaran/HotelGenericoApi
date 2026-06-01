namespace HotelGenericoApi.DTOs.Request;

public class ConsumirAmenidadDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public bool EsCargableAlHuésped { get; set; } = false; // si true, se cobra; si false no
}
