namespace HotelGenericoApi.DTOs.Request;

public class ConfiguracionGeneralUpdateDto
{
    public string? Nombre { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Ruc { get; set; }
    public decimal? TasaIgvHotel { get; set; }
    public decimal? TasaIgvProductos { get; set; }
    public string? NombreComercial { get; set; }
    public string? CodigoEstablecimiento { get; set; }
    public string? PuntoEmisionBoleta { get; set; }
    public string? PuntoEmisionFactura { get; set; }
    public string? LogoUrl { get; set; }
    public string? Ubigeo { get; set; }
    public string? Departamento { get; set; }
    public string? Provincia { get; set; }
    public string? Distrito { get; set; }
    public string? Urbanizacion { get; set; }
    public bool? AplicaExoneracionAmazonia { get; set; }
    public string? LeyendaAmazonia { get; set; }
    public string? RegimenTributario { get; set; }
}
