using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class TipoHabitacionConfiguration : IEntityTypeConfiguration<TipoHabitacion>
{
    public void Configure(EntityTypeBuilder<TipoHabitacion> builder)
    {
        builder.HasKey(e => e.IdTipo);

        builder.ToTable("tipo_habitacion");

        builder.HasIndex(e => e.Nombre, "UQ_tipo_habitacion_nombre").IsUnique();

        builder.Property(e => e.IdTipo).HasColumnName("id_tipo");
        builder.Property(e => e.Capacidad)
            .HasDefaultValue(2, "DF_tipo_habitacion_capacidad")
            .HasColumnName("capacidad");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(200)
            .HasColumnName("descripcion");
        builder.Property(e => e.Nombre)
            .HasMaxLength(50)
            .HasColumnName("nombre");
        builder.Property(e => e.PrecioBase)
            .HasDefaultValue(50.00m, "DF_tipo_habitacion_precio")
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("precio_base");
    }
}
