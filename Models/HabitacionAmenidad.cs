using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class HabitacionAmenidad
{
    public int IdHabitacionAmenidad { get; set; }

    public int IdHabitacion { get; set; }

    public int IdProducto { get; set; }

    public int CantidadBase { get; set; }

    public virtual Habitacion IdHabitacionNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
