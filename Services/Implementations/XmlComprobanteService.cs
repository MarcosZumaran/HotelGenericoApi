using HotelGenericoApi.Data;
using HotelGenericoApi.Helpers;
using HotelGenericoApi.Models.Exceptions;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelGenericoApi.Services.Implementations;

public class XmlComprobanteService : IXmlComprobanteService
{
    private readonly HotelDbContext _db;
    private readonly IConfiguracionCacheService _configCache;

    public XmlComprobanteService(HotelDbContext db, IConfiguracionCacheService configCache)
    {
        _db = db;
        _configCache = configCache;
    }

    public async Task<string> GenerarXmlComprobanteAsync(int idComprobante)
    {
        var comp = await _db.Comprobantes
            .Include(c => c.IdEstanciaNavigation)
                .ThenInclude(e => e != null ? e.IdClienteTitularNavigation : null)
            .Include(c => c.IdVentaNavigation)
                .ThenInclude(v => v != null ? v.ItemVenta : null)
                    .ThenInclude(iv => iv != null ? iv.IdProductoNavigation : null)
            .FirstOrDefaultAsync(c => c.IdComprobante == idComprobante);

        if (comp == null)
            throw new BusinessRuleViolationException(BusinessErrorCode.ComprobanteNotFound, "Comprobante no encontrado.");

        var config = await _configCache.GetConfiguracionAsync();

        string emisorRuc = config?.Ruc ?? "00000000000";
        string emisorRazonSocial = config?.Nombre ?? "HOTEL GENERICO";
        string emisorNombreComercial = config?.NombreComercial ?? "";
        string emisorDireccion = config?.Direccion ?? "";
        string emisorUbigeo = config?.Ubigeo ?? "";
        string emisorDepartamento = config?.Departamento ?? "";
        string emisorProvincia = config?.Provincia ?? "";
        string emisorDistrito = config?.Distrito ?? "";
        string emisorUrbanizacion = config?.Urbanizacion ?? "";

        string clienteDocNum = comp.ClienteDocumentoNum ?? "00000000";
        string clienteTipoDoc = comp.ClienteDocumentoTipo ?? "1";
        string clienteNombre = comp.ClienteNombre ?? "CLIENTE ANONIMO";

        string serieCorrelativo = $"{comp.Serie}-{comp.Correlativo}";
        decimal total = comp.MontoTotal;
        decimal igv = comp.IgvMonto;
        decimal baseImponible = total - igv;

        var items = new List<(string descripcion, int cantidad, decimal precioUnitario, decimal subtotal)>();

        // Add hospedaje item if estancia exists
        if (comp.IdEstancia.HasValue && comp.IdEstanciaNavigation != null)
        {
            var estancia = comp.IdEstanciaNavigation;
            string habNumero = estancia.Habitacion?.NumeroHabitacion ?? "-";
            string fechas = $"{estancia.FechaCheckin:dd/MM/yyyy} - {estancia.FechaCheckoutPrevista:dd/MM/yyyy}";
            items.Add(($"Hospedaje {habNumero} ({fechas})", 1, baseImponible, baseImponible));
        }

        // Add items from venta if exists
        if (comp.IdVenta.HasValue && comp.IdVentaNavigation?.ItemVenta != null)
        {
            foreach (var iv in comp.IdVentaNavigation.ItemVenta)
            {
                string nombre = iv.Producto?.Nombre ?? "Producto";
                items.Add((nombre, iv.Cantidad, iv.PrecioUnitario, iv.Subtotal ?? 0m));
            }
        }

        // If neither estancia nor venta items exist, add a generic line
        if (items.Count == 0)
        {
            items.Add(("Servicio de hospedaje", 1, baseImponible, baseImponible));
        }

        bool aplicarLeyendaAmazonia = config?.AplicaExoneracionAmazonia == true;
        string? leyendaAmazonia = config?.LeyendaAmazonia;

        var doc = XmlBoletaBuilder.BuildBoleta(
            serieCorrelativo,
            comp.TipoComprobante ?? "03",
            comp.FechaEmision,
            total,
            igv,
            baseImponible,
            clienteTipoDoc,
            clienteDocNum,
            clienteNombre,
            emisorRuc,
            emisorRazonSocial,
            emisorNombreComercial,
            emisorDireccion,
            emisorUbigeo,
            emisorDepartamento,
            emisorProvincia,
            emisorDistrito,
            emisorUrbanizacion,
            "PEN",
            items,
            aplicarLeyendaAmazonia,
            leyendaAmazonia
        );

        return doc.ToString();
    }
}
