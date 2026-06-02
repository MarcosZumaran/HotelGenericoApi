using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Constants;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class ReservaQueryService : IReservaQueryService
{
    private readonly HotelDbContext _db;

    public ReservaQueryService(HotelDbContext db)
    {
        _db = db;
    }

    public async Task<List<ReservaResponseDto>> GetAllAsync()
    {
        return await _db.Reservas
            .Include(r => r.IdClienteNavigation)
            .Include(r => r.IdHabitacionNavigation)
            .Select(r => new ReservaResponseDto(
                r.IdReserva,
                r.IdHabitacion,
                r.IdHabitacionNavigation != null ? r.IdHabitacionNavigation.NumeroHabitacion : null,
                r.IdClienteNavigation != null ? $"{r.IdClienteNavigation.Nombres} {r.IdClienteNavigation.Apellidos}" : null,
                r.FechaEntradaPrevista,
                r.FechaSalidaPrevista,
                r.MontoTotal,
                r.Estado ?? EstadoReservaCodigo.Code.Pendiente,
                r.IdClienteNavigation != null ? r.IdClienteNavigation.Documento : null,
                r.Observaciones,
                r.EsNoShow
            ))
            .ToListAsync();
    }
}
