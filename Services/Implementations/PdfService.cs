using HotelGenericoApi.Data;
using HotelGenericoApi.Helpers;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HotelGenericoApi.Services.Interfaces;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Models.Exceptions;
using QRCoder;
using System.Text;

namespace HotelGenericoApi.Services.Implementations;

public class PdfService : IPdfService
{
    private readonly HotelDbContext _db;
    private readonly IConfiguracionCacheService _configCache;
    private readonly IWebHostEnvironment _env;

    public PdfService(HotelDbContext db, IConfiguracionCacheService configCache, IWebHostEnvironment env)
    {
        _db = db;
        _configCache = configCache;
        _env = env;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerarPdfComprobanteAsync(int idComprobante)
    {
        var comp = await _db.Comprobantes.FirstOrDefaultAsync(c => c.IdComprobante == idComprobante);
        if (comp == null) throw new BusinessRuleViolationException(BusinessErrorCode.ComprobanteNotFound, "Comprobante no encontrado.");

        string? numeroHabitacion = null;
        string? fechasHospedaje = null;
        int? noches = null;
        List<ItemVentum>? itemsVenta = null;

        if (comp.IdEstancia.HasValue)
        {
            var estancia = await _db.Estancias.Include(e => e.IdHabitacionNavigation).FirstOrDefaultAsync(e => e.IdEstancia == comp.IdEstancia.Value);
            if (estancia != null)
            {
                numeroHabitacion = estancia.Habitacion?.NumeroHabitacion;
                fechasHospedaje = $"{estancia.FechaCheckin:dd/MM/yyyy} - {estancia.FechaCheckoutPrevista:dd/MM/yyyy}";
                noches = (estancia.FechaCheckoutPrevista.Date - estancia.FechaCheckin.Date).Days;
                if (noches < 1) noches = 1;
            }
        }

        if (comp.IdVenta.HasValue)
        {
            var venta = await _db.Ventas.Include(v => v.ItemVenta).ThenInclude(i => i.IdProductoNavigation).FirstOrDefaultAsync(v => v.IdVenta == comp.IdVenta.Value);
            itemsVenta = venta?.ItemVenta.ToList();
        }

        return await GenerarPdfComprobanteAsync(comp, numeroHabitacion, fechasHospedaje, noches, itemsVenta);
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
            .Include(e => e.ItemsEstancia).ThenInclude(i => i.IdProductoNavigation)
            .Include(e => e.Pagos)
            .FirstOrDefaultAsync(e => e.IdEstancia == idEstancia);

        if (estancia == null)
            throw new BusinessRuleViolationException(BusinessErrorCode.ComprobanteNotFound, "Estancia no encontrada.");

        var comp = await _db.Comprobantes.FirstOrDefaultAsync(c => c.IdEstancia == idEstancia);
        if (comp != null)
            return await GenerarPdfComprobanteAsync(comp.IdComprobante);

        var config = await _configCache.GetConfiguracionAsync();
        var (logoBytes, nombreEmisor, ruc, direccionEmisor, telefonoEmisor) = await CargarDatosEmisorAsync(config);

        string clienteNombre = estancia.ClienteTitular != null
            ? $"{estancia.ClienteTitular.Nombres} {estancia.ClienteTitular.Apellidos}"
            : "CLIENTE ANONIMO";
        string docNum = estancia.ClienteTitular?.Documento ?? "-";
        string habNum = estancia.Habitacion?.NumeroHabitacion ?? "-";
        string fechas = $"{estancia.FechaCheckin:dd/MM/yyyy HH:mm} - {estancia.FechaCheckoutPrevista:dd/MM/yyyy HH:mm}";
        int noches = (estancia.FechaCheckoutPrevista.Date - estancia.FechaCheckin.Date).Days;
        if (noches < 1) noches = 1;

        var items = estancia.ItemsEstancia?.ToList() ?? [];
        var pagos = estancia.Pagos?.ToList() ?? [];
        decimal totalItems = items.Sum(i => i.Subtotal.GetValueOrDefault());
        decimal totalPagos = pagos.Sum(p => p.Monto);
        decimal saldo = estancia.MontoTotal - totalPagos;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(10);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(3);

                    EncabezadoEmisor(col, logoBytes, nombreEmisor, ruc, direccionEmisor, telefonoEmisor);
                    LineaSeparadora(col);

                    col.Item().AlignCenter().Text("RESUMEN DE ESTANCIA").FontSize(10).Bold();
                    col.Item().AlignCenter().Text($"Nro: E-{estancia.IdEstancia:D8}").FontSize(9);

                    LineaSeparadora(col);

                    col.Item().Text($"Cliente: {clienteNombre}").FontSize(8);
                    col.Item().Text($"Doc: {docNum}  Hab: {habNum}").FontSize(8);
                    col.Item().Text($"Fechas: {fechas}  ({noches} noche{(noches != 1 ? "s" : "")})").FontSize(8);

                    LineaSeparadora(col);

                    if (items.Count > 0)
                    {
                        col.Item().Text("Consumos:").FontSize(8).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.3f);
                                columns.RelativeColumn(2.3f);
                                columns.RelativeColumn(0.4f);
                                columns.RelativeColumn(0.5f);
                                columns.RelativeColumn(0.7f);
                                columns.RelativeColumn(0.7f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Text("#").FontSize(7).Bold().AlignCenter();
                                header.Cell().Background(Colors.Grey.Lighten3).Text("Producto").FontSize(7).Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Text("Cant").FontSize(7).Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten3).Text("Und").FontSize(7).Bold().AlignCenter();
                                header.Cell().Background(Colors.Grey.Lighten3).Text("P.Unit").FontSize(7).Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten3).Text("Subtotal").FontSize(7).Bold().AlignRight();
                            });

                            int idx = 0;
                            foreach (var item in items)
                            {
                                idx++;
                                table.Cell().Text(idx.ToString()).FontSize(7).AlignCenter();
                                table.Cell().Text(item.Producto?.Nombre ?? "-").FontSize(7);
                                table.Cell().Text(item.Cantidad.ToString()).FontSize(7).AlignRight();
                                table.Cell().AlignCenter().Text(item.Producto?.UnidadMedida ?? "UNIDAD").FontSize(7);
                                table.Cell().Text($"{item.PrecioUnitario:F2}").FontSize(7).AlignRight();
                                table.Cell().Text($"{item.Subtotal.GetValueOrDefault():F2}").FontSize(7).AlignRight();
                            }
                        });
                    }

                    if (pagos.Count > 0)
                    {
                        col.Item().PaddingVertical(2);
                        col.Item().Text("Pagos:").FontSize(8).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2f);
                                columns.RelativeColumn(1f);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Text("Metodo").FontSize(7).Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Text("Monto").FontSize(7).Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten3).Text("Concepto").FontSize(7).Bold();
                            });

                            foreach (var pago in pagos)
                            {
                                table.Cell().Text(pago.MetodoPago).FontSize(7);
                                table.Cell().AlignRight().Text($"S/ {pago.Monto:F2}").FontSize(7);
                                table.Cell().Text(pago.Concepto ?? "-").FontSize(7);
                            }
                        });
                    }

                    LineaSeparadora(col);

                    col.Item().AlignRight().Text($"Total estancia: S/ {estancia.MontoTotal:F2}").FontSize(8);
                    col.Item().AlignRight().Text($"Total pagado: S/ {totalPagos:F2}").FontSize(8);
                    col.Item().AlignRight().Text($"Saldo: S/ {saldo:F2}").FontSize(9).Bold();

                    col.Item().PaddingVertical(3);
                    col.Item().AlignCenter().Text("Gracias por su preferencia").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                });
            });
        });

        return document.GeneratePdf();
    }

    private byte[] GenerarQr(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(4);
    }

    private async Task<byte[]> GenerarPdfComprobanteAsync(Models.Comprobante comp, string? numeroHabitacion, string? fechasHospedaje, int? noches, List<ItemVentum>? itemsVenta)
    {
        string tipoDisplay = comp.TipoComprobante == "03" ? "BOLETA DE VENTA ELECTRONICA" : "FACTURA ELECTRONICA";
        string cliente = comp.ClienteNombre ?? "CLIENTE ANONIMO";
        string doc = comp.ClienteDocumentoNum ?? "-";
        string docTipo = comp.ClienteDocumentoTipo == "1" ? "DNI" : comp.ClienteDocumentoTipo == "6" ? "RUC" : "DOC";

        string? metodoDesc = null;
        if (!string.IsNullOrEmpty(comp.MetodoPago))
        {
            metodoDesc = await _db.MetodosPago
                .Where(m => m.Codigo == comp.MetodoPago)
                .Select(m => m.Descripcion)
                .FirstOrDefaultAsync();
        }
        string metodo = metodoDesc ?? comp.MetodoPago ?? "-";

        var config = await _configCache.GetConfiguracionAsync();
        var (logoBytes, nombreEmisor, ruc, direccionEmisor, telefonoEmisor) = await CargarDatosEmisorAsync(config);

        string serieCompleta = $"{comp.Serie}-{comp.Correlativo:D8}";
        string horaEmision = comp.FechaEmision.ToString("HH:mm");
        string fechaHoraEmision = $"{comp.FechaEmision:dd/MM/yyyy} {horaEmision}";
        string montoLetras = MontoEnLetrasHelper.Convertir(comp.MontoTotal);

        bool mostrarLeyendaAmazonia = config?.AplicaExoneracionAmazonia == true;
        string leyendaAmazonia = config?.LeyendaAmazonia;
        if (mostrarLeyendaAmazonia && string.IsNullOrEmpty(leyendaAmazonia))
            leyendaAmazonia = "BIENES TRANSFERIDOS/SERVICIOS PRESTADOS EN LA REGION DE SELVA PARA SER CONSUMIDOS EN LA MISMA";

        decimal baseImponible = comp.MontoTotal - comp.IgvMonto;

        string clientLine = $"{docTipo}: {doc}  {cliente}";

        var qrPayload = new StringBuilder();
        qrPayload.AppendLine($"RUC: {ruc}");
        qrPayload.AppendLine($"Tipo: {comp.TipoComprobante} ({(comp.TipoComprobante == "03" ? "Boleta" : "Factura")})");
        qrPayload.AppendLine($"Serie: {comp.Serie}");
        qrPayload.AppendLine($"Numero: {comp.Correlativo:D8}");
        qrPayload.AppendLine($"Fecha: {comp.FechaEmision:yyyy-MM-dd}");
        qrPayload.AppendLine($"Total: {comp.MontoTotal:F2}");
        qrPayload.AppendLine($"IGV: {comp.IgvMonto:F2}");

        byte[]? qrBytes = null;
        try { qrBytes = GenerarQr(qrPayload.ToString()); } catch { }

        bool esFactura = comp.TipoComprobante == "01";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                if (esFactura)
                {
                    page.Size(210, 148, QuestPDF.Infrastructure.Unit.Millimetre);
                    page.Margin(15);
                }
                else
                {
                    page.Size(new PageSize(80, 297, QuestPDF.Infrastructure.Unit.Millimetre));
                    page.Margin(8f);
                }

                page.DefaultTextStyle(x => x.FontSize(esFactura ? 9 : 8).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(esFactura ? 3 : 2);

                    EncabezadoEmisor(col, logoBytes, nombreEmisor, ruc, direccionEmisor, telefonoEmisor);
                    LineaSeparadora(col);

                    col.Item().AlignCenter().Text(tipoDisplay).FontSize(esFactura ? 12 : 10).Bold();
                    col.Item().AlignCenter().Text(serieCompleta).FontSize(esFactura ? 10 : 9);

                    LineaSeparadora(col);

                    col.Item().Text(clientLine).FontSize(esFactura ? 9 : 8);
                    col.Item().Text($"Fecha: {fechaHoraEmision}  Metodo: {metodo}").FontSize(esFactura ? 8 : 7);

                    if (esFactura)
                    {
                        string codEst = comp.Serie.Length >= 1 ? comp.Serie.Substring(0, 1) : "";
                        if (!string.IsNullOrEmpty(codEst))
                            col.Item().Text($"Establecimiento: {codEst}").FontSize(8);
                    }

                    LineaSeparadora(col);

                    float descCol = esFactura ? 2.5f : 1.8f;
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(esFactura ? 0.4f : 0.35f);
                            columns.RelativeColumn(descCol);
                            columns.RelativeColumn(esFactura ? 0.5f : 0.45f);
                            columns.RelativeColumn(esFactura ? 0.6f : 0.55f);
                            columns.RelativeColumn(esFactura ? 0.8f : 0.75f);
                            columns.RelativeColumn(esFactura ? 1.0f : 0.9f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Text("#").Bold().FontSize(8).AlignCenter();
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Descripcion").Bold().FontSize(8);
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Cant").Bold().FontSize(8).AlignRight();
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Und").Bold().FontSize(8).AlignCenter();
                            header.Cell().Background(Colors.Grey.Lighten3).Text("P.Unit").Bold().FontSize(8).AlignRight();
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Subtotal").Bold().FontSize(8).AlignRight();
                        });

                        int lineNum = 0;

                        if (numeroHabitacion != null && fechasHospedaje != null)
                        {
                            lineNum++;
                            string desc = $"Hospedaje {numeroHabitacion} ({fechasHospedaje})";
                            if (noches.HasValue)
                                desc += $" / {noches} noche{(noches != 1 ? "s" : "")}";
                            int fs = esFactura ? 8 : 7;
                            table.Cell().Text(lineNum.ToString()).FontSize(fs).AlignCenter();
                            table.Cell().Text(desc).FontSize(fs);
                            table.Cell().Text("1").FontSize(fs).AlignRight();
                            table.Cell().AlignCenter().Text("NOCHE").FontSize(fs);
                            table.Cell().Text($"{baseImponible:F2}").FontSize(fs).AlignRight();
                            table.Cell().Text($"{comp.MontoTotal:F2}").FontSize(fs).AlignRight();
                        }

                        if (itemsVenta != null)
                        {
                            foreach (var item in itemsVenta)
                            {
                                lineNum++;
                                int fs = esFactura ? 8 : 7;
                                table.Cell().Text(lineNum.ToString()).FontSize(fs).AlignCenter();
                                table.Cell().Text(item.Producto?.Nombre ?? "Producto").FontSize(fs);
                                table.Cell().Text(item.Cantidad.ToString()).FontSize(fs).AlignRight();
                                table.Cell().AlignCenter().Text(item.Producto?.UnidadMedida ?? "UNIDAD").FontSize(fs);
                                table.Cell().Text($"{item.PrecioUnitario:F2}").FontSize(fs).AlignRight();
                                table.Cell().Text($"{item.Subtotal:F2}").FontSize(fs).AlignRight();
                            }
                        }
                    });

                    LineaSeparadora(col);

                    if (esFactura)
                    {
                        col.Item().AlignRight().Text($"Valor de Venta: S/ {baseImponible:F2}").FontSize(9);
                        col.Item().AlignRight().Text($"IGV (18%): S/ {comp.IgvMonto:F2}").FontSize(9);
                        col.Item().AlignRight().Text($"Total: S/ {comp.MontoTotal:F2}").FontSize(12).Bold();
                    }
                    else
                    {
                        col.Item().AlignRight().Text($"Subtotal Gravado: S/ {baseImponible:F2}").FontSize(8);
                        col.Item().AlignRight().Text($"IGV (18%): S/ {comp.IgvMonto:F2}").FontSize(8);
                        col.Item().AlignRight().Text($"Total: S/ {comp.MontoTotal:F2}").FontSize(11).Bold();
                    }

                    col.Item().PaddingVertical(2);
                    col.Item().AlignRight().Text($"SON: {montoLetras}").FontSize(esFactura ? 8 : 7).Italic();

                    col.Item().PaddingVertical(2);
                    if (esFactura)
                    {
                        col.Item().AlignCenter().Text("Representacion impresa de la Factura Electronica").FontSize(8).Italic().FontColor(Colors.Grey.Darken2);
                    }
                    else
                    {
                        col.Item().AlignCenter().Text("Representacion impresa de la Boleta de Venta Electronica").FontSize(7).Italic().FontColor(Colors.Grey.Darken2);
                    }

                    if (mostrarLeyendaAmazonia && !string.IsNullOrEmpty(leyendaAmazonia))
                    {
                        col.Item().PaddingVertical(1);
                        col.Item().AlignCenter().Text(leyendaAmazonia).FontSize(esFactura ? 8 : 7).Italic().FontColor(Colors.Grey.Darken1);
                    }

                    col.Item().PaddingVertical(2);
                    if (qrBytes != null)
                    {
                        float qrSize = esFactura ? 80 : 60;
                        col.Item().AlignCenter().Width(qrSize).Height(qrSize).Image(qrBytes, ImageScaling.FitArea);
                        col.Item().PaddingVertical(1);
                        col.Item().AlignCenter().Text("Consulte este comprobante en recepcion").FontSize(7).Italic().FontColor(Colors.Grey.Darken2);
                    }

                    if (esFactura)
                    {
                        col.Item().PaddingVertical(3);
                        col.Item().AlignRight().Text("_________________________").FontSize(9);
                        col.Item().AlignRight().Text("Firma del emisor").FontSize(7).Italic().FontColor(Colors.Grey.Darken2);
                    }

                    col.Item().PaddingVertical(2);
                    col.Item().AlignCenter().Text("Gracias por su visita").FontSize(esFactura ? 9 : 8).Bold();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerarPdfCierreCajaAsync(DateOnly fecha)
    {
        var datos = await _db.VCierreCajaDiario
            .Where(v => v.Fecha == fecha)
            .ToListAsync();

        decimal totalGeneral = datos.Sum(d => d.Ingresos.GetValueOrDefault());
        var config = await _configCache.GetConfiguracionAsync();
        var (logoBytes, nombreEmisor, ruc, direccionEmisor, telefonoEmisor) = await CargarDatosEmisorAsync(config);

        // Count operations
        int checkinsHoy = await _db.Estancias.CountAsync(e => e.FechaCheckin.Date == fecha.ToDateTime(TimeOnly.MinValue).Date);
        int checkoutsHoy = await _db.Estancias.CountAsync(e => e.FechaCheckoutReal.HasValue && e.FechaCheckoutReal.Value.Date == fecha.ToDateTime(TimeOnly.MinValue).Date);
        int comprobantesHoy = await _db.Comprobantes.CountAsync(c => c.FechaEmision.Date == fecha.ToDateTime(TimeOnly.MinValue).Date);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(3);

                    EncabezadoEmisor(col, logoBytes, nombreEmisor, ruc, direccionEmisor, telefonoEmisor);
                    LineaSeparadora(col);

                    col.Item().AlignCenter().Text("CIERRE DE CAJA").FontSize(14).Bold();
                    col.Item().AlignCenter().Text($"Fecha: {fecha:dd/MM/yyyy}").FontSize(10);

                    LineaSeparadora(col);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Concepto").Bold().FontSize(8);
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Metodo de Pago").Bold().FontSize(8);
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Ingresos").Bold().FontSize(8).AlignRight();
                        });

                        foreach (var item in datos)
                        {
                            table.Cell().Text(item.Concepto ?? "-").FontSize(8);
                            table.Cell().Text(item.MetodoPago ?? "-").FontSize(8);
                            table.Cell().AlignRight().Text($"S/ {item.Ingresos:F2}").FontSize(8);
                        }
                    });

                    LineaSeparadora(col);

                    col.Item().AlignRight().Text($"TOTAL GENERAL: S/ {totalGeneral:F2}").FontSize(12).Bold();

                    col.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().Text("Resumen de operaciones:").FontSize(9).Bold();
                    col.Item().Text($"Check-ins: {checkinsHoy}  |  Check-outs: {checkoutsHoy}  |  Comprobantes emitidos: {comprobantesHoy}").FontSize(8);

                    col.Item().PaddingVertical(5);
                    col.Item().AlignRight().Text($"_________________________").FontSize(8);
                    col.Item().AlignRight().Text("Firma del responsable").FontSize(7).Italic().FontColor(Colors.Grey.Darken2);
                    col.Item().AlignCenter().PaddingTop(10).Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Darken2);
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerarPdfEstanciasActivasAsync()
    {
        var estancias = await _db.Estancias
            .AsNoTracking()
            .Where(e => e.IdEstadoEstancia == 2)
            .Include(e => e.IdHabitacionNavigation)
            .Include(e => e.IdClienteTitularNavigation)
            .OrderBy(e => e.IdHabitacion)
            .ToListAsync();

        var config = await _configCache.GetConfiguracionAsync();
        var (logoBytes, nombreEmisor, ruc, direccionEmisor, telefonoEmisor) = await CargarDatosEmisorAsync(config);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(3);

                    EncabezadoEmisor(col, logoBytes, nombreEmisor, ruc, direccionEmisor, telefonoEmisor);
                    LineaSeparadora(col);

                    col.Item().AlignCenter().Text("HUESPEDES ACTIVOS").FontSize(14).Bold();
                    col.Item().AlignCenter().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);

                    LineaSeparadora(col);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Hab.").Bold().FontSize(8);
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Cliente").Bold().FontSize(8);
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Check-in").Bold().FontSize(8);
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Check-out prev.").Bold().FontSize(8);
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Total").Bold().FontSize(8).AlignRight();
                        });

                        foreach (var e in estancias)
                        {
                            table.Cell().Text(e.IdHabitacionNavigation?.NumeroHabitacion ?? "-").FontSize(8);
                            table.Cell().Text($"{e.IdClienteTitularNavigation?.Nombres} {e.IdClienteTitularNavigation?.Apellidos}").FontSize(8);
                            table.Cell().Text(e.FechaCheckin.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                            table.Cell().Text(e.FechaCheckoutPrevista.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                            table.Cell().AlignRight().Text($"S/ {e.MontoTotal:F2}").FontSize(8);
                        }
                    });

                    col.Item().PaddingVertical(5);
                    col.Item().AlignRight().Text($"Total huespedes: {estancias.Count}").FontSize(9).Bold();

                    col.Item().PaddingTop(10);
                    col.Item().AlignCenter().Text($"Generado por SIGHLRN").FontSize(7).FontColor(Colors.Grey.Darken2);
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerarPdfParStockAsync()
    {
        var productos = await _db.Productos
            .Include(p => p.IdCategoriaNavigation)
            .Where(p => p.EsVendibleEnTienda)
            .AsNoTracking()
            .ToListAsync();

        var items = productos
            .Select(p => new ParStockItemDto(
                p.IdProducto,
                p.Nombre,
                p.IdCategoriaNavigation?.Nombre,
                p.Stock,
                p.StockMinimo,
                p.StockMinimo > 0
                    ? Math.Round((decimal)p.Stock / p.StockMinimo * 100m, 1)
                    : 100m,
                p.EsAmenidad,
                p.UnidadMedida
            ))
            .OrderBy(p => p.NivelPorcentaje)
            .ToList();

        var config = await _configCache.GetConfiguracionAsync();
        var (logoBytes, nombreEmisor, ruc, direccionEmisor, telefonoEmisor) = await CargarDatosEmisorAsync(config);

        int criticos = items.Count(p => p.NivelPorcentaje < 100);
        int muyCriticos = items.Count(p => p.NivelPorcentaje < 50);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(3);

                    EncabezadoEmisor(col, logoBytes, nombreEmisor, ruc, direccionEmisor, telefonoEmisor);
                    LineaSeparadora(col);

                    col.Item().AlignCenter().Text("PAR STOCK").FontSize(14).Bold();
                    col.Item().AlignCenter().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);

                    LineaSeparadora(col);

                    col.Item().Text($"Total productos: {items.Count}  |  Por debajo del minimo: {criticos}  |  Critico (<50%): {muyCriticos}").FontSize(8).FontColor(Colors.Grey.Darken2);

                    col.Item().PaddingVertical(3);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1f);
                            columns.RelativeColumn(1f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Producto").Bold().FontSize(8);
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Categoria").Bold().FontSize(8);
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Stock").Bold().FontSize(8).AlignRight();
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Stock Min.").Bold().FontSize(8).AlignRight();
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Nivel %").Bold().FontSize(8).AlignRight();
                            header.Cell().Background(Colors.Grey.Lighten3).Text("Und.").Bold().FontSize(8).AlignCenter();
                        });

                        foreach (var item in items)
                        {
                            var color = item.NivelPorcentaje < 50 ? Colors.Red.Darken2 :
                                        item.NivelPorcentaje < 100 ? Colors.Orange.Darken2 :
                                        Colors.Green.Darken2;

                            table.Cell().Text(item.Nombre ?? "-").FontSize(8);
                            table.Cell().Text(item.Categoria ?? "-").FontSize(8);
                            table.Cell().AlignRight().Text(item.Stock.ToString()).FontSize(8);
                            table.Cell().AlignRight().Text(item.StockMinimo.ToString()).FontSize(8);
                            table.Cell().AlignRight().Text($"{item.NivelPorcentaje:F1}%").FontSize(8).FontColor(color);
                            table.Cell().AlignCenter().Text(item.UnidadMedida ?? "-").FontSize(8);
                        }
                    });

                    LineaSeparadora(col);

                    col.Item().PaddingTop(5);
                    col.Item().AlignRight().Text($"Total productos: {items.Count}").FontSize(9).Bold();

                    col.Item().PaddingTop(10);
                    col.Item().AlignCenter().Text($"Generado por SIGHLRN").FontSize(7).FontColor(Colors.Grey.Darken2);
                });
            });
        });

        return document.GeneratePdf();
    }

    // ========== HELPER METHODS ==========

    private static void LineaSeparadora(ColumnDescriptor col)
    {
        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Darken2);
    }

    private static void EncabezadoEmisor(ColumnDescriptor col, byte[]? logoBytes, string nombre, string ruc, string direccion, string telefono)
    {
        col.Item().Row(row =>
        {
            if (logoBytes != null)
            {
                row.ConstantItem(50).Height(36).Image(logoBytes, ImageScaling.FitArea);
                row.RelativeItem().PaddingLeft(8).Column(c =>
                {
                    c.Spacing(1);
                    c.Item().Text(nombre).FontSize(12).Bold();
                    c.Item().Text($"RUC: {ruc}").FontSize(8);
                    if (!string.IsNullOrEmpty(direccion))
                        c.Item().Text(direccion).FontSize(7);
                    if (!string.IsNullOrEmpty(telefono))
                        c.Item().Text($"Tel: {telefono}").FontSize(7);
                });
            }
            else
            {
                row.RelativeItem().Column(c =>
                {
                    c.Spacing(1);
                    c.Item().AlignCenter().Text(nombre).FontSize(12).Bold();
                    c.Item().AlignCenter().Text($"RUC: {ruc}").FontSize(8);
                    if (!string.IsNullOrEmpty(direccion))
                        c.Item().AlignCenter().Text(direccion).FontSize(7);
                    if (!string.IsNullOrEmpty(telefono))
                        c.Item().AlignCenter().Text($"Tel: {telefono}").FontSize(7);
                });
            }
        });
    }

    private async Task<(byte[]? logoBytes, string nombre, string ruc, string direccion, string telefono)> CargarDatosEmisorAsync(Models.Configuracion? config)
    {
        string nombre = !string.IsNullOrEmpty(config?.NombreComercial) ? config.NombreComercial : (config?.Nombre ?? "HOTEL");
        string ruc = config?.Ruc ?? "-";
        string direccion = config?.Direccion ?? "";
        if (!string.IsNullOrEmpty(config?.Distrito))
            direccion += $", {config.Distrito}";
        if (!string.IsNullOrEmpty(config?.Provincia))
            direccion += $", {config.Provincia}";
        if (!string.IsNullOrEmpty(config?.Departamento))
            direccion += $" - {config.Departamento}";
        string telefono = config?.Telefono ?? "";

        byte[]? logoBytes = null;
        if (config?.LogoUrl != null)
        {
            var logoPath = Path.Combine(_env.WebRootPath, config.LogoUrl);
            if (File.Exists(logoPath))
                logoBytes = await File.ReadAllBytesAsync(logoPath);
        }

        return (logoBytes, nombre, ruc, direccion, telefono);
    }
}
