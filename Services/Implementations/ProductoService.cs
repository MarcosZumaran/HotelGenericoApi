using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Mappings;
using HotelGenericoApi.Services.Interfaces;
using HotelGenericoApi.Models;

using ImageMagick;

namespace HotelGenericoApi.Services.Implementations;

public class ProductoService : IProductoService
{
    private readonly HotelDbContext _db;
    private readonly ProductoMapper _mapper;

    public ProductoService(HotelDbContext db, ProductoMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductoResponseDto>> GetAllAsync()
    {
        var entities = await _db.Productos
            .Include(p => p.AfectacionIgv)
            .AsNoTracking()
            .ToListAsync();

        return entities.Select(MapToResponse);
    }

    public async Task<ProductoResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _db.Productos
            .Include(p => p.AfectacionIgv)
            .FirstOrDefaultAsync(p => p.IdProducto == id);

        return entity is not null ? MapToResponse(entity) : null;
    }

    public async Task<ProductoResponseDto> CreateAsync(ProductoCreateDto dto, IFormFile? file)
    {
        var entity = _mapper.FromCreate(dto);
        entity.CreatedAt = DateTime.UtcNow;

        if (file is not null)
            entity.ImagenUrl = await ProcesarImagenAsync(file);

        _db.Productos.Add(entity);
        await _db.SaveChangesAsync();
        await _db.Entry(entity).Reference(p => p.AfectacionIgv).LoadAsync();
        return MapToResponse(entity);
    }

    public async Task<bool> UpdateAsync(int id, ProductoUpdateDto dto, IFormFile? file)
    {
        var entity = await _db.Productos.FindAsync(id);
        if (entity is null) return false;

        _mapper.UpdateFromDto(dto, entity);

        if (file is not null)
            entity.ImagenUrl = await ProcesarImagenAsync(file);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Productos.FindAsync(id);
        if (entity is null) return false;
        _db.Productos.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ProductoResponseDto MapToResponse(Producto p)
    {
        return new ProductoResponseDto(
            p.IdProducto,
            p.CodigoSunat,
            p.Nombre,
            p.Descripcion,
            p.PrecioUnitario,
            p.IdAfectacionIgv,
            p.AfectacionIgv?.Descripcion,
            p.Stock,
            p.StockMinimo,
            p.UnidadMedida,
            p.CreatedAt,
            p.ImagenUrl,
            p.EsAmenidad,
            p.EsVendibleEnTienda,
            p.StockPorHabitacion
        );
    }


    private async Task<string?> ProcesarImagenAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        var extensiones = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!extensiones.Contains(ext))
            throw new InvalidOperationException("Formato de imagen no permitido.");

        var nombreArchivo = $"{Guid.NewGuid()}.webp";
        var rutaRelativa = Path.Combine("imagenes", "productos", nombreArchivo);
        var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", rutaRelativa);
        Directory.CreateDirectory(Path.GetDirectoryName(rutaCompleta)!);

        using var stream = file.OpenReadStream();
        using var image = new MagickImage(stream);

        // Redimensionar si es más ancha de 800px
        if (image.Width > 800)
        {
            var geometry = new MagickGeometry(800);
            image.Resize(geometry);
        }

        // Guardar como WebP con calidad 80
        image.Quality = 80;
        image.Format = MagickFormat.WebP;
        await image.WriteAsync(rutaCompleta);

        return $"/{rutaRelativa.Replace(Path.DirectorySeparatorChar, '/')}";
    }


    public async Task<bool> AddStockAsync(int id, int cantidad)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

        var producto = await _db.Productos.FindAsync(id);
        if (producto is null) return false;

        producto.Stock = (producto.Stock ?? 0) + cantidad;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetImagenUrlAsync(int id, string url)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto is null) return false;
        producto.ImagenUrl = url;
        await _db.SaveChangesAsync();
        return true;
    }
}
