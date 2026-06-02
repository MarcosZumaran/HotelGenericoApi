using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class HistorialTraslado
{
    public int IdTraslado { get; set; }

    public int IdEstancia { get; set; }

    public int IdHabitacionOrigen { get; set; }

    public int IdHabitacionDestino { get; set; }

    public string? Motivo { get; set; }

    public DateTime FechaTraslado { get; set; }

    public int UsuarioId { get; set; }

    public decimal? AjusteMonto { get; set; }

    public virtual Estancium IdEstanciaNavigation { get; set; } = null!;

    public virtual Habitacion IdHabitacionDestinoNavigation { get; set; } = null!;

    public virtual Habitacion IdHabitacionOrigenNavigation { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
