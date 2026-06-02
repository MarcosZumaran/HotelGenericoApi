using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class HabitacionConfiguration : IEntityTypeConfiguration<Habitacion>
{
    public void Configure(EntityTypeBuilder<Habitacion> builder)
    {
        builder.HasKey(e => e.IdHabitacion);

        builder.ToTable("habitacion");

        builder.HasIndex(e => e.NumeroHabitacion, "UQ_habitacion_numero").IsUnique();

        builder.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
        builder.Property(e => e.Caracteristicas).HasColumnName("caracteristicas");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(200)
            .HasColumnName("descripcion");
        builder.Property(e => e.FechaUltimoCambio)
            .HasDefaultValueSql("(sysdatetime())", "DF_habitacion_fecha")
            .HasColumnName("fecha_ultimo_cambio");
        builder.Property(e => e.IdEstado).HasColumnName("id_estado");
        builder.Property(e => e.IdTipo).HasColumnName("id_tipo");
        builder.Property(e => e.NumeroHabitacion)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("numero_habitacion");
        builder.Property(e => e.Piso)
            .HasDefaultValue(1, "DF_habitacion_piso")
            .HasColumnName("piso");
        builder.Property(e => e.PrecioNoche)
            .HasDefaultValue(50.00m, "DF_habitacion_precio")
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("precio_noche");
        builder.Property(e => e.UsuarioCambio).HasColumnName("usuario_cambio");

        builder.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Habitacions)
            .HasForeignKey(d => d.IdEstado)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_habitacion_estado");

        builder.HasOne(d => d.IdTipoNavigation).WithMany(p => p.Habitacions)
            .HasForeignKey(d => d.IdTipo)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_habitacion_tipo");

        builder.HasOne(d => d.UsuarioCambioNavigation).WithMany(p => p.Habitacions)
            .HasForeignKey(d => d.UsuarioCambio)
            .HasConstraintName("FK_habitacion_usuario");
    }
}
