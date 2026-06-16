using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Hubs;
using HotelGenericoApi.Models;
using HotelGenericoApi.Extensions;
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

    public async Task<PagedResult<Habitacion>> GetPagedAsync(int page, int pageSize)
    {
        var query = _db.Habitaciones
            .Include(h => h.IdTipoNavigation)
            .Include(h => h.IdEstadoNavigation)
            .AsNoTracking();
        return await query.ToPagedResultAsync(page, pageSize);
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

    public async Task<Habitacion> CreateAsync(HabitacionCreateDto dto)
    {
        var habitacion = new Habitacion
        {
            NumeroHabitacion = dto.NumeroHabitacion,
            Piso = dto.Piso ?? 0,
            Descripcion = dto.Descripcion,
            IdTipo = dto.IdTipo,
            PrecioNoche = dto.PrecioNoche,
            IdEstado = dto.IdEstado ?? 1,
            FechaUltimoCambio = DateTime.UtcNow,
            Caracteristicas = dto.Caracteristicas != null
                ? System.Text.Json.JsonSerializer.Serialize(dto.Caracteristicas)
                : null
        };

        _db.Habitaciones.Add(habitacion);
        await _db.SaveChangesAsync();

        if (dto.Amenidades != null && dto.Amenidades.Count != 0)
        {
            var amenidades = dto.Amenidades.Select(a => new HabitacionAmenidad
            {
                IdHabitacion = habitacion.IdHabitacion,
                IdProducto = a.IdProducto,
                CantidadBase = a.CantidadBase
            });
            await _db.HabitacionAmenidades.AddRangeAsync(amenidades);
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("Habitación {Numero} creada", habitacion.NumeroHabitacion);
        return habitacion;
    }

    public async Task<Habitacion?> UpdateAsync(int id, HabitacionUpdateDto dto)
    {
        var existente = await _db.Habitaciones
            .Include(h => h.HabitacionAmenidades)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id);
        if (existente == null) return null;

        if (dto.NumeroHabitacion != null) existente.NumeroHabitacion = dto.NumeroHabitacion;
        if (dto.Piso.HasValue) existente.Piso = dto.Piso.Value;
        if (dto.Descripcion != null) existente.Descripcion = dto.Descripcion;
        if (dto.IdTipo.HasValue) existente.IdTipo = dto.IdTipo.Value;
        if (dto.PrecioNoche.HasValue) existente.PrecioNoche = dto.PrecioNoche.Value;
        if (dto.Caracteristicas != null)
            existente.Caracteristicas = System.Text.Json.JsonSerializer.Serialize(dto.Caracteristicas);

        if (dto.Amenidades != null)
        {
            var existentes = existente.HabitacionAmenidades?.ToList() ?? new List<HabitacionAmenidad>();
            if (existentes.Count != 0)
                _db.HabitacionAmenidades.RemoveRange(existentes);

            var nuevas = dto.Amenidades.Select(a => new HabitacionAmenidad
            {
                IdHabitacion = id,
                IdProducto = a.IdProducto,
                CantidadBase = a.CantidadBase
            });
            await _db.HabitacionAmenidades.AddRangeAsync(nuevas);
        }

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
            .Include(h => h.IdTipoNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == idHabitacion);

        if (habitacion == null) return false;

        var estadoAnterior = habitacion.IdEstado;
        var estadoNuevo = await _db.EstadosHabitacion.FindAsync(idNuevoEstado);
        if (estadoNuevo == null) return false;

        var transicionValida = await _db.TransicionesEstado
            .AnyAsync(t => t.IdEstadoActual == estadoAnterior && t.IdEstadoSiguiente == idNuevoEstado);

        if (!transicionValida)
        {
            // Permitir Limpieza -> Disponible (limpieza completada por el panel de limpieza)
            if (estadoAnterior == EstadoHabitacionCodigo.Limpieza && idNuevoEstado == EstadoHabitacionCodigo.Disponible)
            {
                _logger.LogInformation(
                    "Transición Limpieza -> Disponible permitida (bypass) para habitación {Id}", idHabitacion);
            }
            else
            {
                _logger.LogWarning("Transición de estado no permitida: {Anterior} -> {Nuevo} en habitación {Id}",
                    estadoAnterior, idNuevoEstado, idHabitacion);
                throw new InvalidOperationException($"No se puede cambiar de estado '{estadoAnterior}' a '{idNuevoEstado}'.");
            }
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
            if (idNuevoEstado == 1) // 1 = Disponible
            {
                var repuestas = await _amenidadService.ReponerAmenidadesHabitacionAsync(idHabitacion, idUsuario);
                _logger.LogInformation("Amenidades repuestas para habitación {Id}: {Count} productos", idHabitacion, repuestas);

                // T1.1: Emitir evento específico HabitacionLista para notificar a recepción
                await _hubContext.Clients.All.SendAsync("HabitacionLista", new
                {
                    idHabitacion,
                    numeroHabitacion = habitacion.NumeroHabitacion,
                    tipoHabitacion = habitacion.Tipo?.Nombre ?? ""
                });
            }

            // Enviar notificación en tiempo real del cambio de estado genérico
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

    public async Task<HabitacionSugeridaDto?> SugerirDisponibleAsync(int? tipoHabitacion = null, int? piso = null, int? cercanaA = null)
    {
        _logger.LogInformation("Sugiriendo habitación disponible (tipo={Tipo}, piso={Piso}, cercanaA={Cercana})",
            tipoHabitacion, piso, cercanaA);

        var disponibles = _db.Habitaciones
            .Include(h => h.IdTipoNavigation)
            .Where(h => h.IdEstado == EstadoHabitacionCodigo.Disponible)
            .AsNoTracking();

        if (tipoHabitacion.HasValue)
            disponibles = disponibles.Where(h => h.IdTipo == tipoHabitacion.Value);

        IOrderedQueryable<Habitacion> ordenadas;
        if (piso.HasValue)
        {
            ordenadas = disponibles.OrderBy(h => h.Piso == piso.Value ? 0 : 1)
                                   .ThenBy(h => h.Piso)
                                   .ThenBy(h => h.NumeroHabitacion);
        }
        else if (cercanaA.HasValue)
        {
            ordenadas = disponibles.OrderBy(h => Math.Abs((long)(h.IdHabitacion - cercanaA.Value)))
                                   .ThenBy(h => h.Piso)
                                   .ThenBy(h => h.NumeroHabitacion);
        }
        else
        {
            ordenadas = disponibles.OrderBy(h => h.Piso)
                                   .ThenBy(h => h.NumeroHabitacion);
        }

        var sugerida = await ordenadas.FirstOrDefaultAsync();
        if (sugerida == null) return null;

        return new HabitacionSugeridaDto(
            IdHabitacion: sugerida.IdHabitacion,
            Numero: sugerida.NumeroHabitacion,
            NombreTipo: sugerida.Tipo?.Nombre ?? "",
            Piso: sugerida.Piso,
            PrecioNoche: sugerida.PrecioNoche,
            Capacidad: sugerida.Tipo?.Capacidad ?? 0
        );
    }

    public async Task<List<HabitacionEstadoActualDetalladoDto>> GetEstadoActualDetalladoAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        _logger.LogInformation("GetEstadoActualDetalladoAsync — fecha={Hoy:yyyy-MM-dd}", hoy);

        try
        {
            var habitaciones = await _db.Habitaciones
                .Include(h => h.IdTipoNavigation)
                .Include(h => h.IdEstadoNavigation)
                .Include(h => h.Estancias.Where(e => e.FechaCheckoutReal == null))
                    .ThenInclude(e => e.IdClienteTitularNavigation)
                .Include(h => h.Reservas.Where(r =>
                    (r.IdEstadoReservaNavigation.Codigo == EstadoReservaCodigo.Code.Confirmada ||
                     r.IdEstadoReservaNavigation.Codigo == EstadoReservaCodigo.Code.Pendiente) &&
                    r.FechaEntradaPrevista >= hoy &&
                    r.FechaEntradaPrevista < hoy.AddDays(1)))
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("GetEstadoActualDetalladoAsync — {Count} habitaciones cargadas", habitaciones.Count);

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
                if (reservaHoy != null) acciones.Add("CancelarReserva");

                // Calcular minutos en limpieza
                var minutosEnLimpieza = h.IdEstado == EstadoHabitacionCodigo.Limpieza && h.FechaUltimoCambio != default
                    ? (int)Math.Floor((DateTime.UtcNow - h.FechaUltimoCambio).TotalMinutes)
                    : 0;

                // Calcular prioridad
                string prioridad;
                if (h.IdEstado == EstadoHabitacionCodigo.Limpieza && reservaHoy != null)
                    prioridad = "salida";
                else if (h.IdEstado == EstadoHabitacionCodigo.Limpieza)
                    prioridad = "normal";
                else
                    prioridad = "normal";

                return new HabitacionEstadoActualDetalladoDto(
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
                    FechaReservaEntrada: reservaHoy?.FechaEntradaPrevista,
                    MinutosEnLimpieza: minutosEnLimpieza,
                    Prioridad: prioridad
                );
            }).ToList();

            _logger.LogInformation("GetEstadoActualDetalladoAsync — {ResultCount} DTOs generados", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetEstadoActualDetalladoAsync: {Mensaje}", ex.Message);
            throw;
        }
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
