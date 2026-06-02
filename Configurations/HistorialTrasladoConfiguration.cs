using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class HistorialTrasladoConfiguration : IEntityTypeConfiguration<HistorialTraslado>
{
    public void Configure(EntityTypeBuilder<HistorialTraslado> builder)
    {
        builder.HasKey(e => e.IdTraslado);

        builder.ToTable("historial_traslado");

        builder.HasIndex(e => e.IdEstancia, "IX_traslado_estancia");

        builder.HasIndex(e => e.FechaTraslado, "IX_traslado_fecha").IsDescending();

        builder.Property(e => e.IdTraslado).HasColumnName("id_traslado");
        builder.Property(e => e.AjusteMonto)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("ajuste_monto");
        builder.Property(e => e.FechaTraslado)
            .HasDefaultValueSql("(sysdatetime())", "DF_historial_traslado_fecha")
            .HasColumnName("fecha_traslado");
        builder.Property(e => e.IdEstancia).HasColumnName("id_estancia");
        builder.Property(e => e.IdHabitacionDestino).HasColumnName("id_habitacion_destino");
        builder.Property(e => e.IdHabitacionOrigen).HasColumnName("id_habitacion_origen");
        builder.Property(e => e.Motivo)
            .HasMaxLength(200)
            .HasColumnName("motivo");
        builder.Property(e => e.UsuarioId).HasColumnName("usuario_id");

        builder.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.HistorialTraslados)
            .HasForeignKey(d => d.IdEstancia)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_traslado_estancia");

        builder.HasOne(d => d.IdHabitacionDestinoNavigation).WithMany(p => p.HistorialTrasladoIdHabitacionDestinoNavigations)
            .HasForeignKey(d => d.IdHabitacionDestino)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_traslado_habitacion_destino");

        builder.HasOne(d => d.IdHabitacionOrigenNavigation).WithMany(p => p.HistorialTrasladoIdHabitacionOrigenNavigations)
            .HasForeignKey(d => d.IdHabitacionOrigen)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_traslado_habitacion_origen");

        builder.HasOne(d => d.Usuario).WithMany(p => p.HistorialTraslados)
            .HasForeignKey(d => d.UsuarioId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_traslado_usuario");
    }
}
