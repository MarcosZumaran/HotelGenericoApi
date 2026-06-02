using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class MovimientoStockConfiguration : IEntityTypeConfiguration<MovimientoStock>
{
    public void Configure(EntityTypeBuilder<MovimientoStock> builder)
    {
        builder.HasKey(e => e.IdMovimiento);

        builder.ToTable("movimiento_stock");

        builder.Property(e => e.IdMovimiento).HasColumnName("id_movimiento");
        builder.Property(e => e.Cantidad).HasColumnName("cantidad");
        builder.Property(e => e.CodigoTipoMovimiento)
            .HasMaxLength(20)
            .HasColumnName("codigo_tipo_movimiento");
        builder.Property(e => e.CostoUnitario)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("costo_unitario");
        builder.Property(e => e.FechaMovimiento)
            .HasDefaultValueSql("(sysdatetime())", "DF_movimiento_stock_fecha")
            .HasColumnName("fecha_movimiento");
        builder.Property(e => e.IdEstancia).HasColumnName("id_estancia");
        builder.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
        builder.Property(e => e.IdProducto).HasColumnName("id_producto");
        builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
        builder.Property(e => e.IdVenta).HasColumnName("id_venta");
        builder.Property(e => e.Motivo)
            .HasMaxLength(300)
            .HasColumnName("motivo");
        builder.Property(e => e.StockAnterior).HasColumnName("stock_anterior");
        builder.Property(e => e.StockNuevo).HasColumnName("stock_nuevo");

        builder.HasOne(d => d.CodigoTipoMovimientoNavigation).WithMany(p => p.MovimientoStocks)
            .HasForeignKey(d => d.CodigoTipoMovimiento)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_movimiento_stock_tipo");

        builder.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.MovimientoStocks)
            .HasForeignKey(d => d.IdEstancia)
            .HasConstraintName("FK_movimiento_stock_estancia");

        builder.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.MovimientoStocks)
            .HasForeignKey(d => d.IdHabitacion)
            .HasConstraintName("FK_movimiento_stock_habitacion");

        builder.HasOne(d => d.IdProductoNavigation).WithMany(p => p.MovimientoStocks)
            .HasForeignKey(d => d.IdProducto)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_movimiento_stock_producto");

        builder.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.MovimientoStocks)
            .HasForeignKey(d => d.IdUsuario)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_movimiento_stock_usuario");

        builder.HasOne(d => d.IdVentaNavigation).WithMany(p => p.MovimientoStocks)
            .HasForeignKey(d => d.IdVenta)
            .HasConstraintName("FK_movimiento_stock_venta");
    }
}
