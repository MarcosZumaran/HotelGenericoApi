using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Models;

namespace HotelGenericoApi.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    // Tablas
    public DbSet<Configuracion> Configuraciones => Set<Configuracion>();
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<TipoComprobante> TiposComprobante => Set<TipoComprobante>();
    public DbSet<AfectacionIgv> AfectacionesIgv => Set<AfectacionIgv>();
    public DbSet<CategoriaProducto> CategoriasProducto => Set<CategoriaProducto>();
    public DbSet<EstadoHabitacion> EstadosHabitacion => Set<EstadoHabitacion>();
    public DbSet<RolUsuario> RolesUsuario => Set<RolUsuario>();
    public DbSet<EstadoSunat> EstadosSunat => Set<EstadoSunat>();
    public DbSet<Temporada> Temporadas => Set<Temporada>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<TipoHabitacion> TiposHabitacion => Set<TipoHabitacion>();
    public DbSet<Tarifa> Tarifas => Set<Tarifa>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<HistorialEstadoHabitacion> HistorialEstadoHabitaciones => Set<HistorialEstadoHabitacion>();
    public DbSet<TransicionEstado> TransicionesEstado => Set<TransicionEstado>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Estancia> Estancias => Set<Estancia>();
    public DbSet<Huesped> Huespedes => Set<Huesped>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<StockHabitacion> StockHabitaciones => Set<StockHabitacion>();
    public DbSet<ItemEstancia> ItemsEstancia => Set<ItemEstancia>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<ItemVenta> ItemsVenta => Set<ItemVenta>();
    public DbSet<Comprobante> Comprobantes => Set<Comprobante>();
    public DbSet<CierreCajaEnvio> CierresCajaEnvio => Set<CierreCajaEnvio>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

    // Vistas keyless
    public DbSet<VCierreCajaDiario> VCierreCajaDiario => Set<VCierreCajaDiario>();
    public DbSet<VEstadoHabitacion> VEstadoHabitacion => Set<VEstadoHabitacion>();
    public DbSet<VOcupacionDiaria> VOcupacionDiaria => Set<VOcupacionDiaria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Evitar múltiples rutas en cascada en SQL Server
        foreach (var fk in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            fk.DeleteBehavior = DeleteBehavior.NoAction;
        }

        // --- Configuración de cada tabla con Fluent API ---

        modelBuilder.Entity<Configuracion>(entity =>
        {
            entity.ToTable("configuracion");
            entity.HasKey(e => e.IdConfiguracion);
            entity.Property(e => e.IdConfiguracion).HasColumnName("id_configuracion").HasDefaultValue(1);
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Direccion).HasColumnName("direccion").HasMaxLength(200);
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(20);
            entity.Property(e => e.Ruc).HasColumnName("ruc").HasMaxLength(11);
            entity.Property(e => e.TasaIgvHotel).HasColumnName("tasa_igv_hotel").HasColumnType("decimal(5,2)").HasDefaultValue(18.00m);
            entity.Property(e => e.TasaIgvProductos).HasColumnName("tasa_igv_productos").HasColumnType("decimal(5,2)").HasDefaultValue(18.00m);
        });

        modelBuilder.Entity<TipoDocumento>(entity =>
        {
            entity.ToTable("tipo_documento");
            entity.HasKey(e => e.Codigo);
            entity.Property(e => e.Codigo).HasColumnName("codigo").HasColumnType("char(1)");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").IsRequired().HasMaxLength(60);
        });

        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.ToTable("metodo_pago");
            entity.HasKey(e => e.Codigo);
            entity.Property(e => e.Codigo).HasColumnName("codigo").HasColumnType("char(3)");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").IsRequired().HasMaxLength(60);
        });

        modelBuilder.Entity<TipoComprobante>(entity =>
        {
            entity.ToTable("tipo_comprobante");
            entity.HasKey(e => e.Codigo);
            entity.Property(e => e.Codigo).HasColumnName("codigo").HasColumnType("char(2)");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").IsRequired().HasMaxLength(60);
        });

        modelBuilder.Entity<AfectacionIgv>(entity =>
        {
            entity.ToTable("afectacion_igv");
            entity.HasKey(e => e.Codigo);
            entity.Property(e => e.Codigo).HasColumnName("codigo").HasColumnType("char(2)");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").IsRequired().HasMaxLength(60);
        });

        modelBuilder.Entity<CategoriaProducto>(entity =>
        {
            entity.ToTable("categoria_producto");
            entity.HasKey(e => e.IdCategoria);
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria").ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(100);
            entity.Property(e => e.MostrarEnVentas).HasColumnName("mostrar_en_ventas").IsRequired().HasDefaultValue(true);
        });

        modelBuilder.Entity<EstadoHabitacion>(entity =>
        {
            entity.ToTable("estado_habitacion");
            entity.HasKey(e => e.IdEstado);
            entity.Property(e => e.IdEstado).HasColumnName("id_estado").ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(30);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(100);
            entity.Property(e => e.PermiteCheckin).HasColumnName("permite_checkin");
            entity.Property(e => e.PermiteCheckout).HasColumnName("permite_checkout");
            entity.Property(e => e.EsEstadoFinal).HasColumnName("es_estado_final");
            entity.Property(e => e.ColorUi).HasColumnName("color_ui").HasMaxLength(20);
        });

        modelBuilder.Entity<RolUsuario>(entity =>
        {
            entity.ToTable("rol_usuario");
            entity.HasKey(e => e.IdRol);
            entity.Property(e => e.IdRol).HasColumnName("id_rol").ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(30);
        });

        modelBuilder.Entity<EstadoSunat>(entity =>
        {
            entity.ToTable("estado_sunat");
            entity.HasKey(e => e.Codigo);
            entity.Property(e => e.Codigo).HasColumnName("codigo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").IsRequired().HasMaxLength(60);
            entity.Property(e => e.DescripcionLarga).HasColumnName("descripcion_larga").HasMaxLength(200);
        });

        modelBuilder.Entity<Temporada>(entity =>
        {
            entity.ToTable("temporada");
            entity.HasKey(e => e.IdTemporada);
            entity.Property(e => e.IdTemporada).HasColumnName("id_temporada").ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(50);
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("date");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin").HasColumnType("date");
            entity.Property(e => e.Multiplicador).HasColumnName("multiplicador").HasColumnType("decimal(3,2)").HasDefaultValue(1.00m);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuario");
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario").ValueGeneratedOnAdd();
            entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired().HasMaxLength(255);
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.EstaActivo).HasColumnName("esta_activo").HasDefaultValue(true);
            entity.Property(e => e.DebeCambiarPassword).HasColumnName("debe_cambiar_password").HasDefaultValue(true);
            entity.HasOne(e => e.Rol).WithMany().HasForeignKey(e => e.IdRol).HasConstraintName("fk_usuario_rol");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("cliente");
            entity.HasKey(e => e.IdCliente);
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente").ValueGeneratedOnAdd();
            entity.Property(e => e.TipoDocumento).HasColumnName("tipo_documento").HasColumnType("char(1)");
            entity.Property(e => e.Documento).HasColumnName("documento").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Nombres).HasColumnName("nombres").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellidos).HasColumnName("apellidos").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Nacionalidad).HasColumnName("nacionalidad").HasMaxLength(50).HasDefaultValue("PERUANA");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento").HasColumnType("date");
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(15);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.Direccion).HasColumnName("direccion").HasMaxLength(200);
            entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaVerificacionReniec).HasColumnName("fecha_verificacion_reniec");
            entity.HasOne(e => e.TipoDocumentoNavigation).WithMany().HasForeignKey(e => e.TipoDocumento).HasConstraintName("fk_cliente_tipo_documento");
            entity.HasIndex(e => new { e.TipoDocumento, e.Documento }).IsUnique().HasDatabaseName("uq_cliente_documento");
        });

        modelBuilder.Entity<TipoHabitacion>(entity =>
        {
            entity.ToTable("tipo_habitacion");
            entity.HasKey(e => e.IdTipo);
            entity.Property(e => e.IdTipo).HasColumnName("id_tipo").ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Capacidad).HasColumnName("capacidad").HasDefaultValue(2);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
            entity.Property(e => e.PrecioBase).HasColumnName("precio_base").HasColumnType("decimal(10,2)").HasDefaultValue(50.00m);
        });

        modelBuilder.Entity<Tarifa>(entity =>
        {
            entity.ToTable("tarifa");
            entity.HasKey(e => e.IdTarifa);
            entity.Property(e => e.IdTarifa).HasColumnName("id_tarifa").ValueGeneratedOnAdd();
            entity.Property(e => e.IdTipoHabitacion).HasColumnName("id_tipo_habitacion");
            entity.Property(e => e.IdTemporada).HasColumnName("id_temporada");
            entity.Property(e => e.Precio).HasColumnName("precio").HasColumnType("decimal(10,2)");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio").HasColumnType("date");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin").HasColumnType("date");
            entity.HasOne(e => e.TipoHabitacion).WithMany().HasForeignKey(e => e.IdTipoHabitacion).HasConstraintName("fk_tarifa_tipo_habitacion");
            entity.HasOne(e => e.Temporada).WithMany().HasForeignKey(e => e.IdTemporada).HasConstraintName("fk_tarifa_temporada");
        });

        modelBuilder.Entity<Habitacion>(entity =>
        {
            entity.ToTable("habitacion");
            entity.HasKey(e => e.IdHabitacion);
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion").ValueGeneratedOnAdd();
            entity.Property(e => e.NumeroHabitacion).HasColumnName("numero_habitacion").IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.NumeroHabitacion).IsUnique().HasDatabaseName("uq_habitacion_numero");
            entity.Property(e => e.Piso).HasColumnName("piso").HasDefaultValue(1);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
            entity.Property(e => e.IdTipo).HasColumnName("id_tipo");
            entity.Property(e => e.PrecioNoche).HasColumnName("precio_noche").HasColumnType("decimal(10,2)").HasDefaultValue(50.00m);
            entity.Property(e => e.IdEstado).HasColumnName("id_estado").HasDefaultValue(1);
            entity.Property(e => e.FechaUltimoCambio).HasColumnName("fecha_ultimo_cambio").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UsuarioCambio).HasColumnName("usuario_cambio");
            entity.HasOne(e => e.Tipo).WithMany().HasForeignKey(e => e.IdTipo).HasConstraintName("fk_habitacion_tipo");
            entity.HasOne(e => e.Estado).WithMany().HasForeignKey(e => e.IdEstado).HasConstraintName("fk_habitacion_estado");
            entity.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.UsuarioCambio).HasConstraintName("fk_habitacion_usuario");
        });

        modelBuilder.Entity<HistorialEstadoHabitacion>(entity =>
        {
            entity.ToTable("historial_estado_habitacion");
            entity.HasKey(e => e.IdHistorial);
            entity.Property(e => e.IdHistorial).HasColumnName("id_historial").ValueGeneratedOnAdd();
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.IdEstadoAnterior).HasColumnName("id_estado_anterior");
            entity.Property(e => e.IdEstadoNuevo).HasColumnName("id_estado_nuevo");
            entity.Property(e => e.FechaCambio).HasColumnName("fecha_cambio").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Observacion).HasColumnName("observacion").HasMaxLength(200);
            entity.HasOne(e => e.Habitacion).WithMany().HasForeignKey(e => e.IdHabitacion).HasConstraintName("fk_historial_habitacion");
            entity.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario).HasConstraintName("fk_historial_usuario");
            entity.HasIndex(e => new { e.IdHabitacion, e.FechaCambio }).HasDatabaseName("ix_historial_habitacion_fecha").IsDescending(false, true);
        });

        modelBuilder.Entity<TransicionEstado>(entity =>
        {
            entity.ToTable("transicion_estado");
            entity.HasKey(e => e.IdTransicion);
            entity.Property(e => e.IdTransicion).HasColumnName("id_transicion").ValueGeneratedOnAdd();
            entity.Property(e => e.IdEstadoActual).HasColumnName("id_estado_actual");
            entity.Property(e => e.IdEstadoSiguiente).HasColumnName("id_estado_siguiente");
            entity.HasIndex(e => new { e.IdEstadoActual, e.IdEstadoSiguiente }).IsUnique().HasDatabaseName("uq_transicion");
            entity.HasOne(e => e.EstadoActual).WithMany().HasForeignKey(e => e.IdEstadoActual).HasConstraintName("fk_transicion_actual").OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.EstadoSiguiente).WithMany().HasForeignKey(e => e.IdEstadoSiguiente).HasConstraintName("fk_transicion_siguiente").OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.ToTable("reserva");
            entity.HasKey(e => e.IdReserva);
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva").ValueGeneratedOnAdd();
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaEntradaPrevista).HasColumnName("fecha_entrada_prevista").HasColumnType("datetime");
            entity.Property(e => e.FechaSalidaPrevista).HasColumnName("fecha_salida_prevista").HasColumnType("datetime");
            entity.Property(e => e.MontoTotal).HasColumnName("monto_total").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("Pendiente");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
            entity.Property(e => e.EsNoShow).HasColumnName("es_no_show");
            entity.HasOne(e => e.Cliente).WithMany().HasForeignKey(e => e.IdCliente).HasConstraintName("fk_reserva_cliente");
            entity.HasOne(e => e.Habitacion).WithMany(h => h.Reservas).HasForeignKey(e => e.IdHabitacion).HasConstraintName("fk_reserva_habitacion");
            entity.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario).HasConstraintName("fk_reserva_usuario");
        });

        modelBuilder.Entity<Estancia>(entity =>
        {
            entity.ToTable("estancia");
            entity.HasKey(e => e.IdEstancia);
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia").ValueGeneratedOnAdd();
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.IdClienteTitular).HasColumnName("id_cliente_titular");
            entity.Property(e => e.FechaCheckin).HasColumnName("fecha_checkin").HasColumnType("datetime");
            entity.Property(e => e.FechaCheckoutPrevista).HasColumnName("fecha_checkout_prevista").HasColumnType("datetime");
            entity.Property(e => e.FechaCheckoutReal).HasColumnName("fecha_checkout_real").HasColumnType("datetime");
            entity.Property(e => e.MontoTotal).HasColumnName("monto_total").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("Activa");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.EstaFuera).HasColumnName("esta_fuera");
            entity.Property(e => e.HoraSalidaTemporal).HasColumnName("hora_salida_temporal");
            entity.Property(e => e.HoraRegresoTemporal).HasColumnName("hora_regreso_temporal");
            entity.Property(e => e.LlavesDejadas).HasColumnName("llaves_dejadas");
            entity.HasOne(e => e.Reserva).WithMany().HasForeignKey(e => e.IdReserva).HasConstraintName("fk_estancia_reserva");
            entity.HasOne(e => e.Habitacion).WithMany(h => h.Estancias).HasForeignKey(e => e.IdHabitacion).HasConstraintName("fk_estancia_habitacion");
            entity.HasOne(e => e.ClienteTitular).WithMany().HasForeignKey(e => e.IdClienteTitular).HasConstraintName("fk_estancia_cliente");
        });

        modelBuilder.Entity<Huesped>(entity =>
        {
            entity.ToTable("huesped");
            entity.HasKey(e => e.IdHuesped);
            entity.Property(e => e.IdHuesped).HasColumnName("id_huesped").ValueGeneratedOnAdd();
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.EsTitular).HasColumnName("es_titular").HasDefaultValue(false);
            entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("GETDATE()");
            entity.HasOne(e => e.Estancia).WithMany(e => e.Huespedes).HasForeignKey(e => e.IdEstancia).HasConstraintName("fk_huesped_estancia");
            entity.HasOne(e => e.Cliente).WithMany().HasForeignKey(e => e.IdCliente).HasConstraintName("fk_huesped_cliente");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("producto");
            entity.HasKey(e => e.IdProducto);
            entity.Property(e => e.IdProducto).HasColumnName("id_producto").ValueGeneratedOnAdd();
            entity.Property(e => e.CodigoSunat).HasColumnName("codigo_sunat").HasMaxLength(20);
            entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(200);
            entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("decimal(10,2)");
            entity.Property(e => e.IdAfectacionIgv).HasColumnName("id_afectacion_igv").HasColumnType("char(2)").HasDefaultValue("10");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Stock).HasColumnName("stock").HasDefaultValue(0);
            entity.Property(e => e.StockMinimo).HasColumnName("stock_minimo").HasDefaultValue(5);
            entity.Property(e => e.UnidadMedida).HasColumnName("unidad_medida").HasMaxLength(3).HasDefaultValue("NIU");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            entity.HasOne(e => e.AfectacionIgv).WithMany().HasForeignKey(e => e.IdAfectacionIgv).HasConstraintName("fk_producto_afectacion");
            entity.HasOne(e => e.Categoria).WithMany().HasForeignKey(e => e.IdCategoria).HasConstraintName("fk_producto_categoria");
            entity.HasIndex(e => e.CodigoSunat).HasDatabaseName("ix_producto_codigo_sunat");
            entity.Property(e => e.ImagenUrl).HasColumnName("imagen_url").HasMaxLength(255);
            entity.Property(e => e.EsAmenidad).HasColumnName("es_amenidad");
            entity.Property(e => e.EsVendibleEnTienda).HasColumnName("es_vendible_en_tienda");
            entity.Property(e => e.StockPorHabitacion).HasColumnName("stock_por_habitacion");
        });

        modelBuilder.Entity<StockHabitacion>(entity =>
        {
            entity.ToTable("stock_habitacion");
            entity.HasKey(e => e.IdStock);
            entity.Property(e => e.IdStock).HasColumnName("id_stock").ValueGeneratedOnAdd();
            entity.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.CantidadActual).HasColumnName("cantidad_actual");

            entity.HasIndex(e => new { e.IdHabitacion, e.IdProducto })
                .IsUnique()
                .HasDatabaseName("uq_stock_habitacion");

            entity.HasOne(e => e.Habitacion)
                .WithMany()
                .HasForeignKey(e => e.IdHabitacion)
                .HasConstraintName("fk_stock_habitacion_habitacion");

            entity.HasOne(e => e.Producto)
                .WithMany()
                .HasForeignKey(e => e.IdProducto)
                .HasConstraintName("fk_stock_habitacion_producto");
        });

        modelBuilder.Entity<ItemEstancia>(entity =>
        {
            entity.ToTable("item_estancia");
            entity.HasKey(e => e.IdItem);
            entity.Property(e => e.IdItem).HasColumnName("id_item").ValueGeneratedOnAdd();
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(10,2)").HasComputedColumnSql("(cantidad * precio_unitario)", stored: true);
            entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro").HasDefaultValueSql("GETDATE()");
            entity.HasOne(e => e.Estancia).WithMany(e => e.ItemsEstancia).HasForeignKey(e => e.IdEstancia).HasConstraintName("fk_item_estancia_estancia");
            entity.HasOne(e => e.Producto).WithMany().HasForeignKey(e => e.IdProducto).HasConstraintName("fk_item_estancia_producto");
            entity.HasIndex(e => e.IdEstancia).HasDatabaseName("ix_item_estancia_estancia");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.ToTable("venta");
            entity.HasKey(e => e.IdVenta);
            entity.Property(e => e.IdVenta).HasColumnName("id_venta").ValueGeneratedOnAdd();
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.FechaVenta).HasColumnName("fecha_venta").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Total).HasColumnName("total").HasColumnType("decimal(10,2)");
            entity.Property(e => e.MetodoPago).HasColumnName("metodo_pago").HasColumnType("char(3)");
            entity.HasOne(e => e.Cliente).WithMany().HasForeignKey(e => e.IdCliente).HasConstraintName("fk_venta_cliente");
            entity.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario).HasConstraintName("fk_venta_usuario");
            entity.HasOne(e => e.MetodoPagoNavigation).WithMany().HasForeignKey(e => e.MetodoPago).HasConstraintName("fk_venta_metodo_pago");
        });

        modelBuilder.Entity<ItemVenta>(entity =>
        {
            entity.ToTable("item_venta");
            entity.HasKey(e => e.IdItem);
            entity.Property(e => e.IdItem).HasColumnName("id_item").ValueGeneratedOnAdd();
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(10,2)").HasComputedColumnSql("(cantidad * precio_unitario)", stored: true);
            entity.HasOne(e => e.Venta).WithMany(v => v.ItemsVenta).HasForeignKey(e => e.IdVenta).HasConstraintName("fk_item_venta_venta");
            entity.HasOne(e => e.Producto).WithMany().HasForeignKey(e => e.IdProducto).HasConstraintName("fk_item_venta_producto");
            entity.HasIndex(e => e.IdVenta).HasDatabaseName("ix_item_venta_venta");
        });

        modelBuilder.Entity<Comprobante>(entity =>
        {
            entity.ToTable("comprobante");
            entity.HasKey(e => e.IdComprobante);
            entity.Property(e => e.IdComprobante).HasColumnName("id_comprobante").ValueGeneratedOnAdd();
            entity.Property(e => e.IdEstancia).HasColumnName("id_estancia");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.TipoComprobante).HasColumnName("tipo_comprobante").HasColumnType("char(2)");
            entity.Property(e => e.Serie).HasColumnName("serie").IsRequired().HasMaxLength(4);
            entity.Property(e => e.Correlativo).HasColumnName("correlativo");
            entity.Property(e => e.FechaEmision).HasColumnName("fecha_emision").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.MontoTotal).HasColumnName("monto_total").HasColumnType("decimal(10,2)");
            entity.Property(e => e.IgvMonto).HasColumnName("igv_monto").HasColumnType("decimal(10,2)");
            entity.Property(e => e.ClienteDocumentoTipo).HasColumnName("cliente_documento_tipo").HasColumnType("char(1)");
            entity.Property(e => e.ClienteDocumentoNum).HasColumnName("cliente_documento_num").HasMaxLength(20);
            entity.Property(e => e.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200);
            entity.Property(e => e.MetodoPago).HasColumnName("metodo_pago").HasColumnType("char(3)");
            entity.Property(e => e.IdEstadoSunat).HasColumnName("id_estado_sunat").HasDefaultValue(1);
            entity.Property(e => e.XmlFirmado).HasColumnName("xml_firmado").HasColumnType("nvarchar(max)");
            entity.Property(e => e.CdrZip).HasColumnName("cdr_zip").HasColumnType("varbinary(max)");
            entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");
            entity.Property(e => e.IntentosEnvio).HasColumnName("intentos_envio").HasDefaultValue(0);
            entity.Property(e => e.HashXml).HasColumnName("hash_xml").HasMaxLength(64);
            entity.HasIndex(e => new { e.Serie, e.Correlativo }).IsUnique().HasDatabaseName("uq_comprobante_serie_correlativo");
            entity.HasOne(e => e.Estancia).WithMany().HasForeignKey(e => e.IdEstancia).HasConstraintName("fk_comprobante_estancia");
            entity.HasOne(e => e.Venta).WithMany().HasForeignKey(e => e.IdVenta).HasConstraintName("fk_comprobante_venta");
            entity.HasOne(e => e.TipoComprobanteNavigation).WithMany().HasForeignKey(e => e.TipoComprobante).HasConstraintName("fk_comprobante_tipo");
            entity.HasOne(e => e.ClienteDocumentoTipoNavigation).WithMany().HasForeignKey(e => e.ClienteDocumentoTipo).HasConstraintName("fk_comprobante_cliente_tipo");
            entity.HasOne(e => e.MetodoPagoNavigation).WithMany().HasForeignKey(e => e.MetodoPago).HasConstraintName("fk_comprobante_metodo_pago");
            entity.HasOne(e => e.EstadoSunat).WithMany().HasForeignKey(e => e.IdEstadoSunat).HasConstraintName("fk_comprobante_estado_sunat");
            entity.HasIndex(e => e.FechaEmision).HasDatabaseName("ix_comprobante_fecha_emision");
            entity.HasIndex(e => new { e.ClienteDocumentoTipo, e.ClienteDocumentoNum }).HasDatabaseName("ix_comprobante_cliente");
        });

        modelBuilder.Entity<CierreCajaEnvio>(entity =>
        {
            entity.ToTable("cierre_caja_envio");
            entity.HasKey(e => e.Fecha);
            entity.Property(e => e.Fecha).HasColumnName("fecha").HasColumnType("date");
            entity.Property(e => e.IdEstadoSunat).HasColumnName("id_estado_sunat").HasDefaultValue(1);
            entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");
            entity.Property(e => e.IntentosEnvio).HasColumnName("intentos_envio").HasDefaultValue(0);
            entity.Property(e => e.HashXml).HasColumnName("hash_xml").HasMaxLength(64);
            entity.HasOne(e => e.EstadoSunat).WithMany().HasForeignKey(e => e.IdEstadoSunat).HasConstraintName("fk_cierre_estado_sunat");
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.ToTable("login_attempt");
            entity.HasKey(e => e.IdLoginAttempt);
            entity.Property(e => e.IdLoginAttempt).HasColumnName("id_login_attempt").ValueGeneratedOnAdd();
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").IsRequired().HasMaxLength(50);
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100);
            entity.Property(e => e.AttemptedAt).HasColumnName("attempted_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Succeeded).HasColumnName("succeeded");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            entity.HasIndex(e => new { e.IpAddress, e.AttemptedAt }).HasDatabaseName("ix_login_attempt_ip_fecha");
            entity.HasIndex(e => new { e.Username, e.AttemptedAt }).HasDatabaseName("IX_login_attempt_username_at");
        });

        // Vistas keyless
        modelBuilder.Entity<VCierreCajaDiario>(entity =>
        {
            entity.ToView("v_cierre_caja_diario");
            entity.HasNoKey();
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.MetodoPago).HasColumnName("metodo_pago");
            entity.Property(e => e.Ingresos).HasColumnName("ingresos");
            entity.Property(e => e.Concepto).HasColumnName("concepto");
        });

        modelBuilder.Entity<VEstadoHabitacion>(entity =>
        {
            entity.ToView("v_estado_habitaciones");
            entity.HasNoKey();
            entity.Property(e => e.NumeroHabitacion).HasColumnName("numero_habitacion");
            entity.Property(e => e.TipoHabitacion).HasColumnName("tipo_habitacion");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.PrecioNoche).HasColumnName("precio_noche");
            entity.Property(e => e.FechaUltimoCambio).HasColumnName("fecha_ultimo_cambio");
        });

        modelBuilder.Entity<VOcupacionDiaria>(entity =>
        {
            entity.ToView("v_ocupacion_diaria");
            entity.HasNoKey();
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Ocupadas).HasColumnName("ocupadas");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.PorcentajeOcupacion).HasColumnName("porcentaje_ocupacion");
        });
    }
}
