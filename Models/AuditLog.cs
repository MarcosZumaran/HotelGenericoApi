using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class AuditLog
{
    public long IdAudit { get; set; }

    public string Tabla { get; set; } = null!;

    public string? IdRegistro { get; set; }

    public string Accion { get; set; } = null!;

    public string? Usuario { get; set; }

    public DateTime Fecha { get; set; }

    public string? DatosAnteriores { get; set; }

    public string? DatosNuevos { get; set; }

    public string? IpAddress { get; set; }

    public string? Modulo { get; set; }
}
