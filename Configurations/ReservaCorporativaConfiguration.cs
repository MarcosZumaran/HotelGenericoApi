using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class ReservaCorporativaConfiguration : IEntityTypeConfiguration<ReservaCorporativa>
{
    public void Configure(EntityTypeBuilder<ReservaCorporativa> builder)
    {
        builder.HasKey(e => e.IdReservaCorporativa);

        builder.ToTable("reserva_corporativa");

        builder.Property(e => e.IdReservaCorporativa).HasColumnName("id_reserva_corporativa");
        builder.Property(e => e.Estado)
            .HasMaxLength(20)
            .HasDefaultValue("Pendiente", "DF_reserva_corporativa_estado")
            .HasColumnName("estado");
        builder.Property(e => e.FechaFin).HasColumnName("fecha_fin");
        builder.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
        builder.Property(e => e.FechaRegistro)
            .HasDefaultValueSql("(sysdatetime())", "DF_reserva_corporativa_fecha")
            .HasColumnName("fecha_registro");
        builder.Property(e => e.IdClienteEmpresa).HasColumnName("id_cliente_empresa");
        builder.Property(e => e.NumeroHabitaciones).HasColumnName("numero_habitaciones");
        builder.Property(e => e.Observaciones)
            .HasMaxLength(300)
            .HasColumnName("observaciones");

        builder.HasOne(d => d.IdClienteEmpresaNavigation).WithMany(p => p.ReservaCorporativas)
            .HasForeignKey(d => d.IdClienteEmpresa)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_reserva_corporativa_cliente");
    }
}
