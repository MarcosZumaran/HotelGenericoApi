using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class StockHabitacionConfiguration : IEntityTypeConfiguration<StockHabitacion>
{
    public void Configure(EntityTypeBuilder<StockHabitacion> builder)
    {
        builder.HasKey(e => e.IdStock);

        builder.ToTable("stock_habitacion");

        builder.HasIndex(e => new { e.IdHabitacion, e.IdProducto }, "UQ_stock_habitacion").IsUnique();

        builder.Property(e => e.IdStock).HasColumnName("id_stock");
        builder.Property(e => e.CantidadActual).HasColumnName("cantidad_actual");
        builder.Property(e => e.FechaActualizacion)
            .HasDefaultValueSql("(sysdatetime())", "DF_stock_habitacion_fecha")
            .HasColumnName("fecha_actualizacion");
        builder.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
        builder.Property(e => e.IdProducto).HasColumnName("id_producto");

        builder.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.StockHabitacions)
            .HasForeignKey(d => d.IdHabitacion)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_stock_habitacion_habitacion");

        builder.HasOne(d => d.IdProductoNavigation).WithMany(p => p.StockHabitacions)
            .HasForeignKey(d => d.IdProducto)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_stock_habitacion_producto");
    }
}
