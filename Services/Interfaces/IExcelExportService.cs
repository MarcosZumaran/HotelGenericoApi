using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;

namespace HotelGenericoApi.Services.Interfaces;

public interface IExcelExportService
{
    byte[] GenerateCierreCajaExcel(IEnumerable<VCierreCajaDiario> data, DateOnly fecha);
    byte[] GenerateOcupacionDiariaExcel(IEnumerable<VOcupacionDiaria> data, DateOnly fecha);
    byte[] GenerateProductosExcel(IEnumerable<ProductoResponseDto> data);
    byte[] GenerateEstanciasActivasExcel(IEnumerable<Estancia> data);
}
