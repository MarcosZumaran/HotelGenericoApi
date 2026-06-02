using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class SalidaTemporalService : ISalidaTemporalService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<SalidaTemporalService> _logger;

    public SalidaTemporalService(HotelDbContext db, ILogger<SalidaTemporalService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task RegistrarSalidaTemporalAsync(int estanciaId, bool llavesDejadas)
    {
        var estancia = await _db.Estancias.FindAsync(estanciaId);
        if (estancia == null)
            throw new ArgumentException("Estancia no encontrada.");
        if (estancia.EstaFuera)
            throw new InvalidOperationException("El huésped ya está marcado como fuera.");

        estancia.EstaFuera = true;
        estancia.HoraSalidaTemporal = DateTime.UtcNow;
        estancia.LlavesDejadas = llavesDejadas;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Salida temporal registrada para estancia {Id}, llaves dejadas: {Llaves}", estanciaId, llavesDejadas);
    }

    public async Task RegistrarRegresoAsync(int estanciaId)
    {
        var estancia = await _db.Estancias.FindAsync(estanciaId);
        if (estancia == null)
            throw new ArgumentException("Estancia no encontrada.");
        if (!estancia.EstaFuera)
            throw new InvalidOperationException("El huésped no estaba fuera.");

        estancia.EstaFuera = false;
        estancia.HoraRegresoTemporal = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Regreso registrado para estancia {Id}", estanciaId);
    }
}
