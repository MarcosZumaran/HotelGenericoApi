namespace HotelGenericoApi.DTOs.Response;

public sealed record HabitacionEstadoActualDetalladoDto(
    int IdHabitacion,
    string NumeroHabitacion,
    int? Piso,
    int IdTipo,
    string NombreTipo,
    decimal PrecioNoche,
    int IdEstado,
    string NombreEstado,
    string? Descripcion,
    int? IdEstanciaActiva,
    string? ClienteHuesped,
    List<string> AccionesDisponibles,
    DateTime? FechaCheckin,
    DateTime? FechaCheckoutPrevista,
    DateTime? FechaReservaEntrada,
    int MinutosEnLimpieza,
    string Prioridad
);
