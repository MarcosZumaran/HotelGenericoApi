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

    public byte[] GenerateProductosExcel(IEnumerable<ProductoResponseDto> data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Productos");
        ws.Cell(1, 1).Value = "Código SUNAT";
        ws.Cell(1, 2).Value = "Nombre";
        ws.Cell(1, 3).Value = "Descripción";
        ws.Cell(1, 4).Value = "Precio Unitario";
        ws.Cell(1, 5).Value = "Stock";
        ws.Cell(1, 6).Value = "Stock Mínimo";
        ws.Cell(1, 7).Value = "Unidad Medida";
        ws.Cell(1, 8).Value = "Afectación IGV";
        var headerRange = ws.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        int row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.CodigoSunat ?? "";
            ws.Cell(row, 2).Value = item.Nombre;
            ws.Cell(row, 3).Value = item.Descripcion ?? "";
            ws.Cell(row, 4).Value = item.PrecioUnitario;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = item.Stock;
            ws.Cell(row, 6).Value = item.StockMinimo;
            ws.Cell(row, 7).Value = item.UnidadMedida ?? "";
            ws.Cell(row, 8).Value = item.NombreAfectacionIgv ?? "";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
