namespace HotelGenericoApi.DTOs.Response;

public sealed record ConfiguracionHotelResponseDto(
    string Nombre,
    string? Direccion,
    string? Telefono,
    string? Ruc,
    decimal TasaIgvHotel,
    decimal TasaIgvProductos,
    string? NombreComercial,
    string? CodigoEstablecimiento,
    string? PuntoEmisionBoleta,
    string? PuntoEmisionFactura,
    string? LogoUrl,
    string? Ubigeo,
    string? Departamento,
    string? Provincia,
    string? Distrito,
    string? Urbanizacion,
    bool? AplicaExoneracionAmazonia,
    string? LeyendaAmazonia,
    string? RegimenTributario
);
