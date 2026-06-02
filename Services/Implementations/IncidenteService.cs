using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;
using ImageMagick;

namespace HotelGenericoApi.Services.Implementations;

public class IncidenteService : IIncidenteService
{
    private readonly HotelDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public IncidenteService(HotelDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    // INCIDENTES

    public async Task<IEnumerable<IncidenteResponseDto>> GetAllIncidentesAsync()
    {
        return await _db.Incidentes
            .Include(i => i.IdHabitacionNavigation)
            .Include(i => i.ReportadoPorNavigation)
            .OrderByDescending(i => i.FechaRegistro)
            .Select(i => new IncidenteResponseDto
            {
                IdIncidente = i.IdIncidente,
                IdEstancia = i.IdEstancia,
                IdHabitacion = i.IdHabitacion,
                NumeroHabitacion = i.IdHabitacionNavigation != null ? i.IdHabitacionNavigation.NumeroHabitacion : "",
                Tipo = i.Tipo,
                Descripcion = i.Descripcion,
                CostoEstimado = i.CostoEstimado,
                CobradoAlCliente = i.CobradoAlCliente,
                Resuelto = i.Resuelto,
                FechaRegistro = i.FechaRegistro,
                ReportadoPorNombre = i.ReportadoPorNavigation != null ? i.ReportadoPorNavigation.Username : null,
                ImagenUrl = i.ImagenUrl
            })
            .ToListAsync();
    }

    public async Task<IncidenteResponseDto?> GetIncidenteByIdAsync(int id)
    {
        var i = await _db.Incidentes
            .Include(i => i.IdHabitacionNavigation)
            .Include(i => i.ReportadoPorNavigation)
            .FirstOrDefaultAsync(i => i.IdIncidente == id);

        if (i == null) return null;

        return new IncidenteResponseDto
        {
            IdIncidente = i.IdIncidente,
            IdEstancia = i.IdEstancia,
            IdHabitacion = i.IdHabitacion,
            NumeroHabitacion = i.IdHabitacionNavigation != null ? i.IdHabitacionNavigation.NumeroHabitacion : "",
            Tipo = i.Tipo,
            Descripcion = i.Descripcion,
            CostoEstimado = i.CostoEstimado,
            CobradoAlCliente = i.CobradoAlCliente,
            Resuelto = i.Resuelto,
            FechaRegistro = i.FechaRegistro,
            ReportadoPorNombre = i.ReportadoPorNavigation != null ? i.ReportadoPorNavigation.Username : null,
            ImagenUrl = i.ImagenUrl
        };
    }

    public async Task<IEnumerable<IncidenteResponseDto>> GetIncidentesByHabitacionAsync(int idHabitacion)
    {
        return await _db.Incidentes
            .Include(i => i.IdHabitacionNavigation)
            .Include(i => i.ReportadoPorNavigation)
            .Where(i => i.IdHabitacion == idHabitacion)
            .OrderByDescending(i => i.FechaRegistro)
            .Select(i => new IncidenteResponseDto
            {
                IdIncidente = i.IdIncidente,
                IdEstancia = i.IdEstancia,
                IdHabitacion = i.IdHabitacion,
                NumeroHabitacion = i.IdHabitacionNavigation != null ? i.IdHabitacionNavigation.NumeroHabitacion : "",
                Tipo = i.Tipo,
                Descripcion = i.Descripcion,
                CostoEstimado = i.CostoEstimado,
                CobradoAlCliente = i.CobradoAlCliente,
                Resuelto = i.Resuelto,
                FechaRegistro = i.FechaRegistro,
                ReportadoPorNombre = i.ReportadoPorNavigation != null ? i.ReportadoPorNavigation.Username : null,
                ImagenUrl = i.ImagenUrl
            })
            .ToListAsync();
    }

    public async Task<IncidenteResponseDto> CreateIncidenteAsync(IncidenteCreateDto dto, int idUsuario, IFormFile? imagen)
    {
        var habitacion = await _db.Habitaciones.FindAsync(dto.IdHabitacion);
        if (habitacion == null)
            throw new ArgumentException("Habitación no encontrada.");

        var imagenUrl = await GuardarImagenAsync(imagen, "incidentes");

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
            ReportadoPor = idUsuario,
            ImagenUrl = imagenUrl
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
            .Include(o => o.IdHabitacionNavigation)
            .Include(o => o.IdEstanciaNavigation)
            .OrderByDescending(o => o.FechaHallazgo)
            .Select(o => new ObjetoPerdidoResponseDto
            {
                IdObjeto = o.IdObjeto,
                IdHabitacion = o.IdHabitacion,
                NumeroHabitacion = o.IdHabitacionNavigation != null ? o.IdHabitacionNavigation.NumeroHabitacion : null,
                IdEstancia = o.IdEstancia,
                Descripcion = o.Descripcion,
                FechaHallazgo = o.FechaHallazgo,
                Estado = o.Estado,
                EntregadoA = o.EntregadoA,
                FechaEntregado = o.FechaEntregado,
                ImagenUrl = o.ImagenUrl
            })
            .ToListAsync();
    }

    public async Task<ObjetoPerdidoResponseDto?> GetObjetoPerdidoByIdAsync(int id)
    {
        var o = await _db.ObjetosPerdidos
            .Include(o => o.IdHabitacionNavigation)
            .Include(o => o.IdEstanciaNavigation)
            .FirstOrDefaultAsync(o => o.IdObjeto == id);

        if (o == null) return null;

        return new ObjetoPerdidoResponseDto
        {
            IdObjeto = o.IdObjeto,
            IdHabitacion = o.IdHabitacion,
            NumeroHabitacion = o.IdHabitacionNavigation != null ? o.IdHabitacionNavigation.NumeroHabitacion : null,
            IdEstancia = o.IdEstancia,
            Descripcion = o.Descripcion,
            FechaHallazgo = o.FechaHallazgo,
            Estado = o.Estado,
            EntregadoA = o.EntregadoA,
            FechaEntregado = o.FechaEntregado,
            ImagenUrl = o.ImagenUrl
        };
    }

    public async Task<IEnumerable<ObjetoPerdidoResponseDto>> GetObjetosPendientesAsync()
    {
        return await _db.ObjetosPerdidos
            .Include(o => o.IdHabitacionNavigation)
            .Where(o => o.Estado == "pendiente")
            .OrderByDescending(o => o.FechaHallazgo)
            .Select(o => new ObjetoPerdidoResponseDto
            {
                IdObjeto = o.IdObjeto,
                IdHabitacion = o.IdHabitacion,
                NumeroHabitacion = o.IdHabitacionNavigation != null ? o.IdHabitacionNavigation.NumeroHabitacion : null,
                IdEstancia = o.IdEstancia,
                Descripcion = o.Descripcion,
                FechaHallazgo = o.FechaHallazgo,
                Estado = o.Estado,
                EntregadoA = o.EntregadoA,
                FechaEntregado = o.FechaEntregado,
                ImagenUrl = o.ImagenUrl
            })
            .ToListAsync();
    }

    public async Task<ObjetoPerdidoResponseDto> CreateObjetoPerdidoAsync(ObjetoPerdidoCreateDto dto, IFormFile? imagen)
    {
        var imagenUrl = await GuardarImagenAsync(imagen, "objetos");

        var objeto = new ObjetoPerdido
        {
            IdHabitacion = dto.IdHabitacion,
            IdEstancia = dto.IdEstancia,
            Descripcion = dto.Descripcion,
            FechaHallazgo = DateTime.UtcNow,
            Estado = "pendiente",
            ImagenUrl = imagenUrl
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

    // MÉTODOS PRIVADOS

    private async Task<string?> GuardarImagenAsync(IFormFile? file, string subcarpeta)
    {
        if (file == null || file.Length == 0) return null;

        var extensiones = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!extensiones.Contains(ext))
            throw new InvalidOperationException("Formato de imagen no permitido. Use .jpg, .jpeg, .png o .webp.");

        var nombreArchivo = $"{Guid.NewGuid()}.webp";
        var rutaRelativa = Path.Combine("imagenes", subcarpeta, nombreArchivo);
        var rutaCompleta = Path.Combine(_webHostEnvironment.WebRootPath, rutaRelativa);
        Directory.CreateDirectory(Path.GetDirectoryName(rutaCompleta)!);

        using var stream = file.OpenReadStream();
        using var image = new MagickImage(stream);

        if (image.Width > 800)
        {
            var geometry = new MagickGeometry(800);
            image.Resize(geometry);
        }

        image.Quality = 80;
        image.Format = MagickFormat.WebP;
        await image.WriteAsync(rutaCompleta);

        return $"/{rutaRelativa.Replace(Path.DirectorySeparatorChar, '/')}";
    }
}
