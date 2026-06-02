using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class EstadoEstanciaConfiguration : IEntityTypeConfiguration<EstadoEstancia>
{
    public void Configure(EntityTypeBuilder<EstadoEstancia> builder)
    {
        builder.HasKey(e => e.IdEstadoEstancia);

        builder.ToTable("estado_estancia");

        builder.HasIndex(e => e.Codigo, "UQ_estado_estancia_codigo").IsUnique();

        builder.Property(e => e.IdEstadoEstancia).HasColumnName("id_estado_estancia");
        builder.Property(e => e.Codigo)
            .HasMaxLength(20)
            .HasColumnName("codigo");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(100)
            .HasColumnName("descripcion");
        builder.Property(e => e.EsFinal).HasColumnName("es_final");
    }
}
