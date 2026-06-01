using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class IncidenteService : IIncidenteService
{
    private readonly HotelDbContext _db;

    public IncidenteService(HotelDbContext db)
    {
        _db = db;
    }

    //  INCIDENTES
    public async Task<IEnumerable<IncidenteResponseDto>> GetAllIncidentesAsync()
    {
        return await _db.Incidentes
            .Include(i => i.Habitacion)
            .Include(i => i.UsuarioReporte)
            .OrderByDescending(i => i.FechaRegistro)
            .Select(i => new IncidenteResponseDto
            {
                IdIncidente = i.IdIncidente,
                IdEstancia = i.IdEstancia,
                IdHabitacion = i.IdHabitacion,
                NumeroHabitacion = i.Habitacion != null ? i.Habitacion.NumeroHabitacion : "",
                Tipo = i.Tipo,
                Descripcion = i.Descripcion,
                CostoEstimado = i.CostoEstimado,
                CobradoAlCliente = i.CobradoAlCliente,
                Resuelto = i.Resuelto,
                FechaRegistro = i.FechaRegistro,
                ReportadoPorNombre = i.UsuarioReporte != null ? i.UsuarioReporte.Username : null
            })
            .ToListAsync();
    }

    public async Task<IncidenteResponseDto?> GetIncidenteByIdAsync(int id)
    {
        var i = await _db.Incidentes
            .Include(i => i.Habitacion)
            .Include(i => i.UsuarioReporte)
            .FirstOrDefaultAsync(i => i.IdIncidente == id);

        if (i == null) return null;

        return new IncidenteResponseDto
        {
            IdIncidente = i.IdIncidente,
            IdEstancia = i.IdEstancia,
            IdHabitacion = i.IdHabitacion,
            NumeroHabitacion = i.Habitacion != null ? i.Habitacion.NumeroHabitacion : "",
            Tipo = i.Tipo,
            Descripcion = i.Descripcion,
            CostoEstimado = i.CostoEstimado,
            CobradoAlCliente = i.CobradoAlCliente,
            Resuelto = i.Resuelto,
            FechaRegistro = i.FechaRegistro,
            ReportadoPorNombre = i.UsuarioReporte != null ? i.UsuarioReporte.Username : null
        };
    }

    public async Task<IEnumerable<IncidenteResponseDto>> GetIncidentesByHabitacionAsync(int idHabitacion)
    {
        return await _db.Incidentes
            .Include(i => i.Habitacion)
            .Include(i => i.UsuarioReporte)
            .Where(i => i.IdHabitacion == idHabitacion)
            .OrderByDescending(i => i.FechaRegistro)
            .Select(i => new IncidenteResponseDto
            {
                IdIncidente = i.IdIncidente,
                IdEstancia = i.IdEstancia,
                IdHabitacion = i.IdHabitacion,
                NumeroHabitacion = i.Habitacion != null ? i.Habitacion.NumeroHabitacion : "",
                Tipo = i.Tipo,
                Descripcion = i.Descripcion,
                CostoEstimado = i.CostoEstimado,
                CobradoAlCliente = i.CobradoAlCliente,
                Resuelto = i.Resuelto,
                FechaRegistro = i.FechaRegistro,
                ReportadoPorNombre = i.UsuarioReporte != null ? i.UsuarioReporte.Username : null
            })
            .ToListAsync();
    }

    public async Task<IncidenteResponseDto> CreateIncidenteAsync(IncidenteCreateDto dto, int idUsuario)
    {
        // Validar que la habitación exista
        var habitacion = await _db.Habitaciones.FindAsync(dto.IdHabitacion);
        if (habitacion == null)
            throw new ArgumentException("Habitación no encontrada.");

        var incidente = new Incidente
        {
            IdEstancia = dto.IdEstancia,
            IdHabitacion = dto.IdHabitacion,
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion,
            CostoEstimado = dto.CostoEstimado,
            CobradoAlCliente = dto.CobradoAlCliente,
            Resuelto = false,
            FechaRegistro = DateTime.UtcNow,
            ReportadoPor = idUsuario
        };

        _db.Incidentes.Add(incidente);
        await _db.SaveChangesAsync();

        return (await GetIncidenteByIdAsync(incidente.IdIncidente))!;
    }

    public async Task<bool> ResolverIncidenteAsync(int id)
    {
        var incidente = await _db.Incidentes.FindAsync(id);
        if (incidente == null) return false;

        incidente.Resuelto = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarcarCobradoAsync(int id, bool cobrado)
    {
        var incidente = await _db.Incidentes.FindAsync(id);
        if (incidente == null) return false;

        incidente.CobradoAlCliente = cobrado;
        await _db.SaveChangesAsync();
        return true;
    }

    // OBJETOS PERDIDOS
    public async Task<IEnumerable<ObjetoPerdidoResponseDto>> GetAllObjetosPerdidosAsync()
    {
        return await _db.ObjetosPerdidos
            .Include(o => o.Habitacion)
            .Include(o => o.Estancia)
            .OrderByDescending(o => o.FechaHallazgo)
            .Select(o => new ObjetoPerdidoResponseDto
            {
                IdObjeto = o.IdObjeto,
                IdHabitacion = o.IdHabitacion,
                NumeroHabitacion = o.Habitacion != null ? o.Habitacion.NumeroHabitacion : null,
                IdEstancia = o.IdEstancia,
                Descripcion = o.Descripcion,
                FechaHallazgo = o.FechaHallazgo,
                Estado = o.Estado,
                EntregadoA = o.EntregadoA,
                FechaEntregado = o.FechaEntregado
            })
            .ToListAsync();
    }

    public async Task<ObjetoPerdidoResponseDto?> GetObjetoPerdidoByIdAsync(int id)
    {
        var o = await _db.ObjetosPerdidos
            .Include(o => o.Habitacion)
            .Include(o => o.Estancia)
            .FirstOrDefaultAsync(o => o.IdObjeto == id);

        if (o == null) return null;

        return new ObjetoPerdidoResponseDto
        {
            IdObjeto = o.IdObjeto,
            IdHabitacion = o.IdHabitacion,
            NumeroHabitacion = o.Habitacion != null ? o.Habitacion.NumeroHabitacion : null,
            IdEstancia = o.IdEstancia,
            Descripcion = o.Descripcion,
            FechaHallazgo = o.FechaHallazgo,
            Estado = o.Estado,
            EntregadoA = o.EntregadoA,
            FechaEntregado = o.FechaEntregado
        };
    }

    public async Task<IEnumerable<ObjetoPerdidoResponseDto>> GetObjetosPendientesAsync()
    {
        return await _db.ObjetosPerdidos
            .Include(o => o.Habitacion)
            .Where(o => o.Estado == "pendiente")
            .OrderByDescending(o => o.FechaHallazgo)
            .Select(o => new ObjetoPerdidoResponseDto
            {
                IdObjeto = o.IdObjeto,
                IdHabitacion = o.IdHabitacion,
                NumeroHabitacion = o.Habitacion != null ? o.Habitacion.NumeroHabitacion : null,
                IdEstancia = o.IdEstancia,
                Descripcion = o.Descripcion,
                FechaHallazgo = o.FechaHallazgo,
                Estado = o.Estado,
                EntregadoA = o.EntregadoA,
                FechaEntregado = o.FechaEntregado
            })
            .ToListAsync();
    }

    public async Task<ObjetoPerdidoResponseDto> CreateObjetoPerdidoAsync(ObjetoPerdidoCreateDto dto)
    {
        var objeto = new ObjetoPerdido
        {
            IdHabitacion = dto.IdHabitacion,
            IdEstancia = dto.IdEstancia,
            Descripcion = dto.Descripcion,
            FechaHallazgo = DateTime.UtcNow,
            Estado = "pendiente"
        };

        _db.ObjetosPerdidos.Add(objeto);
        await _db.SaveChangesAsync();

        return (await GetObjetoPerdidoByIdAsync(objeto.IdObjeto))!;
    }

    public async Task<bool> EntregarObjetoAsync(int id, string entregadoA)
    {
        var objeto = await _db.ObjetosPerdidos.FindAsync(id);
        if (objeto == null) return false;

        objeto.Estado = "entregado";
        objeto.EntregadoA = entregadoA;
        objeto.FechaEntregado = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesecharObjetoAsync(int id)
    {
        var objeto = await _db.ObjetosPerdidos.FindAsync(id);
        if (objeto == null) return false;

        objeto.Estado = "desechado";
        await _db.SaveChangesAsync();
        return true;
    }
}
