namespace HotelGenericoApi.DTOs.Response;

public sealed record EstanciaResponseDto(
    int IdEstancia,
    int? IdHabitacion,
    string? NumeroHabitacion,
    int? IdClienteTitular,
    string? ClienteNombreCompleto,
    DateTime FechaCheckin,
    DateTime FechaCheckoutPrevista,
    DateTime? FechaCheckoutReal,
    decimal MontoTotal,
    string? Estado,
    DateTime? CreatedAt,
    bool EstaFuera,
    DateTime? HoraSalidaTemporal,
    DateTime? HoraRegresoTemporal,
    bool? LlavesDejadas
);
