using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class EstadoEstancium
{
    public int IdEstadoEstancia { get; set; }

    public string Codigo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool EsFinal { get; set; }

    public virtual ICollection<Estancium> Estancia { get; set; } = new List<Estancium>();
}
