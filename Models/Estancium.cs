using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class Estancium
{
    public int IdEstancia { get; set; }

    public int? IdReserva { get; set; }

    public int? IdReservaCorporativa { get; set; }

    public int IdHabitacion { get; set; }

    public int IdClienteTitular { get; set; }

    public int IdEstadoEstancia { get; set; }

    public DateTime FechaCheckin { get; set; }

    public DateTime FechaCheckoutPrevista { get; set; }

    public DateTime? FechaCheckoutReal { get; set; }

    public decimal MontoTotal { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool EstaFuera { get; set; }

    public DateTime? HoraSalidaTemporal { get; set; }

    public DateTime? HoraRegresoTemporal { get; set; }

    public bool? LlavesDejadas { get; set; }

    public virtual ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();

    public virtual ICollection<HistorialTraslado> HistorialTraslados { get; set; } = new List<HistorialTraslado>();

    public virtual ICollection<Huesped> Huespeds { get; set; } = new List<Huesped>();

    public virtual Cliente IdClienteTitularNavigation { get; set; } = null!;

    public virtual EstadoEstancium IdEstadoEstanciaNavigation { get; set; } = null!;

    public virtual Habitacion IdHabitacionNavigation { get; set; } = null!;

    public virtual ReservaCorporativa? IdReservaCorporativaNavigation { get; set; }

    public virtual Reserva? IdReservaNavigation { get; set; }

    public virtual ICollection<Incidente> Incidentes { get; set; } = new List<Incidente>();

    public virtual ICollection<ItemEstancium> ItemEstancia { get; set; } = new List<ItemEstancium>();

    public virtual ICollection<MovimientoStock> MovimientoStocks { get; set; } = new List<MovimientoStock>();

    public virtual ICollection<ObjetoPerdido> ObjetoPerdidos { get; set; } = new List<ObjetoPerdido>();
}
