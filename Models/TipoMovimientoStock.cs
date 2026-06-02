using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class TipoMovimientoStock
{
    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<MovimientoStock> MovimientoStocks { get; set; } = new List<MovimientoStock>();
}
