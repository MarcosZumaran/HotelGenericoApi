using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelGenericoApi.Models;

public partial class StockHabitacion
{
    public int IdStock { get; set; }

    public int IdHabitacion { get; set; }

    public int IdProducto { get; set; }

    public int CantidadActual { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Habitacion IdHabitacionNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    // Convenience navigation properties (excluded from EF mapping)
    [NotMapped]
    public Habitacion? Habitacion => IdHabitacionNavigation;
    [NotMapped]
    public Producto? Producto => IdProductoNavigation;
}
