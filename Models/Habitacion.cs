using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class Habitacion
{
    public int IdHabitacion { get; set; }

    public string NumeroHabitacion { get; set; } = null!;

    public int Piso { get; set; }

    public string? Descripcion { get; set; }

    public string? Caracteristicas { get; set; }

    public int IdTipo { get; set; }

    public decimal PrecioNoche { get; set; }

    public int IdEstado { get; set; }

    public DateTime FechaUltimoCambio { get; set; }

    public int? UsuarioCambio { get; set; }

    public virtual ICollection<Estancia> Estancias { get; set; } = new List<Estancia>();

    public virtual ICollection<HabitacionAmenidad> HabitacionAmenidades { get; set; } = new List<HabitacionAmenidad>();

    public virtual ICollection<HistorialEstadoHabitacion> HistorialEstadoHabitaciones { get; set; } = new List<HistorialEstadoHabitacion>();

    public virtual ICollection<HistorialTraslado> HistorialTrasladoIdHabitacionDestinoNavigations { get; set; } = new List<HistorialTraslado>();

    public virtual ICollection<HistorialTraslado> HistorialTrasladoIdHabitacionOrigenNavigations { get; set; } = new List<HistorialTraslado>();

    public virtual EstadoHabitacion IdEstadoNavigation { get; set; } = null!;

    public virtual TipoHabitacion IdTipoNavigation { get; set; } = null!;

    public virtual ICollection<Incidente> Incidentes { get; set; } = new List<Incidente>();

    public virtual ICollection<MovimientoStock> MovimientoStocks { get; set; } = new List<MovimientoStock>();

    public virtual ICollection<ObjetoPerdido> ObjetoPerdidos { get; set; } = new List<ObjetoPerdido>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual ICollection<StockHabitacion> StockHabitacions { get; set; } = new List<StockHabitacion>();

    public virtual Usuario? UsuarioCambioNavigation { get; set; }

    // Convenience navigation properties
    public TipoHabitacion? Tipo => IdTipoNavigation;
    public string? Estado => IdEstadoNavigation?.Nombre;
}
