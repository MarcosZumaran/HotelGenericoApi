using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class ItemEstancia
{
    public int IdItem { get; set; }

    public int IdEstancia { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal? Subtotal { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Estancia IdEstanciaNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    // Convenience navigation properties
    public Estancia? Estancia => IdEstanciaNavigation;
    public Producto? Producto => IdProductoNavigation;
}
