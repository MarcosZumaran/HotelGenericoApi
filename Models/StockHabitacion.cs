using System;
using System.Collections.Generic;

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
}
