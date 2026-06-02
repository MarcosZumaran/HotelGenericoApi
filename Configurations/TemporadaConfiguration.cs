using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class TemporadaConfiguration : IEntityTypeConfiguration<Temporadum>
{
    public void Configure(EntityTypeBuilder<Temporadum> builder)
    {
        builder.HasKey(e => e.IdTemporada);

        builder.ToTable("temporada");

        builder.Property(e => e.IdTemporada).HasColumnName("id_temporada");
        builder.Property(e => e.FechaFin).HasColumnName("fecha_fin");
        builder.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
        builder.Property(e => e.Multiplicador)
            .HasDefaultValue(1.00m, "DF_temporada_mult")
            .HasColumnType("decimal(5, 2)")
            .HasColumnName("multiplicador");
        builder.Property(e => e.Nombre)
            .HasMaxLength(50)
            .HasColumnName("nombre");
    }
}
