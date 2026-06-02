using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class Huesped
{
    public int IdHuesped { get; set; }

    public int IdEstancia { get; set; }

    public int IdCliente { get; set; }

    public bool EsTitular { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Estancia IdEstanciaNavigation { get; set; } = null!;

    // Convenience navigation properties
    public Cliente? Cliente => IdClienteNavigation;
}
