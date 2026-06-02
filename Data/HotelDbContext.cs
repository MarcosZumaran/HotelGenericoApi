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

    public virtual DbSet<AfectacionIgv> AfectacionIgvs { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<CategoriaProducto> CategoriaProductos { get; set; }

    public virtual DbSet<CierreCajaEnvio> CierreCajaEnvios { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Comprobante> Comprobantes { get; set; }

    public virtual DbSet<Configuracion> Configuracions { get; set; }

    public virtual DbSet<EstadoEstancium> EstadoEstancia { get; set; }

    public virtual DbSet<EstadoHabitacion> EstadoHabitacions { get; set; }

    public virtual DbSet<EstadoReserva> EstadoReservas { get; set; }

    public virtual DbSet<EstadoSunat> EstadoSunats { get; set; }

    public virtual DbSet<Estancium> Estancia { get; set; }

    public virtual DbSet<Habitacion> Habitacions { get; set; }

    public virtual DbSet<HabitacionAmenidad> HabitacionAmenidads { get; set; }

    public virtual DbSet<HistorialEstadoHabitacion> HistorialEstadoHabitacions { get; set; }

    public virtual DbSet<HistorialTraslado> HistorialTraslados { get; set; }

    public virtual DbSet<Huesped> Huespeds { get; set; }

    public virtual DbSet<Incidente> Incidentes { get; set; }

    public virtual DbSet<ItemEstancium> ItemEstancia { get; set; }

    public virtual DbSet<ItemVentum> ItemVenta { get; set; }

    public virtual DbSet<LoginAttempt> LoginAttempts { get; set; }

    public virtual DbSet<MetodoPago> MetodoPagos { get; set; }

    public virtual DbSet<MovimientoStock> MovimientoStocks { get; set; }

    public virtual DbSet<ObjetoPerdido> ObjetoPerdidos { get; set; }

    public virtual DbSet<ParametroHotel> ParametroHotels { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<ReservaCorporativa> ReservaCorporativas { get; set; }

    public virtual DbSet<RolUsuario> RolUsuarios { get; set; }

    public virtual DbSet<StockHabitacion> StockHabitacions { get; set; }

    public virtual DbSet<Tarifa> Tarifas { get; set; }

    public virtual DbSet<Temporadum> Temporada { get; set; }

    public virtual DbSet<TipoComprobante> TipoComprobantes { get; set; }

    public virtual DbSet<TipoDocumento> TipoDocumentos { get; set; }

    public virtual DbSet<TipoHabitacion> TipoHabitacions { get; set; }

    public virtual DbSet<TipoMovimientoStock> TipoMovimientoStocks { get; set; }

    public virtual DbSet<TransicionEstado> TransicionEstados { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VCierreCajaDiario> VCierreCajaDiarios { get; set; }

    public virtual DbSet<VEstadoHabitacione> VEstadoHabitaciones { get; set; }

    public virtual DbSet<VOcupacionDiarium> VOcupacionDiaria { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AfectacionIgv>(entity =>
        {
            entity.HasKey(e => e.Codigo);

            entity.ToTable("afectacion_igv");

            entity.Property(e => e.Codigo)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(60)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.IdAudit);

            entity.ToTable("audit_log");

            entity.Property(e => e.IdAudit).HasColumnName("id_audit");
            entity.Property(e => e.Accion)
                .HasMaxLength(20)
                .HasColumnName("accion");
            entity.Property(e => e.DatosAnteriores).HasColumnName("datos_anteriores");
            entity.Property(e => e.DatosNuevos).HasColumnName("datos_nuevos");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(sysdatetime())", "DF_audit_log_fecha")
                .HasColumnName("fecha");
            entity.Property(e => e.IdRegistro)
                .HasMaxLength(100)
                .HasColumnName("id_registro");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");
            entity.Property(e => e.Modulo)
                .HasMaxLength(50)
                .HasColumnName("modulo");
            entity.Property(e => e.Tabla)
                .HasMaxLength(128)
                .HasColumnName("tabla");
            entity.Property(e => e.Usuario)
                .HasMaxLength(100)
                .HasColumnName("usuario");
        });

        modelBuilder.Entity<CategoriaProducto>(entity =>
        {
            entity.HasKey(e => e.IdCategoria);

            entity.ToTable("categoria_producto");

            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.MostrarEnVentas)
                .HasDefaultValue(true, "DF_categoria_producto_mostrar")
                .HasColumnName("mostrar_en_ventas");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CierreCajaEnvio>(entity =>
        {
            entity.HasKey(e => e.Fecha);

            entity.ToTable("cierre_caja_envio");

            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");
            entity.Property(e => e.HashXml)
                .HasMaxLength(64)
                .HasColumnName("hash_xml");
            entity.Property(e => e.IdEstadoSunat)
                .HasDefaultValue(1, "DF_cierre_caja_envio_estado")
                .HasColumnName("id_estado_sunat");
            entity.Property(e => e.IntentosEnvio).HasColumnName("intentos_envio");

            entity.HasOne(d => d.IdEstadoSunatNavigation).WithMany(p => p.CierreCajaEnvios)
                .HasForeignKey(d => d.IdEstadoSunat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cierre_estado_sunat");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente);

            entity.ToTable("cliente");

            entity.HasIndex(e => e.CodigoInterno, "UQ_cliente_codigo").IsUnique();

            entity.HasIndex(e => new { e.TipoDocumento, e.Documento }, "UX_cliente_documento")
                .IsUnique()
                .HasFilter("([tipo_documento] IS NOT NULL AND [documento] IS NOT NULL)");

            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Alias)
                .HasMaxLength(120)
                .HasColumnName("alias");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .HasColumnName("apellidos");
            entity.Property(e => e.CodigoInterno)
                .HasMaxLength(40)
                .HasDefaultValueSql("(concat(N'CLI-',replace(CONVERT([varchar](36),newid()),'-','')))", "DF_cliente_codigo")
                .HasColumnName("codigo_interno");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.Documento)
                .HasMaxLength(20)
                .HasColumnName("documento");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(sysdatetime())", "DF_cliente_fecha")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FechaVerificacionReniec).HasColumnName("fecha_verificacion_reniec");
            entity.Property(e => e.Nacionalidad)
                .HasMaxLength(50)
                .HasDefaultValue("PERUANA", "DF_cliente_nacionalidad")
                .HasColumnName("nacionalidad");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .HasColumnName("nombres");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("tipo_documento");

            entity.HasOne(d => d.TipoDocumentoNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.TipoDocumento)
                .HasConstraintName("FK_cliente_tipo_documento");
        });

        modelBuilder.Entity<Comprobante>(entity =>
        {
            entity.HasKey(e => e.IdComprobante);

            entity.ToTable("comprobante");

            entity.HasIndex(e => new { e.ClienteDocumentoTipo, e.ClienteDocumentoNum }, "IX_comprobante_cliente");

            entity.HasIndex(e => e.FechaEmision, "IX_comprobante_fecha_emision");

            entity.HasIndex(e => new { e.Serie, e.Correlativo }, "UQ_comprobante_serie_correlativo").IsUnique();

            entity.Property(e => e.IdComprobante).HasColumnName("id_comprobante");
            entity.Property(e => e.CdrZip).HasColumnName("cdr_zip");
            entity.Property(e => e.ClienteDocumentoNum)
                .HasMaxLength(20)
                .HasColumnName("cliente_documento_num");
            entity.Property(e => e.ClienteDocumentoTipo)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("cliente_documento_tipo");
            entity.Property(e => e.ClienteNombre)
                .HasMaxLength(200)
                .HasColumnName("cliente_nombre");
            entity.Property(e => e.Correlativo).HasColumnName("correlativo");
            entity.Property(e => e.FechaEmision)
                .HasDefaultValueSql("(sysdatetime())", "DF_comprobante_fecha")
                .HasColumnName("fecha_emision");
            entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");
            entity.Property(e => e.HashXml)
                .HasMaxLength(64)
                .HasColumnName("hash_xml");
            entity.Property(e => e.IdEstadoSunat)
                .HasDefaultValue(1, "DF_comprobante_estado")
                .HasColumnName("id_estado_sunat");
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.IgvMonto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("igv_monto");
            entity.Property(e => e.IntentosEnvio).HasColumnName("intentos_envio");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("metodo_pago");
            entity.Property(e => e.MontoTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_total");
            entity.Property(e => e.Serie)
                .HasMaxLength(4)
                .HasColumnName("serie");
            entity.Property(e => e.TipoComprobante)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("tipo_comprobante");
            entity.Property(e => e.XmlFirmado).HasColumnName("xml_firmado");

            entity.HasOne(d => d.ClienteDocumentoTipoNavigation).WithMany(p => p.Comprobantes)
                .HasForeignKey(d => d.ClienteDocumentoTipo)
                .HasConstraintName("FK_comprobante_cliente_tipo");

            entity.HasOne(d => d.IdEstadoSunatNavigation).WithMany(p => p.Comprobantes)
                .HasForeignKey(d => d.IdEstadoSunat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_comprobante_estado_sunat");

            entity.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.Comprobantes)
                .HasForeignKey(d => d.IdEstancia)
                .HasConstraintName("FK_comprobante_estancia");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.Comprobantes)
                .HasForeignKey(d => d.IdVenta)
                .HasConstraintName("FK_comprobante_venta");

            entity.HasOne(d => d.MetodoPagoNavigation).WithMany(p => p.Comprobantes)
                .HasForeignKey(d => d.MetodoPago)
                .HasConstraintName("FK_comprobante_metodo_pago");

            entity.HasOne(d => d.TipoComprobanteNavigation).WithMany(p => p.Comprobantes)
                .HasForeignKey(d => d.TipoComprobante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_comprobante_tipo");
        });

        modelBuilder.Entity<Configuracion>(entity =>
        {
            entity.HasKey(e => e.IdConfiguracion);

            entity.ToTable("configuracion");

            entity.Property(e => e.IdConfiguracion)
                .HasDefaultValue(1, "DF_configuracion_id")
                .HasColumnName("id_configuracion");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .HasColumnName("direccion");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(sysdatetime())", "DF_configuracion_fecha")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Ruc)
                .HasMaxLength(11)
                .HasColumnName("ruc");
            entity.Property(e => e.TasaIgvHotel)
                .HasDefaultValue(18.00m, "DF_configuracion_igv_hotel")
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("tasa_igv_hotel");
            entity.Property(e => e.TasaIgvProductos)
                .HasDefaultValue(18.00m, "DF_configuracion_igv_productos")
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("tasa_igv_productos");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<EstadoEstancium>(entity =>
        {
            entity.HasKey(e => e.IdEstadoEstancia);

            entity.ToTable("estado_estancia");

            entity.HasIndex(e => e.Codigo, "UQ_estado_estancia_codigo").IsUnique();

            entity.Property(e => e.IdEstadoEstancia).HasColumnName("id_estado_estancia");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.EsFinal).HasColumnName("es_final");
        });

        modelBuilder.Entity<EstadoHabitacion>(entity =>
        {
            entity.HasKey(e => e.IdEstado);

            entity.ToTable("estado_habitacion");

            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.ColorUi)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("color_ui");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.EsEstadoFinal).HasColumnName("es_estado_final");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
            entity.Property(e => e.PermiteCheckin).HasColumnName("permite_checkin");
            entity.Property(e => e.PermiteCheckout).HasColumnName("permite_checkout");
        });

        modelBuilder.Entity<EstadoReserva>(entity =>
        {
            entity.HasKey(e => e.IdEstadoReserva);

            entity.ToTable("estado_reserva");

            entity.HasIndex(e => e.Codigo, "UQ_estado_reserva_codigo").IsUnique();

            entity.Property(e => e.IdEstadoReserva).HasColumnName("id_estado_reserva");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.EsFinal).HasColumnName("es_final");
        });

        modelBuilder.Entity<EstadoSunat>(entity =>
        {
            entity.HasKey(e => e.Codigo);

            entity.ToTable("estado_sunat");

            entity.Property(e => e.Codigo)
                .ValueGeneratedNever()
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(60)
                .HasColumnName("descripcion");
            entity.Property(e => e.DescripcionLarga)
                .HasMaxLength(200)
                .HasColumnName("descripcion_larga");
        });

        modelBuilder.Entity<Estancium>(entity =>
        {
            entity.HasKey(e => e.IdEstancia);

            entity.ToTable("estancia");

            entity.HasIndex(e => e.IdReservaCorporativa, "IX_estancia_id_reserva_corporativa").HasFilter("([id_reserva_corporativa] IS NOT NULL)");

            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())", "DF_estancia_fecha")
                .HasColumnName("created_at");
            entity.Property(e => e.EstaFuera).HasColumnName("esta_fuera");
            entity.Property(e => e.FechaCheckin).HasColumnName("fecha_checkin");
            entity.Property(e => e.FechaCheckoutPrevista).HasColumnName("fecha_checkout_prevista");
            entity.Property(e => e.FechaCheckoutReal).HasColumnName("fecha_checkout_real");
            entity.Property(e => e.HoraRegresoTemporal).HasColumnName("hora_regreso_temporal");
            entity.Property(e => e.HoraSalidaTemporal).HasColumnName("hora_salida_temporal");
            entity.Property(e => e.IdClienteTitular).HasColumnName("id_cliente_titular");
            entity.Property(e => e.IdEstadoEstancia)
                .HasDefaultValue(1, "DF_estancia_estado")
                .HasColumnName("id_estado_estancia");
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.IdReservaCorporativa).HasColumnName("id_reserva_corporativa");
            entity.Property(e => e.LlavesDejadas).HasColumnName("llaves_dejadas");
            entity.Property(e => e.MontoTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_total");

            entity.HasOne(d => d.IdClienteTitularNavigation).WithMany(p => p.Estancia)
                .HasForeignKey(d => d.IdClienteTitular)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_estancia_cliente");

            entity.HasOne(d => d.IdEstadoEstanciaNavigation).WithMany(p => p.Estancia)
                .HasForeignKey(d => d.IdEstadoEstancia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_estancia_estado");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Estancia)
                .HasForeignKey(d => d.IdHabitacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_estancia_habitacion");

            entity.HasOne(d => d.IdReservaNavigation).WithMany(p => p.Estancia)
                .HasForeignKey(d => d.IdReserva)
                .HasConstraintName("FK_estancia_reserva");

            entity.HasOne(d => d.IdReservaCorporativaNavigation).WithMany(p => p.Estancia)
                .HasForeignKey(d => d.IdReservaCorporativa)
                .HasConstraintName("FK_estancia_reserva_corporativa");
        });

        modelBuilder.Entity<Habitacion>(entity =>
        {
            entity.HasKey(e => e.IdHabitacion);

            entity.ToTable("habitacion");

            entity.HasIndex(e => e.NumeroHabitacion, "UQ_habitacion_numero").IsUnique();

            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.Caracteristicas).HasColumnName("caracteristicas");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaUltimoCambio)
                .HasDefaultValueSql("(sysdatetime())", "DF_habitacion_fecha")
                .HasColumnName("fecha_ultimo_cambio");
            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.IdTipo).HasColumnName("id_tipo");
            entity.Property(e => e.NumeroHabitacion)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("numero_habitacion");
            entity.Property(e => e.Piso)
                .HasDefaultValue(1, "DF_habitacion_piso")
                .HasColumnName("piso");
            entity.Property(e => e.PrecioNoche)
                .HasDefaultValue(50.00m, "DF_habitacion_precio")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_noche");
            entity.Property(e => e.UsuarioCambio).HasColumnName("usuario_cambio");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Habitacions)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_habitacion_estado");

            entity.HasOne(d => d.IdTipoNavigation).WithMany(p => p.Habitacions)
                .HasForeignKey(d => d.IdTipo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_habitacion_tipo");

            entity.HasOne(d => d.UsuarioCambioNavigation).WithMany(p => p.Habitacions)
                .HasForeignKey(d => d.UsuarioCambio)
                .HasConstraintName("FK_habitacion_usuario");
        });

        modelBuilder.Entity<HabitacionAmenidad>(entity =>
        {
            entity.HasKey(e => e.IdHabitacionAmenidad);

            entity.ToTable("habitacion_amenidad");

            entity.HasIndex(e => new { e.IdHabitacion, e.IdProducto }, "UQ_habitacion_amenidad").IsUnique();

            entity.Property(e => e.IdHabitacionAmenidad).HasColumnName("id_habitacion_amenidad");
            entity.Property(e => e.CantidadBase)
                .HasDefaultValue(1, "DF_habitacion_amenidad_cantidad")
                .HasColumnName("cantidad_base");
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.HabitacionAmenidads)
                .HasForeignKey(d => d.IdHabitacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_habitacion_amenidad_habitacion");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.HabitacionAmenidads)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_habitacion_amenidad_producto");
        });

        modelBuilder.Entity<HistorialEstadoHabitacion>(entity =>
        {
            entity.HasKey(e => e.IdHistorial);

            entity.ToTable("historial_estado_habitacion");

            entity.HasIndex(e => new { e.IdHabitacion, e.FechaCambio }, "IX_historial_habitacion_fecha").IsDescending(false, true);

            entity.Property(e => e.IdHistorial).HasColumnName("id_historial");
            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("(sysdatetime())", "DF_historial_estado_fecha")
                .HasColumnName("fecha_cambio");
            entity.Property(e => e.IdEstadoAnterior).HasColumnName("id_estado_anterior");
            entity.Property(e => e.IdEstadoNuevo).HasColumnName("id_estado_nuevo");
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Observacion)
                .HasMaxLength(200)
                .HasColumnName("observacion");

            entity.HasOne(d => d.IdEstadoAnteriorNavigation).WithMany(p => p.HistorialEstadoHabitacionIdEstadoAnteriorNavigations)
                .HasForeignKey(d => d.IdEstadoAnterior)
                .HasConstraintName("FK_historial_estado_anterior");

            entity.HasOne(d => d.IdEstadoNuevoNavigation).WithMany(p => p.HistorialEstadoHabitacionIdEstadoNuevoNavigations)
                .HasForeignKey(d => d.IdEstadoNuevo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_historial_estado_nuevo");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.HistorialEstadoHabitacions)
                .HasForeignKey(d => d.IdHabitacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_historial_habitacion");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialEstadoHabitacions)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_historial_usuario");
        });

        modelBuilder.Entity<HistorialTraslado>(entity =>
        {
            entity.HasKey(e => e.IdTraslado);

            entity.ToTable("historial_traslado");

            entity.HasIndex(e => e.IdEstancia, "IX_traslado_estancia");

            entity.HasIndex(e => e.FechaTraslado, "IX_traslado_fecha").IsDescending();

            entity.Property(e => e.IdTraslado).HasColumnName("id_traslado");
            entity.Property(e => e.AjusteMonto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("ajuste_monto");
            entity.Property(e => e.FechaTraslado)
                .HasDefaultValueSql("(sysdatetime())", "DF_historial_traslado_fecha")
                .HasColumnName("fecha_traslado");
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");
            entity.Property(e => e.IdHabitacionDestino).HasColumnName("id_habitacion_destino");
            entity.Property(e => e.IdHabitacionOrigen).HasColumnName("id_habitacion_origen");
            entity.Property(e => e.Motivo)
                .HasMaxLength(200)
                .HasColumnName("motivo");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.HistorialTraslados)
                .HasForeignKey(d => d.IdEstancia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_traslado_estancia");

            entity.HasOne(d => d.IdHabitacionDestinoNavigation).WithMany(p => p.HistorialTrasladoIdHabitacionDestinoNavigations)
                .HasForeignKey(d => d.IdHabitacionDestino)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_traslado_habitacion_destino");

            entity.HasOne(d => d.IdHabitacionOrigenNavigation).WithMany(p => p.HistorialTrasladoIdHabitacionOrigenNavigations)
                .HasForeignKey(d => d.IdHabitacionOrigen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_traslado_habitacion_origen");

            entity.HasOne(d => d.Usuario).WithMany(p => p.HistorialTraslados)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_traslado_usuario");
        });

        modelBuilder.Entity<Huesped>(entity =>
        {
            entity.HasKey(e => e.IdHuesped);

            entity.ToTable("huesped");

            entity.Property(e => e.IdHuesped).HasColumnName("id_huesped");
            entity.Property(e => e.EsTitular).HasColumnName("es_titular");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(sysdatetime())", "DF_huesped_fecha")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Huespeds)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_huesped_cliente");

            entity.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.Huespeds)
                .HasForeignKey(d => d.IdEstancia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_huesped_estancia");
        });

        modelBuilder.Entity<Incidente>(entity =>
        {
            entity.HasKey(e => e.IdIncidente);

            entity.ToTable("incidente");

            entity.HasIndex(e => e.IdEstancia, "IX_incidente_estancia");

            entity.HasIndex(e => new { e.IdHabitacion, e.FechaRegistro }, "IX_incidente_habitacion_fecha").IsDescending(false, true);

            entity.Property(e => e.IdIncidente).HasColumnName("id_incidente");
            entity.Property(e => e.CobradoAlCliente).HasColumnName("cobrado_al_cliente");
            entity.Property(e => e.CostoEstimado)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("costo_estimado");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(sysdatetime())", "DF_incidente_fecha")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(255)
                .HasColumnName("imagen_url");
            entity.Property(e => e.ReportadoPor).HasColumnName("reportado_por");
            entity.Property(e => e.Resuelto).HasColumnName("resuelto");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .HasColumnName("tipo");

            entity.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.Incidentes)
                .HasForeignKey(d => d.IdEstancia)
                .HasConstraintName("FK_incidente_estancia");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Incidentes)
                .HasForeignKey(d => d.IdHabitacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_incidente_habitacion");

            entity.HasOne(d => d.ReportadoPorNavigation).WithMany(p => p.Incidentes)
                .HasForeignKey(d => d.ReportadoPor)
                .HasConstraintName("FK_incidente_usuario");
        });

        modelBuilder.Entity<ItemEstancium>(entity =>
        {
            entity.HasKey(e => e.IdItem);

            entity.ToTable("item_estancia");

            entity.HasIndex(e => e.IdEstancia, "IX_item_estancia_estancia");

            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(sysdatetime())", "DF_item_estancia_fecha")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.Subtotal)
                .HasComputedColumnSql("([cantidad]*[precio_unitario])", true)
                .HasColumnType("decimal(21, 2)")
                .HasColumnName("subtotal");

            entity.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.ItemEstancia)
                .HasForeignKey(d => d.IdEstancia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_item_estancia_estancia");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.ItemEstancia)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_item_estancia_producto");
        });

        modelBuilder.Entity<ItemVentum>(entity =>
        {
            entity.HasKey(e => e.IdItem);

            entity.ToTable("item_venta");

            entity.HasIndex(e => e.IdVenta, "IX_item_venta_venta");

            entity.Property(e => e.IdItem).HasColumnName("id_item");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.Subtotal)
                .HasComputedColumnSql("([cantidad]*[precio_unitario])", true)
                .HasColumnType("decimal(21, 2)")
                .HasColumnName("subtotal");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.ItemVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_item_venta_producto");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.ItemVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_item_venta_venta");
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(e => e.IdLoginAttempt);

            entity.ToTable("login_attempt");

            entity.HasIndex(e => new { e.IpAddress, e.AttemptedAt }, "IX_login_attempt_ip_fecha");

            entity.HasIndex(e => new { e.Username, e.AttemptedAt }, "IX_login_attempt_username_at");

            entity.Property(e => e.IdLoginAttempt).HasColumnName("id_login_attempt");
            entity.Property(e => e.AttemptedAt)
                .HasDefaultValueSql("(sysdatetime())", "DF_login_attempt_fecha")
                .HasColumnName("attempted_at");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");
            entity.Property(e => e.Succeeded).HasColumnName("succeeded");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(e => e.Codigo);

            entity.ToTable("metodo_pago");

            entity.Property(e => e.Codigo)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(60)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<MovimientoStock>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento);

            entity.ToTable("movimiento_stock");

            entity.Property(e => e.IdMovimiento).HasColumnName("id_movimiento");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.CodigoTipoMovimiento)
                .HasMaxLength(20)
                .HasColumnName("codigo_tipo_movimiento");
            entity.Property(e => e.CostoUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("costo_unitario");
            entity.Property(e => e.FechaMovimiento)
                .HasDefaultValueSql("(sysdatetime())", "DF_movimiento_stock_fecha")
                .HasColumnName("fecha_movimiento");
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.Motivo)
                .HasMaxLength(300)
                .HasColumnName("motivo");
            entity.Property(e => e.StockAnterior).HasColumnName("stock_anterior");
            entity.Property(e => e.StockNuevo).HasColumnName("stock_nuevo");

            entity.HasOne(d => d.CodigoTipoMovimientoNavigation).WithMany(p => p.MovimientoStocks)
                .HasForeignKey(d => d.CodigoTipoMovimiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_movimiento_stock_tipo");

            entity.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.MovimientoStocks)
                .HasForeignKey(d => d.IdEstancia)
                .HasConstraintName("FK_movimiento_stock_estancia");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.MovimientoStocks)
                .HasForeignKey(d => d.IdHabitacion)
                .HasConstraintName("FK_movimiento_stock_habitacion");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.MovimientoStocks)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_movimiento_stock_producto");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.MovimientoStocks)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_movimiento_stock_usuario");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.MovimientoStocks)
                .HasForeignKey(d => d.IdVenta)
                .HasConstraintName("FK_movimiento_stock_venta");
        });

        modelBuilder.Entity<ObjetoPerdido>(entity =>
        {
            entity.HasKey(e => e.IdObjeto);

            entity.ToTable("objeto_perdido");

            entity.HasIndex(e => e.Estado, "IX_objeto_estado");

            entity.HasIndex(e => e.FechaHallazgo, "IX_objeto_fecha").IsDescending();

            entity.Property(e => e.IdObjeto).HasColumnName("id_objeto");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.EntregadoA)
                .HasMaxLength(100)
                .HasColumnName("entregado_a");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("pendiente", "DF_objeto_perdido_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaEntregado).HasColumnName("fecha_entregado");
            entity.Property(e => e.FechaHallazgo)
                .HasDefaultValueSql("(sysdatetime())", "DF_objeto_perdido_fecha")
                .HasColumnName("fecha_hallazgo");
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(255)
                .HasColumnName("imagen_url");

            entity.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.ObjetoPerdidos)
                .HasForeignKey(d => d.IdEstancia)
                .HasConstraintName("FK_objeto_perdido_estancia");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.ObjetoPerdidos)
                .HasForeignKey(d => d.IdHabitacion)
                .HasConstraintName("FK_objeto_perdido_habitacion");
        });

        modelBuilder.Entity<ParametroHotel>(entity =>
        {
            entity.HasKey(e => e.IdParametro);

            entity.ToTable("parametro_hotel");

            entity.HasIndex(e => e.Clave, "UQ_parametro_hotel_clave").IsUnique();

            entity.Property(e => e.IdParametro).HasColumnName("id_parametro");
            entity.Property(e => e.Clave)
                .HasMaxLength(100)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(sysdatetime())", "DF_parametro_hotel_fecha")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.Valor)
                .HasMaxLength(500)
                .HasColumnName("valor");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto);

            entity.ToTable("producto");

            entity.HasIndex(e => e.CodigoSunat, "IX_producto_codigo_sunat");

            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.CodigoSunat)
                .HasMaxLength(20)
                .HasColumnName("codigo_sunat");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())", "DF_producto_fecha")
                .HasColumnName("created_at");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.EsAmenidad).HasColumnName("es_amenidad");
            entity.Property(e => e.EsVendibleEnTienda)
                .HasDefaultValue(true, "DF_producto_vendible")
                .HasColumnName("es_vendible_en_tienda");
            entity.Property(e => e.IdAfectacionIgv)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasDefaultValue("10", "DF_producto_afectacion")
                .HasColumnName("id_afectacion_igv");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(255)
                .HasColumnName("imagen_url");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.StockMinimo)
                .HasDefaultValue(5, "DF_producto_stock_min")
                .HasColumnName("stock_minimo");
            entity.Property(e => e.StockPorHabitacion).HasColumnName("stock_por_habitacion");
            entity.Property(e => e.UnidadMedida)
                .HasMaxLength(3)
                .HasDefaultValue("NIU", "DF_producto_unidad")
                .HasColumnName("unidad_medida");

            entity.HasOne(d => d.IdAfectacionIgvNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdAfectacionIgv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_producto_afectacion");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK_producto_categoria");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva);

            entity.ToTable("reserva");

            entity.HasIndex(e => e.IdReservaCorporativa, "IX_reserva_id_reserva_corporativa");

            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.EsNoShow).HasColumnName("es_no_show");
            entity.Property(e => e.FechaEntradaPrevista).HasColumnName("fecha_entrada_prevista");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(sysdatetime())", "DF_reserva_fecha")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FechaSalidaPrevista).HasColumnName("fecha_salida_prevista");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdEstadoReserva)
                .HasDefaultValue(1, "DF_reserva_estado")
                .HasColumnName("id_estado_reserva");
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.IdReservaCorporativa).HasColumnName("id_reserva_corporativa");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.MontoTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_total");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(300)
                .HasColumnName("observaciones");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reserva_cliente");

            entity.HasOne(d => d.IdEstadoReservaNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdEstadoReserva)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reserva_estado");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdHabitacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reserva_habitacion");

            entity.HasOne(d => d.IdReservaCorporativaNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdReservaCorporativa)
                .HasConstraintName("FK_reserva_corporativa");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reserva_usuario");
        });

        modelBuilder.Entity<ReservaCorporativa>(entity =>
        {
            entity.HasKey(e => e.IdReservaCorporativa);

            entity.ToTable("reserva_corporativa");

            entity.Property(e => e.IdReservaCorporativa).HasColumnName("id_reserva_corporativa");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente", "DF_reserva_corporativa_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(sysdatetime())", "DF_reserva_corporativa_fecha")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdClienteEmpresa).HasColumnName("id_cliente_empresa");
            entity.Property(e => e.NumeroHabitaciones).HasColumnName("numero_habitaciones");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(300)
                .HasColumnName("observaciones");

            entity.HasOne(d => d.IdClienteEmpresaNavigation).WithMany(p => p.ReservaCorporativas)
                .HasForeignKey(d => d.IdClienteEmpresa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reserva_corporativa_cliente");
        });

        modelBuilder.Entity<RolUsuario>(entity =>
        {
            entity.HasKey(e => e.IdRol);

            entity.ToTable("rol_usuario");

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<StockHabitacion>(entity =>
        {
            entity.HasKey(e => e.IdStock);

            entity.ToTable("stock_habitacion");

            entity.HasIndex(e => new { e.IdHabitacion, e.IdProducto }, "UQ_stock_habitacion").IsUnique();

            entity.Property(e => e.IdStock).HasColumnName("id_stock");
            entity.Property(e => e.CantidadActual).HasColumnName("cantidad_actual");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(sysdatetime())", "DF_stock_habitacion_fecha")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.StockHabitacions)
                .HasForeignKey(d => d.IdHabitacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_stock_habitacion_habitacion");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.StockHabitacions)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_stock_habitacion_producto");
        });

        modelBuilder.Entity<Tarifa>(entity =>
        {
            entity.HasKey(e => e.IdTarifa);

            entity.ToTable("tarifa");

            entity.Property(e => e.IdTarifa).HasColumnName("id_tarifa");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.IdTemporada).HasColumnName("id_temporada");
            entity.Property(e => e.IdTipoHabitacion).HasColumnName("id_tipo_habitacion");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.IdTemporadaNavigation).WithMany(p => p.Tarifas)
                .HasForeignKey(d => d.IdTemporada)
                .HasConstraintName("FK_tarifa_temporada");

            entity.HasOne(d => d.IdTipoHabitacionNavigation).WithMany(p => p.Tarifas)
                .HasForeignKey(d => d.IdTipoHabitacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tarifa_tipo_habitacion");
        });

        modelBuilder.Entity<Temporadum>(entity =>
        {
            entity.HasKey(e => e.IdTemporada);

            entity.ToTable("temporada");

            entity.Property(e => e.IdTemporada).HasColumnName("id_temporada");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.Multiplicador)
                .HasDefaultValue(1.00m, "DF_temporada_mult")
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("multiplicador");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TipoComprobante>(entity =>
        {
            entity.HasKey(e => e.Codigo);

            entity.ToTable("tipo_comprobante");

            entity.Property(e => e.Codigo)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(60)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<TipoDocumento>(entity =>
        {
            entity.HasKey(e => e.Codigo);

            entity.ToTable("tipo_documento");

            entity.Property(e => e.Codigo)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(60)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<TipoHabitacion>(entity =>
        {
            entity.HasKey(e => e.IdTipo);

            entity.ToTable("tipo_habitacion");

            entity.HasIndex(e => e.Nombre, "UQ_tipo_habitacion_nombre").IsUnique();

            entity.Property(e => e.IdTipo).HasColumnName("id_tipo");
            entity.Property(e => e.Capacidad)
                .HasDefaultValue(2, "DF_tipo_habitacion_capacidad")
                .HasColumnName("capacidad");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.PrecioBase)
                .HasDefaultValue(50.00m, "DF_tipo_habitacion_precio")
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_base");
        });

        modelBuilder.Entity<TipoMovimientoStock>(entity =>
        {
            entity.HasKey(e => e.Codigo);

            entity.ToTable("tipo_movimiento_stock");

            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<TransicionEstado>(entity =>
        {
            entity.HasKey(e => e.IdTransicion);

            entity.ToTable("transicion_estado");

            entity.HasIndex(e => new { e.IdEstadoActual, e.IdEstadoSiguiente }, "UQ_transicion_estado").IsUnique();

            entity.Property(e => e.IdTransicion).HasColumnName("id_transicion");
            entity.Property(e => e.IdEstadoActual).HasColumnName("id_estado_actual");
            entity.Property(e => e.IdEstadoSiguiente).HasColumnName("id_estado_siguiente");

            entity.HasOne(d => d.IdEstadoActualNavigation).WithMany(p => p.TransicionEstadoIdEstadoActualNavigations)
                .HasForeignKey(d => d.IdEstadoActual)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_transicion_actual");

            entity.HasOne(d => d.IdEstadoSiguienteNavigation).WithMany(p => p.TransicionEstadoIdEstadoSiguienteNavigations)
                .HasForeignKey(d => d.IdEstadoSiguiente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_transicion_siguiente");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.ToTable("usuario");

            entity.HasIndex(e => e.Username, "UQ_usuario_username").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.DebeCambiarPassword)
                .HasDefaultValue(true, "DF_usuario_cambio")
                .HasColumnName("debe_cambiar_password");
            entity.Property(e => e.EstaActivo)
                .HasDefaultValue(true, "DF_usuario_activo")
                .HasColumnName("esta_activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())", "DF_usuario_fecha")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_usuario_rol");
        });

        modelBuilder.Entity<VCierreCajaDiario>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_cierre_caja_diario");

            entity.Property(e => e.Concepto)
                .HasMaxLength(9)
                .HasColumnName("concepto");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Ingresos)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("ingresos");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(60)
                .HasColumnName("metodo_pago");
        });

        modelBuilder.Entity<VEstadoHabitacione>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_estado_habitaciones");

            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasColumnName("estado");
            entity.Property(e => e.FechaUltimoCambio).HasColumnName("fecha_ultimo_cambio");
            entity.Property(e => e.NumeroHabitacion)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("numero_habitacion");
            entity.Property(e => e.PrecioNoche)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_noche");
            entity.Property(e => e.TipoHabitacion)
                .HasMaxLength(50)
                .HasColumnName("tipo_habitacion");
        });

        modelBuilder.Entity<VOcupacionDiarium>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_ocupacion_diaria");

            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Ocupadas).HasColumnName("ocupadas");
            entity.Property(e => e.PorcentajeOcupacion)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("porcentaje_ocupacion");
            entity.Property(e => e.Total).HasColumnName("total");
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.IdVenta);

            entity.ToTable("venta");

            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.FechaVenta)
                .HasDefaultValueSql("(sysdatetime())", "DF_venta_fecha")
                .HasColumnName("fecha_venta");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("metodo_pago");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK_venta_cliente");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_venta_usuario");

            entity.HasOne(d => d.MetodoPagoNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.MetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_venta_metodo_pago");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
