namespace HotelGenericoApi.DTOs.Response;

public sealed record EstanciaActivaDto(
    int IdEstancia,
    int IdHabitacion,
    string NumeroHabitacion,
    int IdClienteTitular,
    string ClienteNombreCompleto,
    string? ClienteDocumento,
    DateTime FechaCheckin,
    DateTime FechaCheckoutPrevista,
    DateTime? FechaCheckoutReal,
    decimal MontoTotal,
    string? Estado,
    DateTime? CreatedAt,
    bool EstaFuera,
    DateTime? HoraSalidaTemporal,
    DateTime? HoraRegresoTemporal,
    bool? LlavesDejadas,
    List<AcompananteDto> Acompanantes
);
