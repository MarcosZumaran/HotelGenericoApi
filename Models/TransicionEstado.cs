using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class TransicionEstado
{
    public int IdTransicion { get; set; }

    public int IdEstadoActual { get; set; }

    public int IdEstadoSiguiente { get; set; }

    public virtual EstadoHabitacion IdEstadoActualNavigation { get; set; } = null!;

    public virtual EstadoHabitacion IdEstadoSiguienteNavigation { get; set; } = null!;
}
