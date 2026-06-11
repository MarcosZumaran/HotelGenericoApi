using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelGenericoApi.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int IdRol { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool EstaActivo { get; set; }

    public bool DebeCambiarPassword { get; set; }

    public virtual ICollection<Habitacion> Habitacions { get; set; } = new List<Habitacion>();

    public virtual ICollection<HistorialEstadoHabitacion> HistorialEstadoHabitaciones { get; set; } = new List<HistorialEstadoHabitacion>();

    public virtual ICollection<HistorialTraslado> HistorialTraslados { get; set; } = new List<HistorialTraslado>();

    public virtual RolUsuario IdRolNavigation { get; set; } = null!;

    public virtual ICollection<Incidente> Incidentes { get; set; } = new List<Incidente>();

    public virtual ICollection<MovimientoStock> MovimientoStocks { get; set; } = new List<MovimientoStock>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // Convenience navigation properties (excluded from EF mapping)
    [NotMapped]
    public RolUsuario? Rol => IdRolNavigation;
}
