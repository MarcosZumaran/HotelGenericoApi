using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class EstadoSunatConfiguration : IEntityTypeConfiguration<EstadoSunat>
{
    public void Configure(EntityTypeBuilder<EstadoSunat> builder)
    {
        builder.HasKey(e => e.Codigo);

        builder.ToTable("estado_sunat");

        builder.Property(e => e.Codigo)
            .ValueGeneratedNever()
            .HasColumnName("codigo");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(60)
            .HasColumnName("descripcion");
        builder.Property(e => e.DescripcionLarga)
            .HasMaxLength(200)
            .HasColumnName("descripcion_larga");
    }
}
