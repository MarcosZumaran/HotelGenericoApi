using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class HistorialEstadoHabitacionConfiguration : IEntityTypeConfiguration<HistorialEstadoHabitacion>
{
    public void Configure(EntityTypeBuilder<HistorialEstadoHabitacion> builder)
    {
        builder.HasKey(e => e.IdHistorial);

        builder.ToTable("historial_estado_habitacion");

        builder.HasIndex(e => new { e.IdHabitacion, e.FechaCambio }, "IX_historial_habitacion_fecha").IsDescending(false, true);

        builder.Property(e => e.IdHistorial).HasColumnName("id_historial");
        builder.Property(e => e.FechaCambio)
            .HasDefaultValueSql("(sysdatetime())", "DF_historial_estado_fecha")
            .HasColumnName("fecha_cambio");
        builder.Property(e => e.IdEstadoAnterior).HasColumnName("id_estado_anterior");
        builder.Property(e => e.IdEstadoNuevo).HasColumnName("id_estado_nuevo");
        builder.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
        builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
        builder.Property(e => e.Observacion)
            .HasMaxLength(200)
            .HasColumnName("observacion");

        builder.HasOne(d => d.IdEstadoAnteriorNavigation).WithMany(p => p.HistorialEstadoHabitacionIdEstadoAnteriorNavigations)
            .HasForeignKey(d => d.IdEstadoAnterior)
            .HasConstraintName("FK_historial_estado_anterior");

        builder.HasOne(d => d.IdEstadoNuevoNavigation).WithMany(p => p.HistorialEstadoHabitacionIdEstadoNuevoNavigations)
            .HasForeignKey(d => d.IdEstadoNuevo)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_historial_estado_nuevo");

        builder.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.HistorialEstadoHabitaciones)
            .HasForeignKey(d => d.IdHabitacion)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_historial_habitacion");

        builder.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialEstadoHabitaciones)
            .HasForeignKey(d => d.IdUsuario)
            .HasConstraintName("FK_historial_usuario");
    }
}
