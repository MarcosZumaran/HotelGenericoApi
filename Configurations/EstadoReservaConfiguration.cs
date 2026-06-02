using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class EstadoReservaConfiguration : IEntityTypeConfiguration<EstadoReserva>
{
    public void Configure(EntityTypeBuilder<EstadoReserva> builder)
    {
        builder.HasKey(e => e.IdEstadoReserva);

        builder.ToTable("estado_reserva");

        builder.HasIndex(e => e.Codigo, "UQ_estado_reserva_codigo").IsUnique();

        builder.Property(e => e.IdEstadoReserva).HasColumnName("id_estado_reserva");
        builder.Property(e => e.Codigo)
            .HasMaxLength(20)
            .HasColumnName("codigo");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(100)
            .HasColumnName("descripcion");
        builder.Property(e => e.EsFinal).HasColumnName("es_final");
    }
}
