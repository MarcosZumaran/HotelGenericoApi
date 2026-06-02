using System;
using System.Collections.Generic;
using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelGenericoApi.Data;

public partial class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AfectacionIgv> AfectacionesIgv { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<CategoriaProducto> CategoriasProducto { get; set; }

    public virtual DbSet<CierreCajaEnvio> CierresCajaEnvio { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Comprobante> Comprobantes { get; set; }

    public virtual DbSet<Configuracion> Configuraciones { get; set; }

    public virtual DbSet<EstadoEstancia> EstadosEstancia { get; set; }

    public virtual DbSet<EstadoHabitacion> EstadosHabitacion { get; set; }

    public virtual DbSet<EstadoReserva> EstadosReserva { get; set; }

    public virtual DbSet<EstadoSunat> EstadosSunat { get; set; }

    public virtual DbSet<Estancia> Estancias { get; set; }

    public virtual DbSet<Habitacion> Habitaciones { get; set; }

    public virtual DbSet<HabitacionAmenidad> HabitacionAmenidades { get; set; }

    public virtual DbSet<HistorialEstadoHabitacion> HistorialEstadoHabitaciones { get; set; }

    public virtual DbSet<HistorialTraslado> HistorialTraslados { get; set; }

    public virtual DbSet<Huesped> Huespedes { get; set; }

    public virtual DbSet<Incidente> Incidentes { get; set; }

    public virtual DbSet<ItemEstancia> ItemsEstancia { get; set; }

    public virtual DbSet<ItemVentum> ItemsVenta { get; set; }

    public virtual DbSet<LoginAttempt> LoginAttempts { get; set; }

    public virtual DbSet<MetodoPago> MetodosPago { get; set; }

    public virtual DbSet<MovimientoStock> MovimientosStock { get; set; }

    public virtual DbSet<ObjetoPerdido> ObjetosPerdidos { get; set; }

    public virtual DbSet<ParametroHotel> ParametrosHotel { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<ReservaCorporativa> ReservasCorporativas { get; set; }

    public virtual DbSet<RolUsuario> RolesUsuario { get; set; }

    public virtual DbSet<StockHabitacion> StockHabitaciones { get; set; }

    public virtual DbSet<Tarifa> Tarifas { get; set; }

    public virtual DbSet<Temporadum> Temporada { get; set; }

    public virtual DbSet<TipoComprobante> TiposComprobante { get; set; }

    public virtual DbSet<TipoDocumento> TiposDocumento { get; set; }

    public virtual DbSet<TipoHabitacion> TiposHabitacion { get; set; }

    public virtual DbSet<TipoMovimientoStock> TiposMovimientoStock { get; set; }

    public virtual DbSet<TransicionEstado> TransicionesEstado { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VCierreCajaDiario> VCierreCajaDiario { get; set; }

    public virtual DbSet<VEstadoHabitacion> VEstadoHabitacion { get; set; }

    public virtual DbSet<VOcupacionDiaria> VOcupacionDiaria { get; set; }

    public virtual DbSet<Ventum> Ventas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
