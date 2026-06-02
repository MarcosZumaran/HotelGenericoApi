using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class ParametroHotel
{
    public int IdParametro { get; set; }

    public string Clave { get; set; } = null!;

    public string Valor { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime FechaActualizacion { get; set; }
}
