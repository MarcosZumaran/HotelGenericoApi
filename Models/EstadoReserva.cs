using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class EstadoReserva
{
    public int IdEstadoReserva { get; set; }

    public string Codigo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool EsFinal { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
