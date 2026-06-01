using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class ReservaCorporativaService : IReservaCorporativaService
{
    private readonly HotelDbContext _db;

    public ReservaCorporativaService(HotelDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ReservaCorporativaResponseDto>> GetAllAsync()
    {
        // 1. Cargar datos desde la BD con las relaciones necesarias
        var reservas = await _db.ReservasCorporativas
            .Include(r => r.ClienteEmpresa)
            .Include(r => r.Estancias)
                .ThenInclude(e => e.ItemsEstancia)
            .AsNoTracking()
            .ToListAsync();

        // 2. Calcular el total acumulado en memoria (LINQ to Objects)
        var result = reservas.Select(r => new ReservaCorporativaResponseDto
        {
            IdReservaCorporativa = r.IdReservaCorporativa,
            IdClienteEmpresa = r.IdClienteEmpresa,
            NombreEmpresa = r.ClienteEmpresa != null ? $"{r.ClienteEmpresa.Nombres} {r.ClienteEmpresa.Apellidos}" : "",
            RucEmpresa = r.ClienteEmpresa != null ? r.ClienteEmpresa.Documento : "",
            FechaInicio = r.FechaInicio,
            FechaFin = r.FechaFin,
            NumeroHabitaciones = r.NumeroHabitaciones,
            HabitacionesOcupadas = r.Estancias.Count(e => e.Estado == "Activa"),
            Estado = r.Estado,
            TotalAcumulado = r.Estancias
                .Where(e => e.FechaCheckoutReal != null)
                .Sum(e => e.MontoTotal + (e.ItemsEstancia != null ? e.ItemsEstancia.Sum(i => i.Subtotal) : 0)),
            Observaciones = r.Observaciones,
            FechaRegistro = r.FechaRegistro
        });

        return result;
    }

    public async Task<ReservaCorporativaResponseDto?> GetByIdAsync(int id)
    {
        // 1. Cargar la reserva específica con todas las relaciones
        var reserva = await _db.ReservasCorporativas
            .Include(r => r.ClienteEmpresa)
            .Include(r => r.Estancias)
                .ThenInclude(e => e.ItemsEstancia)
            .FirstOrDefaultAsync(r => r.IdReservaCorporativa == id);

        if (reserva == null) return null;

        // 2. Construir el DTO calculando el total en memoria
        return new ReservaCorporativaResponseDto
        {
            IdReservaCorporativa = reserva.IdReservaCorporativa,
            IdClienteEmpresa = reserva.IdClienteEmpresa,
            NombreEmpresa = reserva.ClienteEmpresa != null ? $"{reserva.ClienteEmpresa.Nombres} {reserva.ClienteEmpresa.Apellidos}" : "",
            RucEmpresa = reserva.ClienteEmpresa != null ? reserva.ClienteEmpresa.Documento : "",
            FechaInicio = reserva.FechaInicio,
            FechaFin = reserva.FechaFin,
            NumeroHabitaciones = reserva.NumeroHabitaciones,
            HabitacionesOcupadas = reserva.Estancias.Count(e => e.Estado == "Activa"),
            Estado = reserva.Estado,
            TotalAcumulado = reserva.Estancias
                .Where(e => e.FechaCheckoutReal != null)
                .Sum(e => e.MontoTotal + (e.ItemsEstancia != null ? e.ItemsEstancia.Sum(i => i.Subtotal) : 0)),
            Observaciones = reserva.Observaciones,
            FechaRegistro = reserva.FechaRegistro
        };
    }

    public async Task<ReservaCorporativaResponseDto> CreateAsync(ReservaCorporativaCreateDto dto, int idUsuario)
    {
        var cliente = await _db.Clientes.FindAsync(dto.IdClienteEmpresa);
        // Verificar que el cliente sea una empresa
        // if (cliente == null || cliente.TipoDocumento != "6") throw new ArgumentException("El cliente debe ser una empresa con RUC (TipoDocumento = 6).");
        // Deshabilitado para permitir pruebas con clientes naturales, dado a que pueden hacer reservaciones familiares, y así no limitar el desarrollo solo a clientes empresariales.

        var reserva = new ReservaCorporativa
        {
            IdClienteEmpresa = dto.IdClienteEmpresa,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            NumeroHabitaciones = dto.NumeroHabitaciones,
            Estado = "Pendiente",
            Observaciones = dto.Observaciones,
            FechaRegistro = DateTime.UtcNow
        };

        _db.ReservasCorporativas.Add(reserva);
        await _db.SaveChangesAsync();

        return (await GetByIdAsync(reserva.IdReservaCorporativa))!;
    }

    public async Task<bool> UpdateAsync(int id, ReservaCorporativaCreateDto dto)
    {
        var reserva = await _db.ReservasCorporativas.FindAsync(id);
        if (reserva == null) return false;

        // Solo se puede modificar si está Pendiente
        if (reserva.Estado != "Pendiente")
            throw new InvalidOperationException("Solo se pueden modificar reservas en estado Pendiente.");

        reserva.FechaInicio = dto.FechaInicio;
        reserva.FechaFin = dto.FechaFin;
        reserva.NumeroHabitaciones = dto.NumeroHabitaciones;
        reserva.Observaciones = dto.Observaciones;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reserva = await _db.ReservasCorporativas
            .Include(r => r.Estancias)
            .FirstOrDefaultAsync(r => r.IdReservaCorporativa == id);

        if (reserva == null) return false;

        if (reserva.Estancias.Any(e => e.Estado == "Activa"))
            throw new InvalidOperationException("No se puede eliminar una reserva corporativa con estancias activas.");

        if (reserva.Estado != "Pendiente")
            throw new InvalidOperationException("Solo se pueden eliminar reservas en estado Pendiente.");

        _db.ReservasCorporativas.Remove(reserva);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ReservaCorporativaResponseDto> FinalizarAsync(int id, int idUsuario)
    {
        var reserva = await _db.ReservasCorporativas
            .Include(r => r.Estancias)
                .ThenInclude(e => e.ItemsEstancia)
            .Include(r => r.ClienteEmpresa)
            .FirstOrDefaultAsync(r => r.IdReservaCorporativa == id);

        if (reserva == null)
            throw new ArgumentException("Reserva corporativa no encontrada.");

        if (reserva.Estado == "Finalizada")
            throw new InvalidOperationException("La reserva ya está finalizada.");

        // Verificar que todas las estancias hayan hecho checkout
        var estanciasPendientes = reserva.Estancias.Any(e => e.FechaCheckoutReal == null);
        if (estanciasPendientes)
            throw new InvalidOperationException("No se puede finalizar porque hay estancias activas sin checkout.");

        // Calcular total acumulado de todas las estancias
        decimal totalFinal = reserva.Estancias.Sum(e => e.MontoTotal + (e.ItemsEstancia != null ? e.ItemsEstancia.Sum(i => i.Subtotal) : 0));

        // Generar comprobante único (factura) a nombre de la empresa
        // Aquí debes integrar tu lógica de ComprobanteService
        // Por ahora, simulamos la creación
        var comprobante = new Comprobante
        {
            IdEstancia = null,
            IdVenta = null,
            TipoComprobante = "01",
            Serie = "F001",
            Correlativo = await ObtenerSiguienteCorrelativo("F001"),
            FechaEmision = DateTime.UtcNow,
            MontoTotal = totalFinal,
            IgvMonto = totalFinal * 0.18m,
            ClienteDocumentoTipo = reserva.ClienteEmpresa!.TipoDocumento,
            ClienteDocumentoNum = reserva.ClienteEmpresa.Documento,
            ClienteNombre = $"{reserva.ClienteEmpresa.Nombres} {reserva.ClienteEmpresa.Apellidos}",
            MetodoPago = null,
            IdEstadoSunat = 1,
            HashXml = null
        };

        _db.Comprobantes.Add(comprobante);
        reserva.Estado = "Finalizada";
        await _db.SaveChangesAsync();

        return (await GetByIdAsync(id))!;
    }

    // Verifica si la corporativa aún tiene cupo para nuevas estancias
    public async Task<bool> ValidarYAsignarHabitacionAsync(int idReservaCorporativa)
    {
        var reserva = await _db.ReservasCorporativas
            .Include(r => r.Estancias)
            .FirstOrDefaultAsync(r => r.IdReservaCorporativa == idReservaCorporativa);

        if (reserva == null) return false;
        if (reserva.Estado != "Confirmada" && reserva.Estado != "Pendiente") return false;

        var activas = reserva.Estancias.Count(e => e.Estado == "Activa");
        return activas < reserva.NumeroHabitaciones;
    }

    private async Task<int> ObtenerSiguienteCorrelativo(string serie)
    {
        var ultimo = await _db.Comprobantes
            .Where(c => c.Serie == serie)
            .MaxAsync(c => (int?)c.Correlativo) ?? 0;
        return ultimo + 1;
    }
}
