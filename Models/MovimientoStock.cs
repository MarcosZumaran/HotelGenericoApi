using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class MovimientoStock
{
    public long IdMovimiento { get; set; }

    public int IdProducto { get; set; }

    public int? IdHabitacion { get; set; }

    public int? IdEstancia { get; set; }

    public int? IdVenta { get; set; }

    public string CodigoTipoMovimiento { get; set; } = null!;

    public int IdUsuario { get; set; }

    public int Cantidad { get; set; }

    public int? StockAnterior { get; set; }

    public int? StockNuevo { get; set; }

    public decimal? CostoUnitario { get; set; }

    public string? Motivo { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public virtual TipoMovimientoStock CodigoTipoMovimientoNavigation { get; set; } = null!;

    public virtual Estancium? IdEstanciaNavigation { get; set; }

    public virtual Habitacion? IdHabitacionNavigation { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual Ventum? IdVentaNavigation { get; set; }
}
