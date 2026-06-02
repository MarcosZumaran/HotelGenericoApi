using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string CodigoInterno { get; set; } = null!;

    public string? TipoDocumento { get; set; }

    public string? Documento { get; set; }

    public string? Nombres { get; set; }

    public string? Apellidos { get; set; }

    public string? Alias { get; set; }

    public string Nacionalidad { get; set; } = null!;

    public DateOnly? FechaNacimiento { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaVerificacionReniec { get; set; }

    public virtual ICollection<Estancium> Estancia { get; set; } = new List<Estancium>();

    public virtual ICollection<Huesped> Huespeds { get; set; } = new List<Huesped>();

    public virtual ICollection<ReservaCorporativa> ReservaCorporativas { get; set; } = new List<ReservaCorporativa>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual TipoDocumento? TipoDocumentoNavigation { get; set; }

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
