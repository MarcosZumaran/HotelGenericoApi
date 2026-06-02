using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class ItemVentum
{
    public int IdItem { get; set; }

    public int IdVenta { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal? Subtotal { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Ventum IdVentaNavigation { get; set; } = null!;

    // Convenience navigation properties
    public Ventum? Venta => IdVentaNavigation;
    public Producto? Producto => IdProductoNavigation;
}
