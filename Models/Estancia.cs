namespace HotelGenericoApi.Models;

public class Estancia
{
    public int IdEstancia { get; set; }
    public int? IdReserva { get; set; }
    public int IdHabitacion { get; set; }
    public int IdClienteTitular { get; set; }
    public DateTime FechaCheckin { get; set; }
    public DateTime FechaCheckoutPrevista { get; set; }
    public DateTime? FechaCheckoutReal { get; set; }
    public decimal MontoTotal { get; set; }
    public string? Estado { get; set; }
    public DateTime? CreatedAt { get; set; }

    public bool EstaFuera { get; set; } = false;
    public DateTime? HoraSalidaTemporal { get; set; }
    public DateTime? HoraRegresoTemporal { get; set; }
    public bool? LlavesDejadas { get; set; } // null = nunca salió, true=dejó, false=no dejó
    public int? IdReservaCorporativa { get; set; }
    public ReservaCorporativa? ReservaCorporativa { get; set; }

    // Navegación
    public Reserva? Reserva { get; set; }
    public Habitacion? Habitacion { get; set; }
    public Cliente? ClienteTitular { get; set; }
    public ICollection<ItemEstancia> ItemsEstancia { get; set; } = new List<ItemEstancia>();
    public ICollection<Huesped> Huespedes { get; set; } = new List<Huesped>();
}
