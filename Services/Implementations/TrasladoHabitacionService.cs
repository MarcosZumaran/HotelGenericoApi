using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Hubs;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class TrasladoHabitacionService : ITrasladoHabitacionService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<TrasladoHabitacionService> _logger;
    private readonly IHubContext<HabitacionHub> _hubContext;

    public TrasladoHabitacionService(
        HotelDbContext db,
        ILogger<TrasladoHabitacionService> logger,
        IHubContext<HabitacionHub> hubContext)
    {
        _db = db;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<TrasladoResultDto> TrasladarHabitacionAsync(int estanciaId, TrasladarEstanciaDto dto, int idUsuario)
    {
        var estancia = await _db.Estancias
            .Include(e => e.IdHabitacionNavigation)
            .Include(e => e.IdEstadoEstanciaNavigation)
            .FirstOrDefaultAsync(e => e.IdEstancia == estanciaId);

        if (estancia == null)
            throw new ArgumentException("Estancia no encontrada.");
        if (estancia.FechaCheckoutReal != null)
            throw new InvalidOperationException("La estancia ya está finalizada, no se puede trasladar.");
        if (estancia.Estado != EstadoEstanciaCodigo.Code.Activa)
            throw new InvalidOperationException("La estancia no está activa.");

        var habitacionOrigen = estancia.Habitacion;
        if (habitacionOrigen == null)
            throw new Exception("Habitación origen no encontrada.");

        var nuevaHabitacion = await _db.Habitaciones
            .Include(h => h.IdEstadoNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == dto.NuevaHabitacionId);
        if (nuevaHabitacion == null)
            throw new ArgumentException("Nueva habitación no encontrada.");
        if (nuevaHabitacion.IdEstado != EstadoHabitacionCodigo.Disponible)
            throw new InvalidOperationException($"La habitación {nuevaHabitacion.NumeroHabitacion} no está disponible.");

        var estanciaSuperpuesta = await _db.Estancias
            .AnyAsync(e => e.IdHabitacion == dto.NuevaHabitacionId &&
                        e.IdEstadoEstanciaNavigation.Codigo == EstadoEstanciaCodigo.Code.Activa &&
                        e.FechaCheckin < estancia.FechaCheckoutPrevista &&
                        e.FechaCheckoutPrevista > estancia.FechaCheckin);
        if (estanciaSuperpuesta)
            throw new InvalidOperationException("La nueva habitación ya está ocupada en el período de la estancia.");

        decimal montoAnterior = estancia.MontoTotal;
        var nochesRestantes = Math.Max(1, (int)(estancia.FechaCheckoutPrevista.Date - DateTime.UtcNow.Date).TotalDays);
        decimal diferencia = 0;

        if (nuevaHabitacion.PrecioNoche > habitacionOrigen.PrecioNoche)
        {
            diferencia = (nuevaHabitacion.PrecioNoche - habitacionOrigen.PrecioNoche) * nochesRestantes;

            if (dto.CobrarDiferencia)
            {
                var productoTraslado = await _db.Productos
                    .FirstOrDefaultAsync(p => p.Nombre == "Diferencia por traslado")
                    ?? await _db.Productos.FirstAsync();

                _db.ItemsEstancia.Add(new ItemEstancia
                {
                    IdEstancia = estanciaId,
                    IdProducto = productoTraslado.IdProducto,
                    Cantidad = 1,
                    PrecioUnitario = diferencia,
                    Subtotal = diferencia,
                    FechaRegistro = DateTime.UtcNow
                });

                estancia.MontoTotal = montoAnterior + diferencia;
            }
        }

        decimal nuevoMonto = estancia.MontoTotal;
        decimal ajuste = nuevoMonto - montoAnterior;

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            habitacionOrigen.IdEstado = EstadoHabitacionCodigo.Limpieza;
            habitacionOrigen.FechaUltimoCambio = DateTime.UtcNow;
            habitacionOrigen.UsuarioCambio = idUsuario;

            _db.HistorialEstadoHabitaciones.Add(new HistorialEstadoHabitacion
            {
                IdHabitacion = habitacionOrigen.IdHabitacion,
                IdEstadoAnterior = EstadoHabitacionCodigo.Ocupada,
                IdEstadoNuevo = EstadoHabitacionCodigo.Limpieza,
                FechaCambio = DateTime.UtcNow,
                IdUsuario = idUsuario,
                Observacion = $"Traslado a habitación {nuevaHabitacion.NumeroHabitacion}"
            });

            nuevaHabitacion.IdEstado = EstadoHabitacionCodigo.Ocupada;
            nuevaHabitacion.FechaUltimoCambio = DateTime.UtcNow;
            nuevaHabitacion.UsuarioCambio = idUsuario;

            _db.HistorialEstadoHabitaciones.Add(new HistorialEstadoHabitacion
            {
                IdHabitacion = nuevaHabitacion.IdHabitacion,
                IdEstadoAnterior = EstadoHabitacionCodigo.Disponible,
                IdEstadoNuevo = EstadoHabitacionCodigo.Ocupada,
                FechaCambio = DateTime.UtcNow,
                IdUsuario = idUsuario,
                Observacion = $"Traslado desde habitación {habitacionOrigen.NumeroHabitacion}"
            });

            estancia.IdHabitacion = nuevaHabitacion.IdHabitacion;

            var historial = new HistorialTraslado
            {
                IdEstancia = estanciaId,
                IdHabitacionOrigen = habitacionOrigen.IdHabitacion,
                IdHabitacionDestino = nuevaHabitacion.IdHabitacion,
                Motivo = dto.Motivo,
                FechaTraslado = DateTime.UtcNow,
                UsuarioId = idUsuario,
                AjusteMonto = ajuste
            };
            _db.HistorialTraslados.Add(historial);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients.All.SendAsync("EstadoHabitacionCambiado", new
            {
                idHabitacion = habitacionOrigen.IdHabitacion,
                numero = habitacionOrigen.NumeroHabitacion,
                nuevoEstado = "Limpieza"
            });

            await _hubContext.Clients.All.SendAsync("EstadoHabitacionCambiado", new
            {
                idHabitacion = nuevaHabitacion.IdHabitacion,
                numero = nuevaHabitacion.NumeroHabitacion,
                nuevoEstado = "Ocupada"
            });

            await _hubContext.Clients.All.SendAsync("EstanciaTrasladada", new
            {
                idEstancia = estanciaId,
                habitacionOrigen = habitacionOrigen.NumeroHabitacion,
                habitacionDestino = nuevaHabitacion.NumeroHabitacion,
                ajuste = ajuste
            });

            _logger.LogInformation("Estancia {IdEstancia} trasladada de habitación {Origen} a {Destino}. Ajuste: {Ajuste}",
                estanciaId, habitacionOrigen.NumeroHabitacion, nuevaHabitacion.NumeroHabitacion, ajuste);

            return new TrasladoResultDto
            {
                IdEstancia = estanciaId,
                HabitacionOrigenId = habitacionOrigen.IdHabitacion,
                HabitacionOrigenNumero = habitacionOrigen.NumeroHabitacion,
                HabitacionDestinoId = nuevaHabitacion.IdHabitacion,
                HabitacionDestinoNumero = nuevaHabitacion.NumeroHabitacion,
                MontoAnterior = montoAnterior,
                MontoNuevo = nuevoMonto,
                Ajuste = ajuste,
                Motivo = dto.Motivo,
                DiferenciaCobrada = diferencia,
                NochesRestantes = nochesRestantes
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al trasladar estancia {IdEstancia}", estanciaId);
            throw;
        }
    }
}
