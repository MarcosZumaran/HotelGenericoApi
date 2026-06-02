using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class ReservaCorporativa
{
    public int IdReservaCorporativa { get; set; }

    public int IdClienteEmpresa { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public int NumeroHabitaciones { get; set; }

    public string Estado { get; set; } = null!;

    public string? Observaciones { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<Estancium> Estancia { get; set; } = new List<Estancium>();

    public virtual Cliente IdClienteEmpresaNavigation { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
