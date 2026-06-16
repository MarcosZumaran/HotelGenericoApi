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
    private readonly IConfiguracionCacheService _configCache;
    private readonly IParametroHotelService _parametroHotelService;

    public CheckoutService(
        HotelDbContext db,
        ILogger<CheckoutService> logger,
        IHubContext<HabitacionHub> hubContext,
        IConfiguracionCacheService configCache,
        IParametroHotelService parametroHotelService)
    {
        _db = db;
        _logger = logger;
        _hubContext = hubContext;
        _configCache = configCache;
        _parametroHotelService = parametroHotelService;
    }

    public async Task<CheckoutResultDto> RealizarCheckoutAsync(int estanciaId, int idUsuario)
    {
        var estancia = await _db.Estancias
            .Include(e => e.IdHabitacionNavigation)
            .Include(e => e.ItemsEstancia)
            .Include(e => e.Pagos)
            .FirstOrDefaultAsync(e => e.IdEstancia == estanciaId);

        if (estancia == null)
            throw new ArgumentException("Estancia no encontrada.");
        if (estancia.FechaCheckoutReal != null)
            throw new InvalidOperationException("La estancia ya tiene checkout realizado.");

        decimal? cargoLateCheckout = null;
        int? horasLateCheckout = null;

        var paramsCk = await _parametroHotelService.GetCheckoutParamsAsync();
        if (TimeSpan.TryParse(paramsCk.CheckoutHoraLimite, out var horaTope)
            && decimal.TryParse(paramsCk.CheckoutCargoPorHora, out var cargoPorHora)
            && int.TryParse(paramsCk.CheckoutGraciaMinutos, out var graciaMinutos))
        {
            var checkpointUtc = estancia.FechaCheckoutPrevista.Date.Add(horaTope);
            var ahoraUtc = DateTime.UtcNow;
            var corteConGracia = checkpointUtc.AddMinutes(graciaMinutos);

            if (ahoraUtc > corteConGracia)
            {
                var diff = ahoraUtc - corteConGracia;
                horasLateCheckout = (int)Math.Ceiling(diff.TotalHours);
                cargoLateCheckout = horasLateCheckout.Value * cargoPorHora;

                var lateProduct = await _db.Productos
                    .FirstOrDefaultAsync(p => p.Nombre == "Late check-out");

                if (lateProduct == null)
                {
                    lateProduct = new Producto
                    {
                        Nombre = "Late check-out",
                        Descripcion = "Cargo por check-out fuera de hora",
                        PrecioUnitario = cargoPorHora,
                        IdAfectacionIgv = "10",
                        Stock = 0,
                        StockMinimo = 0,
                        UnidadMedida = "NIU",
                        EsAmenidad = false,
                        EsVendibleEnTienda = false,
                        CreatedAt = DateTime.UtcNow,
                    };
                    _db.Productos.Add(lateProduct);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    lateProduct.Stock = 0;
                    lateProduct.StockMinimo = 0;
                    lateProduct.EsVendibleEnTienda = false;
                }

                var itemLate = new ItemEstancia
                {
                    IdEstancia = estanciaId,
                    IdProducto = lateProduct.IdProducto,
                    Cantidad = horasLateCheckout.Value,
                    PrecioUnitario = cargoPorHora,
                    Subtotal = cargoLateCheckout.Value,
                    FechaRegistro = DateTime.UtcNow,
                };
                _db.ItemsEstancia.Add(itemLate);
            }
        }

        decimal totalConsumos = estancia.ItemsEstancia?.Sum(i => i.Subtotal) ?? 0;
        if (cargoLateCheckout.HasValue)
            totalConsumos += cargoLateCheckout.Value;
        decimal totalHabitacion = estancia.MontoTotal;
        decimal totalFinal = totalHabitacion + totalConsumos;

        decimal? montoDeposito = null;
        bool? depositoAplicado = null;
        var depositPago = estancia.Pagos?.FirstOrDefault(p => p.Concepto == "Depósito de garantía");
        if (depositPago != null)
        {
            montoDeposito = depositPago.Monto;
            depositoAplicado = false;
        }

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

                if (tipoComprobante == "01")
                {
                    if (string.IsNullOrEmpty(clienteTitular.Documento) || clienteTitular.Documento.Trim().Length != 11)
                        throw new InvalidOperationException("Para emitir una factura, el cliente debe tener un RUC válido. Si el cliente no tiene RUC, emita una boleta de venta.");
                }

                var config = await _configCache.GetConfiguracionAsync();
                if (tipoComprobante == "01" && config?.RegimenTributario == "NRUS")
                    throw new InvalidOperationException("El regimen NRUS no permite emitir facturas. Solo boletas de venta.");

                string serie = (tipoComprobante == "01") ? "F001" : "B001";
                int correlativo = await ObtenerSiguienteCorrelativo(serie);
                var igvPorcentaje = await ObtenerIgvHotelAsync();
                decimal igv = totalFinal * igvPorcentaje;

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
                    MetodoPago = estancia.MetodoPago,
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
                ComprobanteId = comprobanteId ?? 0,
                CargoLateCheckout = cargoLateCheckout,
                HorasLateCheckout = horasLateCheckout,
                MontoDepositoGarantia = montoDeposito,
                DepositoAplicado = depositoAplicado,
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<decimal> ObtenerIgvHotelAsync()
    {
        var config = await _configCache.GetConfiguracionAsync();
        return config?.TasaIgvHotel > 0 ? config.TasaIgvHotel / 100m : 0.18m;
    }

    private async Task<int> ObtenerSiguienteCorrelativo(string serie)
    {
        var ultimo = await _db.Comprobantes
            .Where(c => c.Serie == serie)
            .MaxAsync(c => (int?)c.Correlativo) ?? 0;
        return ultimo + 1;
    }
}
