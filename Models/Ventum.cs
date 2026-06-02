using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class Ventum
{
    public int IdVenta { get; set; }

    public int? IdCliente { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaVenta { get; set; }

    public decimal Total { get; set; }

    public string MetodoPago { get; set; } = null!;

    public virtual ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<ItemVentum> ItemVenta { get; set; } = new List<ItemVentum>();

    public virtual MetodoPago MetodoPagoNavigation { get; set; } = null!;

    public virtual ICollection<MovimientoStock> MovimientoStocks { get; set; } = new List<MovimientoStock>();

    // Convenience navigation properties
    public Cliente? Cliente => IdClienteNavigation;
    public ICollection<ItemVentum> ItemsVenta => ItemVenta;
}
