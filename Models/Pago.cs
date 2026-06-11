using System;

namespace HotelGenericoApi.Models;

public partial class Pago
{
    public int IdPago { get; set; }

    public int IdEstancia { get; set; }

    public decimal Monto { get; set; }

    public string MetodoPago { get; set; } = null!;

    public DateTime FechaPago { get; set; }

    public virtual Estancia IdEstanciaNavigation { get; set; } = null!;
}
