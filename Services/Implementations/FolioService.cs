using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class FolioService : IFolioService
{
    private readonly HotelDbContext _db;

    public FolioService(HotelDbContext db)
    {
        _db = db;
    }

    public async Task<FolioEstanciaDto?> GetFolioAsync(int idEstancia)
    {
        var estancia = await _db.Estancias
            .Include(e => e.IdHabitacionNavigation)
            .Include(e => e.IdClienteTitularNavigation)
            .Include(e => e.ItemsEstancia!)
                .ThenInclude(i => i.IdProductoNavigation)
            .Include(e => e.Pagos)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEstancia == idEstancia);

        if (estancia is null)
            return null;

        var nombreCliente = estancia.IdClienteTitularNavigation is not null
            ? $"{estancia.IdClienteTitularNavigation.Nombres} {estancia.IdClienteTitularNavigation.Apellidos}".Trim()
            : "Anónimo";

        var items = estancia.ItemsEstancia?
            .OrderByDescending(i => i.FechaRegistro)
            .Select(i =>
            {
                var tipo = i.IdProductoNavigation?.EsAmenidad == true
                    ? "amenidad"
                    : i.IdProductoNavigation?.Nombre?.Contains("Late", StringComparison.OrdinalIgnoreCase) == true
                        ? "late_checkout" : "consumo";

                return new FolioItemDto(
                    IdItem: i.IdItem,
                    Concepto: i.IdProductoNavigation?.Nombre ?? "Producto",
                    Cantidad: i.Cantidad,
                    PrecioUnitario: i.PrecioUnitario,
                    Subtotal: i.Subtotal ?? i.Cantidad * i.PrecioUnitario,
                    FechaRegistro: i.FechaRegistro,
                    Tipo: tipo
                );
            }).ToList() ?? new();

        var pagos = estancia.Pagos?
            .OrderByDescending(p => p.FechaPago)
            .Select(p => new FolioPagoDto(
                IdPago: p.IdPago,
                Monto: p.Monto,
                MetodoPago: p.MetodoPago,
                FechaPago: p.FechaPago,
                Concepto: p.Concepto
            )).ToList() ?? new();

        var totalPagado = pagos.Sum(p => p.Monto);
        var sumaItems = items.Where(i => i.Tipo != "amenidad").Sum(i => i.Subtotal);
        var saldoPendiente = estancia.MontoTotal + sumaItems - totalPagado;
        if (saldoPendiente < 0) saldoPendiente = 0;

        return new FolioEstanciaDto(
            IdEstancia: estancia.IdEstancia,
            NumeroHabitacion: estancia.IdHabitacionNavigation?.NumeroHabitacion ?? "—",
            Cliente: nombreCliente,
            FechaCheckin: estancia.FechaCheckin,
            FechaSalidaPrevista: estancia.FechaCheckoutPrevista,
            MontoEstancia: estancia.MontoTotal,
            ModalidadCobro: "Por noche",
            TotalPagado: totalPagado,
            SaldoPendiente: saldoPendiente,
            Items: items,
            Pagos: pagos
        );
    }
}
