using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class ObjetoPerdido
{
    public int IdObjeto { get; set; }

    public int? IdHabitacion { get; set; }

    public int? IdEstancia { get; set; }

    public string Descripcion { get; set; } = null!;

    public string? ImagenUrl { get; set; }

    public DateTime FechaHallazgo { get; set; }

    public string Estado { get; set; } = null!;

    public string? EntregadoA { get; set; }

    public DateTime? FechaEntregado { get; set; }

    public virtual Estancium? IdEstanciaNavigation { get; set; }

    public virtual Habitacion? IdHabitacionNavigation { get; set; }
}
