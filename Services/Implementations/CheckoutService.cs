using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Hubs;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class CheckoutService : ICheckoutService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<CheckoutService> _logger;
    private readonly IHubContext<HabitacionHub> _hubContext;

    public CheckoutService(
        HotelDbContext db,
        ILogger<CheckoutService> logger,
        IHubContext<HabitacionHub> hubContext)
    {
        _db = db;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<CheckoutResultDto> RealizarCheckoutAsync(int estanciaId, int idUsuario)
    {
        var estancia = await _db.Estancias
            .Include(e => e.IdHabitacionNavigation)
            .Include(e => e.ItemsEstancia)
            .FirstOrDefaultAsync(e => e.IdEstancia == estanciaId);

        if (estancia == null)
            throw new ArgumentException("Estancia no encontrada.");
        if (estancia.FechaCheckoutReal != null)
            throw new InvalidOperationException("La estancia ya tiene checkout realizado.");

        decimal totalConsumos = estancia.ItemsEstancia?.Sum(i => i.Subtotal) ?? 0;
        decimal totalHabitacion = estancia.MontoTotal;
        decimal totalFinal = totalHabitacion + totalConsumos;

        int? comprobanteId = null;

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            estancia.FechaCheckoutReal = DateTime.UtcNow;
            estancia.IdEstadoEstancia = EstadoEstanciaCodigo.Finalizada;

            if (estancia.Habitacion != null)
            {
                estancia.Habitacion.IdEstado = EstadoHabitacionCodigo.Limpieza;
                estancia.Habitacion.FechaUltimoCambio = DateTime.UtcNow;
                estancia.Habitacion.UsuarioCambio = idUsuario;

                _db.HistorialEstadoHabitaciones.Add(new HistorialEstadoHabitacion
                {
                    IdHabitacion = estancia.Habitacion.IdHabitacion,
                    IdEstadoAnterior = EstadoHabitacionCodigo.Ocupada,
                    IdEstadoNuevo = EstadoHabitacionCodigo.Limpieza,
                    FechaCambio = DateTime.UtcNow,
                    IdUsuario = idUsuario
                });
            }

            if (!estancia.IdReservaCorporativa.HasValue)
            {
                var clienteTitular = await _db.Clientes.FindAsync(estancia.IdClienteTitular)
                    ?? throw new Exception("Cliente titular no encontrado.");

                string tipoComprobante = (clienteTitular.TipoDocumento == "6") ? "01" : "03";
                string serie = (tipoComprobante == "01") ? "F001" : "B001";
                int correlativo = await ObtenerSiguienteCorrelativo(serie);
                decimal igv = totalFinal * 0.18m;

                var comprobante = new Comprobante
                {
                    IdEstancia = estanciaId,
                    TipoComprobante = tipoComprobante,
                    Serie = serie,
                    Correlativo = correlativo,
                    FechaEmision = DateTime.UtcNow,
                    MontoTotal = totalFinal,
                    IgvMonto = igv,
                    ClienteDocumentoTipo = clienteTitular.TipoDocumento,
                    ClienteDocumentoNum = clienteTitular.Documento,
                    ClienteNombre = $"{clienteTitular.Nombres} {clienteTitular.Apellidos}",
                    MetodoPago = null,
                    IdEstadoSunat = 1,
                    HashXml = null
                };

                _db.Comprobantes.Add(comprobante);
                await _db.SaveChangesAsync();
                comprobanteId = comprobante.IdComprobante;
                _logger.LogInformation("Comprobante {Serie}-{Correlativo} generado para estancia {Id}",
                    serie, correlativo, estanciaId);
            }
            else
            {
                _logger.LogInformation("Estancia corporativa {Id} finalizada sin comprobante individual", estanciaId);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients.All.SendAsync("EstadoHabitacionCambiado", new
            {
                idHabitacion = estancia.IdHabitacion,
                numero = estancia.Habitacion?.NumeroHabitacion,
                nuevoEstado = "Limpieza"
            });

            _logger.LogInformation("Checkout realizado para estancia {Id}. Total: {Total}", estanciaId, totalFinal);

            return new CheckoutResultDto
            {
                TotalHabitacion = totalHabitacion,
                TotalConsumos = totalConsumos,
                TotalFinal = totalFinal,
                ComprobanteId = (int)comprobanteId
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<int> ObtenerSiguienteCorrelativo(string serie)
    {
        var ultimo = await _db.Comprobantes
            .Where(c => c.Serie == serie)
            .MaxAsync(c => (int?)c.Correlativo) ?? 0;
        return ultimo + 1;
    }
}
