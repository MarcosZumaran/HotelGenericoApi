using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class EstanciaConfiguration : IEntityTypeConfiguration<Estancia>
{
    public void Configure(EntityTypeBuilder<Estancia> builder)
    {
        builder.HasKey(e => e.IdEstancia);

        builder.ToTable("estancia");

        builder.HasIndex(e => e.IdReservaCorporativa, "IX_estancia_id_reserva_corporativa").HasFilter("([id_reserva_corporativa] IS NOT NULL)");

        builder.Property(e => e.IdEstancia).HasColumnName("id_estancia");
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(sysdatetime())", "DF_estancia_fecha")
            .HasColumnName("created_at");
        builder.Property(e => e.EstaFuera).HasColumnName("esta_fuera");
        builder.Property(e => e.FechaCheckin).HasColumnName("fecha_checkin");
        builder.Property(e => e.FechaCheckoutPrevista).HasColumnName("fecha_checkout_prevista");
        builder.Property(e => e.FechaCheckoutReal).HasColumnName("fecha_checkout_real");
        builder.Property(e => e.HoraRegresoTemporal).HasColumnName("hora_regreso_temporal");
        builder.Property(e => e.HoraSalidaTemporal).HasColumnName("hora_salida_temporal");
        builder.Property(e => e.IdClienteTitular).HasColumnName("id_cliente_titular");
        builder.Property(e => e.IdEstadoEstancia)
            .HasDefaultValue(1, "DF_estancia_estado")
            .HasColumnName("id_estado_estancia");
        builder.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
        builder.Property(e => e.IdReserva).HasColumnName("id_reserva");
        builder.Property(e => e.IdReservaCorporativa).HasColumnName("id_reserva_corporativa");
        builder.Property(e => e.LlavesDejadas).HasColumnName("llaves_dejadas");
        builder.Property(e => e.MontoTotal)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("monto_total");

        builder.HasOne(d => d.IdClienteTitularNavigation).WithMany(p => p.Estancias)
            .HasForeignKey(d => d.IdClienteTitular)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_estancia_cliente");

        builder.HasOne(d => d.IdEstadoEstanciaNavigation).WithMany(p => p.Estancias)
            .HasForeignKey(d => d.IdEstadoEstancia)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_estancia_estado");

        builder.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Estancias)
            .HasForeignKey(d => d.IdHabitacion)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_estancia_habitacion");

        builder.HasOne(d => d.IdReservaNavigation).WithMany(p => p.Estancias)
            .HasForeignKey(d => d.IdReserva)
            .HasConstraintName("FK_estancia_reserva");

        builder.HasOne(d => d.IdReservaCorporativaNavigation).WithMany(p => p.Estancias)
            .HasForeignKey(d => d.IdReservaCorporativa)
            .HasConstraintName("FK_estancia_reserva_corporativa");
    }
}
