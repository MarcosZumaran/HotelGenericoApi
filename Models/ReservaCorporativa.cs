using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

    public virtual ICollection<Estancia> Estancias { get; set; } = new List<Estancia>();

    public virtual Cliente IdClienteEmpresaNavigation { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    // Convenience navigation properties (excluded from EF mapping)
    [NotMapped]
    public Cliente? ClienteEmpresa => IdClienteEmpresaNavigation;
}
