using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Hubs;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class HabitacionService : IHabitacionService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<HabitacionService> _logger;
    private readonly IHubContext<HabitacionHub> _hubContext;
    private readonly IAmenidadService _amenidadService;

    public HabitacionService(HotelDbContext db, ILogger<HabitacionService> logger, IHubContext<HabitacionHub> hubContext, IAmenidadService amenidadService)
    {
        _db = db;
        _logger = logger;
        _hubContext = hubContext;
        _amenidadService = amenidadService;
    }

    public async Task<List<Habitacion>> GetAllAsync()
    {
        return await _db.Habitaciones
            .Include(h => h.IdTipoNavigation)
            .Include(h => h.IdEstadoNavigation)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Habitacion?> GetByIdAsync(int id)
    {
        return await _db.Habitaciones
            .Include(h => h.IdTipoNavigation)
            .Include(h => h.IdEstadoNavigation)
            .Include(h => h.HabitacionAmenidades)
                .ThenInclude(ha => ha.IdProductoNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id);
    }

    public async Task<Habitacion> CreateAsync(Habitacion habitacion)
    {
        // Si la habitación viene con amenidades, se deben guardar después
        _db.Habitaciones.Add(habitacion);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Habitación {Numero} creada", habitacion.NumeroHabitacion);
        return habitacion;
    }

    public async Task<Habitacion?> UpdateAsync(int id, Habitacion habitacionActualizada)
    {
        var existente = await _db.Habitaciones
            .Include(h => h.HabitacionAmenidades)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id);
        if (existente == null) return null;

        // Actualizar propiedades básicas
        existente.NumeroHabitacion = habitacionActualizada.NumeroHabitacion;
        existente.Piso = habitacionActualizada.Piso;
        existente.Descripcion = habitacionActualizada.Descripcion;
        existente.IdTipo = habitacionActualizada.IdTipo;
        existente.PrecioNoche = habitacionActualizada.PrecioNoche;
        existente.IdEstado = habitacionActualizada.IdEstado;
        existente.Caracteristicas = habitacionActualizada.Caracteristicas;

        // Las amenidades se actualizan aparte con el método específico
        await _db.SaveChangesAsync();
        _logger.LogInformation("Habitación {Numero} actualizada", existente.NumeroHabitacion);
        return existente;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var habitacion = await _db.Habitaciones.FindAsync(id);
        if (habitacion == null) return false;

        _db.Habitaciones.Remove(habitacion);
        await _db.SaveChangesAsync();
        _logger.LogWarning("Habitación {Numero} eliminada", habitacion.NumeroHabitacion);
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int idHabitacion, int idNuevoEstado, int idUsuario, string? observacion = null)
    {
        var habitacion = await _db.Habitaciones
            .Include(h => h.IdEstadoNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == idHabitacion);

        if (habitacion == null) return false;

        var estadoAnterior = habitacion.IdEstado;
        var estadoNuevo = await _db.EstadosHabitacion.FindAsync(idNuevoEstado);
        if (estadoNuevo == null) return false;

        var transicionValida = await _db.TransicionesEstado
            .AnyAsync(t => t.IdEstadoActual == estadoAnterior && t.IdEstadoSiguiente == idNuevoEstado);

        if (!transicionValida)
        {
            _logger.LogWarning("Transición de estado no permitida: {Anterior} -> {Nuevo} en habitación {Id}",
                estadoAnterior, idNuevoEstado, idHabitacion);
            throw new InvalidOperationException($"No se puede cambiar de estado '{estadoAnterior}' a '{idNuevoEstado}'.");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            habitacion.IdEstado = idNuevoEstado;
            habitacion.FechaUltimoCambio = DateTime.UtcNow;
            habitacion.UsuarioCambio = idUsuario;

            var historial = new HistorialEstadoHabitacion
            {
                IdHabitacion = idHabitacion,
                IdEstadoAnterior = estadoAnterior,
                IdEstadoNuevo = idNuevoEstado,
                FechaCambio = DateTime.UtcNow,
                IdUsuario = idUsuario,
                Observacion = observacion
            };

            _db.HistorialEstadoHabitaciones.Add(historial);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Habitación {Id} cambió de estado {Anterior} a {Nuevo}", idHabitacion, estadoAnterior, idNuevoEstado);

            // 👇 REPOSICIÓN DE AMENIDADES SI LA HABITACIÓN PASA A DISPONIBLE
            if (idNuevoEstado == 1) // 1 = Disponible (según tu semilla)
            {
                await _amenidadService.ReponerStockHabitacionAsync(idHabitacion);
                _logger.LogInformation("Amenidades repuestas para habitación {Id}", idHabitacion);
            }

            // Enviar notificación en tiempo real
            await _hubContext.Clients.All.SendAsync("EstadoHabitacionCambiado", new
            {
                idHabitacion,
                numero = habitacion.NumeroHabitacion,
                nuevoEstado = estadoNuevo.Nombre
            });

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al cambiar estado de habitación {Id}", idHabitacion);
            throw;
        }
    }

    public async Task<List<HabitacionEstadoActualDto>> GetEstadoActualAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        _logger.LogInformation("[Backend-Debug] GetEstadoActualAsync — fecha={Hoy:yyyy-MM-dd}", hoy);

        try
        {
            var habitaciones = await _db.Habitaciones
                .Include(h => h.IdTipoNavigation)
                .Include(h => h.IdEstadoNavigation)
                .Include(h => h.Estancias.Where(e => e.FechaCheckoutReal == null))
                    .ThenInclude(e => e.IdClienteTitularNavigation)
                .Include(h => h.Reservas.Where(r => r.IdEstadoReservaNavigation.Codigo == EstadoReservaCodigo.Code.Confirmada && r.FechaEntradaPrevista >= hoy && r.FechaEntradaPrevista < hoy.AddDays(1)))
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("[Backend-Debug] GetEstadoActualAsync — {Count} habitaciones cargadas", habitaciones.Count);

            // Cargar transiciones permitidas
            var transiciones = await _db.TransicionesEstado.ToListAsync();

            var result = habitaciones.Select(h =>
            {
                var estanciaActiva = h.Estancias.FirstOrDefault(e => e.FechaCheckoutReal == null);
                var reservaHoy = h.Reservas.FirstOrDefault();

                var acciones = new List<string>();

                foreach (var t in transiciones.Where(t => t.IdEstadoActual == h.IdEstado))
                {
                    if (t.IdEstadoSiguiente == EstadoHabitacionCodigo.Ocupada) acciones.Add("CheckIn");
                    else if (t.IdEstadoActual == EstadoHabitacionCodigo.Ocupada && t.IdEstadoSiguiente == EstadoHabitacionCodigo.Limpieza) acciones.Add("CheckOut");
                    else if (t.IdEstadoSiguiente == EstadoHabitacionCodigo.Mantenimiento) acciones.Add("Mantenimiento");
                    else if (t.IdEstadoSiguiente == EstadoHabitacionCodigo.Disponible) acciones.Add("Habilitar");
                    else if (t.IdEstadoSiguiente == EstadoHabitacionCodigo.Bloqueado) acciones.Add("Reservar");
                }

                // CancelarReserva
                if (reservaHoy != null) acciones.Add("CancelarReserva");

                return new HabitacionEstadoActualDto(
                    IdHabitacion: h.IdHabitacion,
                    NumeroHabitacion: h.NumeroHabitacion,
                    Piso: h.Piso,
                    IdTipo: h.IdTipo,
                    NombreTipo: h.Tipo?.Nombre ?? "",
                    PrecioNoche: h.PrecioNoche,
                    IdEstado: h.IdEstado,
                    NombreEstado: h.Estado ?? "",
                    Descripcion: h.Descripcion,
                    IdEstanciaActiva: estanciaActiva?.IdEstancia,
                    ClienteHuesped: estanciaActiva?.ClienteTitular != null
                        ? $"{estanciaActiva.ClienteTitular.Nombres} {estanciaActiva.ClienteTitular.Apellidos}"
                        : null,
                    AccionesDisponibles: acciones,
                    FechaCheckin: estanciaActiva?.FechaCheckin,
                    FechaCheckoutPrevista: estanciaActiva?.FechaCheckoutPrevista,
                    FechaReservaEntrada: reservaHoy?.FechaEntradaPrevista
                );
            }).ToList();

            _logger.LogInformation("[Backend-Debug] GetEstadoActualAsync — {ResultCount} DTOs generados", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Backend-Debug] Error en GetEstadoActualAsync: {Mensaje}", ex.Message);
            throw;
        }
    }

    public async Task<List<HabitacionEstadoActualDto>> GetDisponiblesAsync(DateTime fechaEntrada, DateTime fechaSalida)
    {
        var todas = await GetEstadoActualAsync();
        var idsOcupadas = await _db.Reservas
            .Where(r => r.IdEstadoReservaNavigation.Codigo != EstadoReservaCodigo.Code.Cancelada &&
                        r.FechaEntradaPrevista < fechaSalida &&
                        r.FechaSalidaPrevista > fechaEntrada)
            .Select(r => r.IdHabitacion)
            .Union(_db.Estancias
                .Where(e => e.IdEstadoEstanciaNavigation.Codigo == EstadoEstanciaCodigo.Code.Activa &&
                            e.FechaCheckin < fechaSalida &&
                            e.FechaCheckoutPrevista > fechaEntrada)
                .Select(e => e.IdHabitacion))
            .ToListAsync();

        return todas.Where(h => !idsOcupadas.Contains(h.IdHabitacion)).ToList();
    }

    public async Task<List<HabitacionAmenidad>> GetAmenidadesPorHabitacionAsync(int idHabitacion)
    {
        return await _db.HabitacionAmenidades
            .Include(ha => ha.IdProductoNavigation)
            .Where(ha => ha.IdHabitacion == idHabitacion)
            .ToListAsync();
    }

    public async Task<bool> ActualizarAmenidadesAsync(int idHabitacion, List<HabitacionAmenidadDto> amenidades)
    {
        var existentes = await _db.HabitacionAmenidades
            .Where(ha => ha.IdHabitacion == idHabitacion)
            .ToListAsync();
        _db.HabitacionAmenidades.RemoveRange(existentes);

        var nuevas = amenidades.Select(a => new HabitacionAmenidad
        {
            IdHabitacion = idHabitacion,
            IdProducto = a.IdProducto,
            CantidadBase = a.CantidadBase
        });
        _db.HabitacionAmenidades.AddRange(nuevas);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<string, bool>?> GetCaracteristicasAsync(int idHabitacion)
    {
        var hab = await _db.Habitaciones.FindAsync(idHabitacion);
        if (hab == null || string.IsNullOrEmpty(hab.Caracteristicas))
            return null;
        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(hab.Caracteristicas);
    }

    public async Task<bool> ActualizarCaracteristicasAsync(int idHabitacion, Dictionary<string, bool> caracteristicas)
    {
        var hab = await _db.Habitaciones.FindAsync(idHabitacion);
        if (hab == null) return false;
        hab.Caracteristicas = System.Text.Json.JsonSerializer.Serialize(caracteristicas);
        await _db.SaveChangesAsync();
        return true;
    }

}
