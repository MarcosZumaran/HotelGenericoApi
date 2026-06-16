namespace HotelGenericoApi.DTOs.Response;

public sealed record ReniecRucResponseDto(
    string Ruc,
    string BusinessName,
    string TaxpayerType,
    string Status,
    string Condition,
    string Address,
    string Source,
    DateTime? UpdatedAt
);
