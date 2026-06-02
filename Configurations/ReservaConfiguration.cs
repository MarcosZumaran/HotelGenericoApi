using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.HasKey(e => e.IdReserva);

        builder.ToTable("reserva");

        builder.HasIndex(e => e.IdReservaCorporativa, "IX_reserva_id_reserva_corporativa");

        builder.Property(e => e.IdReserva).HasColumnName("id_reserva");
        builder.Property(e => e.EsNoShow).HasColumnName("es_no_show");
        builder.Property(e => e.FechaEntradaPrevista).HasColumnName("fecha_entrada_prevista");
        builder.Property(e => e.FechaRegistro)
            .HasDefaultValueSql("(sysdatetime())", "DF_reserva_fecha")
            .HasColumnName("fecha_registro");
        builder.Property(e => e.FechaSalidaPrevista).HasColumnName("fecha_salida_prevista");
        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.IdEstadoReserva)
            .HasDefaultValue(1, "DF_reserva_estado")
            .HasColumnName("id_estado_reserva");
        builder.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
        builder.Property(e => e.IdReservaCorporativa).HasColumnName("id_reserva_corporativa");
        builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
        builder.Property(e => e.MontoTotal)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("monto_total");
        builder.Property(e => e.Observaciones)
            .HasMaxLength(300)
            .HasColumnName("observaciones");

        builder.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Reservas)
            .HasForeignKey(d => d.IdCliente)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_reserva_cliente");

        builder.HasOne(d => d.IdEstadoReservaNavigation).WithMany(p => p.Reservas)
            .HasForeignKey(d => d.IdEstadoReserva)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_reserva_estado");

        builder.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Reservas)
            .HasForeignKey(d => d.IdHabitacion)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_reserva_habitacion");

        builder.HasOne(d => d.IdReservaCorporativaNavigation).WithMany(p => p.Reservas)
            .HasForeignKey(d => d.IdReservaCorporativa)
            .HasConstraintName("FK_reserva_corporativa");

        builder.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reservas)
            .HasForeignKey(d => d.IdUsuario)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_reserva_usuario");
    }
}
