using ClosedXML.Excel;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class ExcelExportService : IExcelExportService
{
    public byte[] GenerateCierreCajaExcel(IEnumerable<VCierreCajaDiario> data, DateOnly fecha)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Cierre Caja");
        ws.Cell(1, 1).Value = "Concepto";
        ws.Cell(1, 2).Value = "Método de Pago";
        ws.Cell(1, 3).Value = "Ingresos";
        var headerRange = ws.Range(1, 1, 1, 3);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        int row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.Concepto;
            ws.Cell(row, 2).Value = item.MetodoPago;
            ws.Cell(row, 3).Value = item.Ingresos ?? 0;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateOcupacionDiariaExcel(IEnumerable<VOcupacionDiaria> data, DateOnly fecha)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Ocupación Diaria");
        ws.Cell(1, 1).Value = "Fecha";
        ws.Cell(1, 2).Value = "Ocupadas";
        ws.Cell(1, 3).Value = "Total";
        ws.Cell(1, 4).Value = "Porcentaje Ocupación";
        var headerRange = ws.Range(1, 1, 1, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        int row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.Fecha?.ToString("yyyy-MM-dd") ?? "";
            ws.Cell(row, 2).Value = item.Ocupadas ?? 0;
            ws.Cell(row, 3).Value = item.Total ?? 0;
            if (item.PorcentajeOcupacion.HasValue)
            {
                ws.Cell(row, 4).Value = item.PorcentajeOcupacion.Value;
                ws.Cell(row, 4).Style.NumberFormat.Format = "0.00%";
            }
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateEstanciasActivasExcel(IEnumerable<Estancia> data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Huespedes Activos");
        ws.Cell(1, 1).Value = "N° Habitación";
        ws.Cell(1, 2).Value = "Huésped";
        ws.Cell(1, 3).Value = "Check-In";
        ws.Cell(1, 4).Value = "Salida Prevista";
        ws.Cell(1, 5).Value = "Monto";
        ws.Cell(1, 6).Value = "Método de Pago";
        var headerRange = ws.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        int row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.IdHabitacionNavigation?.NumeroHabitacion ?? item.IdHabitacion.ToString();
            ws.Cell(row, 2).Value = item.IdClienteTitularNavigation != null
                ? $"{item.IdClienteTitularNavigation.Nombres} {item.IdClienteTitularNavigation.Apellidos}"
                : "";
            ws.Cell(row, 3).Value = item.FechaCheckin.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 4).Value = item.FechaCheckoutPrevista.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 5).Value = item.MontoTotal;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = item.MetodoPago ?? "";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateProductosExcel(IEnumerable<ProductoResponseDto> data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Productos");
        ws.Cell(1, 1).Value = "Código SUNAT";
        ws.Cell(1, 2).Value = "Nombre";
        ws.Cell(1, 3).Value = "Categoría";
        ws.Cell(1, 4).Value = "Descripción";
        ws.Cell(1, 5).Value = "Precio Unitario";
        ws.Cell(1, 6).Value = "Stock";
        ws.Cell(1, 7).Value = "Stock Mínimo";
        ws.Cell(1, 8).Value = "Unidad Medida";
        ws.Cell(1, 9).Value = "Afectación IGV";
        var headerRange = ws.Range(1, 1, 1, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        int row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.CodigoSunat ?? "";
            ws.Cell(row, 2).Value = item.Nombre;
            ws.Cell(row, 3).Value = item.NombreCategoria ?? "";
            ws.Cell(row, 4).Value = item.Descripcion ?? "";
            ws.Cell(row, 5).Value = item.PrecioUnitario;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = item.Stock;
            ws.Cell(row, 7).Value = item.StockMinimo;
            ws.Cell(row, 8).Value = item.UnidadMedida ?? "";
            ws.Cell(row, 9).Value = item.NombreAfectacionIgv ?? "";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateParStockExcel(IEnumerable<ParStockItemDto> data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Par Stock");
        ws.Cell(1, 1).Value = "Producto";
        ws.Cell(1, 2).Value = "Categoría";
        ws.Cell(1, 3).Value = "Stock Actual";
        ws.Cell(1, 4).Value = "Stock Mínimo";
        ws.Cell(1, 5).Value = "Nivel %";
        ws.Cell(1, 6).Value = "Unidad Medida";
        ws.Cell(1, 7).Value = "Es Amenidad";
        var headerRange = ws.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        int row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.Nombre;
            ws.Cell(row, 2).Value = item.Categoria ?? "";
            ws.Cell(row, 3).Value = item.Stock;
            ws.Cell(row, 4).Value = item.StockMinimo;
            ws.Cell(row, 5).Value = item.NivelPorcentaje / 100m;
            ws.Cell(row, 5).Style.NumberFormat.Format = "0.0%";
            ws.Cell(row, 6).Value = item.UnidadMedida ?? "";
            ws.Cell(row, 7).Value = item.EsAmenidad ? "Sí" : "No";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateGastoAmenitiesExcel(GastoAmenitiesResponseDto resumen, List<GastoAmenitiesDiarioDto> diario)
    {
        using var workbook = new XLWorkbook();

        var wsResumen = workbook.Worksheets.Add("Resumen por producto");
        wsResumen.Cell(1, 1).Value = "Producto";
        wsResumen.Cell(1, 2).Value = "Cantidad Total";
        wsResumen.Cell(1, 3).Value = "Costo Unitario (prom.)";
        wsResumen.Cell(1, 4).Value = "Costo Total";
        var headerResumen = wsResumen.Range(1, 1, 1, 4);
        headerResumen.Style.Font.Bold = true;
        headerResumen.Style.Fill.BackgroundColor = XLColor.LightGray;
        int row = 2;
        foreach (var item in resumen.Detalle)
        {
            wsResumen.Cell(row, 1).Value = item.Nombre;
            wsResumen.Cell(row, 2).Value = item.CantidadTotal;
            wsResumen.Cell(row, 3).Value = item.CostoUnitario;
            wsResumen.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            wsResumen.Cell(row, 4).Value = item.CostoTotal;
            wsResumen.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }
        wsResumen.Cell(row, 1).Value = "TOTAL GENERAL";
        wsResumen.Cell(row, 1).Style.Font.Bold = true;
        wsResumen.Cell(row, 4).Value = resumen.CostoTotal;
        wsResumen.Cell(row, 4).Style.Font.Bold = true;
        wsResumen.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        wsResumen.Columns().AdjustToContents();

        var wsDiario = workbook.Worksheets.Add("Evolución diaria");
        wsDiario.Cell(1, 1).Value = "Fecha";
        wsDiario.Cell(1, 2).Value = "Costo Total";
        var headerDiario = wsDiario.Range(1, 1, 1, 2);
        headerDiario.Style.Font.Bold = true;
        headerDiario.Style.Fill.BackgroundColor = XLColor.LightGray;
        row = 2;
        foreach (var item in diario)
        {
            wsDiario.Cell(row, 1).Value = item.Fecha.ToString("yyyy-MM-dd");
            wsDiario.Cell(row, 2).Value = item.CostoTotal;
            wsDiario.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }
        wsDiario.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
