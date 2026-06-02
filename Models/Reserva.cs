using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int IdCliente { get; set; }

    public int IdHabitacion { get; set; }

    public int IdUsuario { get; set; }

    public int IdEstadoReserva { get; set; }

    public int? IdReservaCorporativa { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime FechaEntradaPrevista { get; set; }

    public DateTime FechaSalidaPrevista { get; set; }

    public decimal MontoTotal { get; set; }

    public string? Observaciones { get; set; }

    public bool EsNoShow { get; set; }

    public virtual ICollection<Estancia> Estancias { get; set; } = new List<Estancia>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual EstadoReserva IdEstadoReservaNavigation { get; set; } = null!;

    public virtual Habitacion IdHabitacionNavigation { get; set; } = null!;

    public virtual ReservaCorporativa? IdReservaCorporativaNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    // Convenience navigation properties
    public Cliente? Cliente => IdClienteNavigation;
    public Habitacion? Habitacion => IdHabitacionNavigation;
    public string? Estado => IdEstadoReservaNavigation?.Codigo;
}
