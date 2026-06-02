using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class Incidente
{
    public int IdIncidente { get; set; }

    public int? IdEstancia { get; set; }

    public int IdHabitacion { get; set; }

    public string Tipo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string? ImagenUrl { get; set; }

    public decimal? CostoEstimado { get; set; }

    public bool CobradoAlCliente { get; set; }

    public bool Resuelto { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? ReportadoPor { get; set; }

    public virtual Estancia? IdEstanciaNavigation { get; set; }

    public virtual Habitacion IdHabitacionNavigation { get; set; } = null!;

    public virtual Usuario? ReportadoPorNavigation { get; set; }

    // Convenience navigation properties
    public Habitacion? Habitacion => IdHabitacionNavigation;
    public Usuario? UsuarioReporte => ReportadoPorNavigation;
}
