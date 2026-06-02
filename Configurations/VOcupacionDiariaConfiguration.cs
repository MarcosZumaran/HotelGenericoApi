using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class VOcupacionDiariaConfiguration : IEntityTypeConfiguration<VOcupacionDiaria>
{
    public void Configure(EntityTypeBuilder<VOcupacionDiaria> builder)
    {
        builder
            .HasNoKey()
            .ToView("v_ocupacion_diaria");

        builder.Property(e => e.Fecha).HasColumnName("fecha");
        builder.Property(e => e.Ocupadas).HasColumnName("ocupadas");
        builder.Property(e => e.PorcentajeOcupacion)
            .HasColumnType("decimal(5, 2)")
            .HasColumnName("porcentaje_ocupacion");
        builder.Property(e => e.Total).HasColumnName("total");
    }
}
