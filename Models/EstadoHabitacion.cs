using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class EstadoHabitacion
{
    public int IdEstado { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool PermiteCheckin { get; set; }

    public bool PermiteCheckout { get; set; }

    public bool EsEstadoFinal { get; set; }

    public string? ColorUi { get; set; }

    public virtual ICollection<Habitacion> Habitacions { get; set; } = new List<Habitacion>();

    public virtual ICollection<HistorialEstadoHabitacion> HistorialEstadoHabitacionIdEstadoAnteriorNavigations { get; set; } = new List<HistorialEstadoHabitacion>();

    public virtual ICollection<HistorialEstadoHabitacion> HistorialEstadoHabitacionIdEstadoNuevoNavigations { get; set; } = new List<HistorialEstadoHabitacion>();

    public virtual ICollection<TransicionEstado> TransicionEstadoIdEstadoActualNavigations { get; set; } = new List<TransicionEstado>();

    public virtual ICollection<TransicionEstado> TransicionEstadoIdEstadoSiguienteNavigations { get; set; } = new List<TransicionEstado>();
}
