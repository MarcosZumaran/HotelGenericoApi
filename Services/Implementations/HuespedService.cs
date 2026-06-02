using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class HuespedService : IHuespedService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<HuespedService> _logger;

    public HuespedService(HotelDbContext db, ILogger<HuespedService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Huesped> AgregarHuespedCompletoAsync(int estanciaId, AgregarHuespedDto dto)
    {
        var estancia = await _db.Estancias.FindAsync(estanciaId);
        if (estancia == null)
            throw new ArgumentException("Estancia no encontrada.");

        var cliente = await _db.Clientes
            .FirstOrDefaultAsync(c => c.TipoDocumento == dto.TipoDocumento && c.Documento == dto.Documento);
        if (cliente == null)
        {
            cliente = new Cliente
            {
                TipoDocumento = dto.TipoDocumento,
                Documento = dto.Documento,
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Nacionalidad = "PERUANA",
                FechaRegistro = DateTime.UtcNow
            };
            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync();
        }

        var yaExiste = await _db.Huespedes
            .AnyAsync(h => h.IdEstancia == estanciaId && h.IdCliente == cliente.IdCliente);
        if (yaExiste)
            throw new InvalidOperationException("El huésped ya está registrado en esta estancia.");

        var huesped = new Huesped
        {
            IdEstancia = estanciaId,
            IdCliente = cliente.IdCliente,
            EsTitular = false,
            FechaRegistro = DateTime.UtcNow
        };
        _db.Huespedes.Add(huesped);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Huésped {ClienteId} agregado a estancia {EstanciaId}", cliente.IdCliente, estanciaId);
        return huesped;
    }
}
