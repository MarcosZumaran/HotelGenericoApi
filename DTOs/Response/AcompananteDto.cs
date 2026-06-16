namespace HotelGenericoApi.DTOs.Response;

public sealed record AcompananteDto(
    int IdHuesped,
    int IdCliente,
    string NombreCompleto,
    string? Documento
);
