using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

    public virtual Estancia? IdEstanciaNavigation { get; set; }

    public virtual Habitacion? IdHabitacionNavigation { get; set; }

    // Convenience navigation properties (excluded from EF mapping)
    [NotMapped]
    public Habitacion? Habitacion => IdHabitacionNavigation;
    [NotMapped]
    public Estancia? Estancia => IdEstanciaNavigation;
}
