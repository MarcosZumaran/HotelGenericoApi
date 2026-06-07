using HotelGenericoApi.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HotelGenericoApi.Services.Interfaces;
using HotelGenericoApi.Models;
using HotelGenericoApi.Models.Exceptions;

namespace HotelGenericoApi.Services.Implementations;

public class PdfService : IPdfService
{
    private readonly HotelDbContext _db;

    public PdfService(HotelDbContext db)
    {
        _db = db;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerarPdfComprobanteAsync(int idComprobante)
    {
        var comp = await _db.Comprobantes.FirstOrDefaultAsync(c => c.IdComprobante == idComprobante);
        if (comp == null) throw new BusinessRuleViolationException(BusinessErrorCode.ComprobanteNotFound, "Comprobante no encontrado.");

        string? numeroHabitacion = null;
        string? fechasHospedaje = null;
        List<ItemVentum>? itemsVenta = null;

        if (comp.IdEstancia.HasValue)
        {
            var estancia = await _db.Estancias.Include(e => e.IdHabitacionNavigation).FirstOrDefaultAsync(e => e.IdEstancia == comp.IdEstancia.Value);
            if (estancia != null)
            {
                numeroHabitacion = estancia.Habitacion?.NumeroHabitacion;
                fechasHospedaje = $"{estancia.FechaCheckin:dd/MM/yyyy} - {estancia.FechaCheckoutPrevista:dd/MM/yyyy}";
            }
        }

        if (comp.IdVenta.HasValue)
        {
            var venta = await _db.Ventas.Include(v => v.ItemVenta).ThenInclude(i => i.IdProductoNavigation).FirstOrDefaultAsync(v => v.IdVenta == comp.IdVenta.Value);
            itemsVenta = venta?.ItemVenta.ToList();
        }

        return await GenerarPdfComprobanteAsync(comp, numeroHabitacion, fechasHospedaje, itemsVenta);
    }

    public async Task<byte[]> GenerarPdfVentaAsync(int idVenta)
    {
        var comp = await _db.Comprobantes.FirstOrDefaultAsync(c => c.IdVenta == idVenta);
        if (comp == null) throw new BusinessRuleViolationException(BusinessErrorCode.ComprobanteNotFound, "Comprobante de venta no encontrado.");
        return await GenerarPdfComprobanteAsync(comp.IdComprobante);
    }

    public async Task<byte[]> GenerarPdfEstanciaAsync(int idEstancia)
    {
        var estancia = await _db.Estancias
            .Include(e => e.IdHabitacionNavigation)
            .Include(e => e.IdClienteTitularNavigation)
            .FirstOrDefaultAsync(e => e.IdEstancia == idEstancia);

        if (estancia == null)
            throw new BusinessRuleViolationException(BusinessErrorCode.ComprobanteNotFound, "Estancia no encontrada.");

        // Intentar obtener un comprobante asociado
        var comp = await _db.Comprobantes.FirstOrDefaultAsync(c => c.IdEstancia == idEstancia);

        if (comp != null)
            return await GenerarPdfComprobanteAsync(comp.IdComprobante);

        // Si no hay comprobante, generar uno temporal con los datos de la estancia
        // Calcular IGV correcto (Perú: 18% sobre la base, total = base * 1.18)
        decimal total = estancia.MontoTotal;
        decimal baseImponible = total / 1.18m;
        decimal igv = total - baseImponible;

        var comprobanteTemporal = new Models.Comprobante
        {
            Serie = "E",
            Correlativo = estancia.IdEstancia,
            FechaEmision = estancia.FechaCheckin,
            MontoTotal = total,
            IgvMonto = igv,
            ClienteNombre = estancia.ClienteTitular != null
                ? $"{estancia.ClienteTitular.Nombres} {estancia.ClienteTitular.Apellidos}"
                : "CLIENTE ANÓNIMO",
            ClienteDocumentoNum = estancia.ClienteTitular?.Documento ?? "—",
            TipoComprobante = "03",
            MetodoPago = Constants.MetodoPagoCodigo.Efectivo
        };

        string numeroHabitacion = estancia.Habitacion?.NumeroHabitacion ?? "—";
        string fechasHospedaje = $"{estancia.FechaCheckin:dd/MM/yyyy} - {estancia.FechaCheckoutPrevista:dd/MM/yyyy}";

        return await GenerarPdfComprobanteAsync(comprobanteTemporal, numeroHabitacion, fechasHospedaje, null);
    }

    // --- Lógica de generación de PDF con QuestPDF ---
    private async Task<byte[]> GenerarPdfComprobanteAsync(Models.Comprobante comp, string? numeroHabitacion, string? fechasHospedaje, List<ItemVentum>? itemsVenta)
    {
        string tipo = comp.TipoComprobante == "03" ? "BOLETA DE VENTA" : "FACTURA";
        string cliente = comp.ClienteNombre ?? "CLIENTE ANÓNIMO";
        string doc = comp.ClienteDocumentoNum ?? "—";

        // Obtener descripción del método de pago si existe el código
        string? metodoDesc = null;
        if (!string.IsNullOrEmpty(comp.MetodoPago))
        {
            metodoDesc = await _db.MetodosPago
                .Where(m => m.Codigo == comp.MetodoPago)
                .Select(m => m.Descripcion)
                .FirstOrDefaultAsync();
        }
        string metodo = metodoDesc ?? comp.MetodoPago ?? "—";

        // Generar el PDF con QuestPDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(col =>
                {
                    // 1. Encabezado
                    col.Item().AlignCenter().Text(tipo).FontSize(18).Bold();
                    col.Item().AlignCenter().Text($"Serie/Número: {comp.Serie}-{comp.Correlativo}");
                    col.Item().AlignCenter().Text($"Fecha: {comp.FechaEmision:dd/MM/yyyy}");
                    col.Item().AlignCenter().Text($"Cliente: {cliente}");
                    col.Item().AlignCenter().Text($"Doc: {doc} | Pago: {metodo}");
                    col.Item().PaddingVertical(10);

                    // 2. Tabla de Items
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Descripción").Bold();
                            header.Cell().Text("Cant.").Bold();
                            header.Cell().Text("P.Unit.").Bold();
                            header.Cell().Text("Subtotal").Bold();
                        });

                        // Items de Hospedaje
                        if (numeroHabitacion != null && fechasHospedaje != null)
                        {
                            table.Cell().Text($"Hospedaje {numeroHabitacion} ({fechasHospedaje})");
                            table.Cell().Text("1");
                            table.Cell().Text($"{comp.MontoTotal - comp.IgvMonto:F2}");
                            table.Cell().Text($"{comp.MontoTotal:F2}");
                        }

                        // Items de Venta
                        if (itemsVenta != null)
                        {
                            foreach (var item in itemsVenta)
                            {
                                table.Cell().Text(item.Producto?.Nombre ?? "Producto");
                                table.Cell().Text(item.Cantidad.ToString());
                                table.Cell().Text($"{item.PrecioUnitario:F2}");
                                table.Cell().Text($"{item.Subtotal:F2}");
                            }
                        }
                    });

                    col.Item().PaddingVertical(5);

                    // 3. Totales
                    col.Item().AlignRight().Text($"Subtotal: S/ {comp.MontoTotal - comp.IgvMonto:F2}");
                    col.Item().AlignRight().Text($"IGV (18%): S/ {comp.IgvMonto:F2}");
                    col.Item().AlignRight().Text($"TOTAL: S/ {comp.MontoTotal:F2}").FontSize(12).Bold();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerarPdfCierreCajaAsync(DateOnly fecha)
    {
        // Este método no necesita cambios, se incluye para que todo compile
        var datos = await _db.VCierreCajaDiario
            .Where(v => v.Fecha == fecha)
            .ToListAsync();

        decimal totalGeneral = datos.Sum(d => d.Ingresos.GetValueOrDefault());

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(col =>
                {
                    // Encabezado
                    col.Item().AlignCenter().Text("CIERRE DE CAJA").FontSize(16).Bold();
                    col.Item().AlignCenter().Text($"Fecha: {fecha:dd/MM/yyyy}").FontSize(12);
                    col.Item().PaddingVertical(10);

                    // Tabla
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Concepto").Bold();
                            header.Cell().Text("Método de Pago").Bold();
                            header.Cell().Text("Ingresos").Bold().AlignRight();
                        });

                        foreach (var item in datos)
                        {
                            table.Cell().Text(item.Concepto ?? "—");
                            table.Cell().Text(item.MetodoPago ?? "—");
                            table.Cell().AlignRight().Text($"S/ {item.Ingresos:F2}");
                        }
                    });

                    col.Item().PaddingVertical(5);

                    // Total general
                    col.Item().AlignRight().Text($"TOTAL GENERAL: S/ {totalGeneral:F2}")
                        .FontSize(12).Bold();
                });
            });
        });

        return document.GeneratePdf();
    }
}
