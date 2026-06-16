namespace HotelGenericoApi.DTOs.Response;

public sealed record FolioEstanciaDto(
    int IdEstancia,
    string NumeroHabitacion,
    string Cliente,
    DateTime FechaCheckin,
    DateTime FechaSalidaPrevista,
    decimal MontoEstancia,
    string ModalidadCobro,
    decimal TotalPagado,
    decimal SaldoPendiente,
    List<FolioItemDto> Items,
    List<FolioPagoDto> Pagos
);

public sealed record FolioItemDto(
    int IdItem,
    string Concepto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal,
    DateTime? FechaRegistro,
    string Tipo
);

public sealed record FolioPagoDto(
    int IdPago,
    decimal Monto,
    string MetodoPago,
    DateTime FechaPago,
    string? Concepto
);
