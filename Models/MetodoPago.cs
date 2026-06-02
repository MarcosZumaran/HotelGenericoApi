using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class MetodoPago
{
    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
