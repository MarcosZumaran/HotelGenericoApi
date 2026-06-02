using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class HabitacionAmenidadConfiguration : IEntityTypeConfiguration<HabitacionAmenidad>
{
    public void Configure(EntityTypeBuilder<HabitacionAmenidad> builder)
    {
        builder.HasKey(e => e.IdHabitacionAmenidad);

        builder.ToTable("habitacion_amenidad");

        builder.HasIndex(e => new { e.IdHabitacion, e.IdProducto }, "UQ_habitacion_amenidad").IsUnique();

        builder.Property(e => e.IdHabitacionAmenidad).HasColumnName("id_habitacion_amenidad");
        builder.Property(e => e.CantidadBase)
            .HasDefaultValue(1, "DF_habitacion_amenidad_cantidad")
            .HasColumnName("cantidad_base");
        builder.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
        builder.Property(e => e.IdProducto).HasColumnName("id_producto");

        builder.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.HabitacionAmenidades)
            .HasForeignKey(d => d.IdHabitacion)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_habitacion_amenidad_habitacion");

        builder.HasOne(d => d.IdProductoNavigation).WithMany(p => p.HabitacionAmenidades)
            .HasForeignKey(d => d.IdProducto)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_habitacion_amenidad_producto");
    }
}
