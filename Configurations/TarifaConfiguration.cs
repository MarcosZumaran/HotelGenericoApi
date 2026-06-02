using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class TarifaConfiguration : IEntityTypeConfiguration<Tarifa>
{
    public void Configure(EntityTypeBuilder<Tarifa> builder)
    {
        builder.HasKey(e => e.IdTarifa);

        builder.ToTable("tarifa");

        builder.Property(e => e.IdTarifa).HasColumnName("id_tarifa");
        builder.Property(e => e.FechaFin).HasColumnName("fecha_fin");
        builder.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
        builder.Property(e => e.IdTemporada).HasColumnName("id_temporada");
        builder.Property(e => e.IdTipoHabitacion).HasColumnName("id_tipo_habitacion");
        builder.Property(e => e.Precio)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("precio");

        builder.HasOne(d => d.IdTemporadaNavigation).WithMany(p => p.Tarifas)
            .HasForeignKey(d => d.IdTemporada)
            .HasConstraintName("FK_tarifa_temporada");

        builder.HasOne(d => d.IdTipoHabitacionNavigation).WithMany(p => p.Tarifas)
            .HasForeignKey(d => d.IdTipoHabitacion)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_tarifa_tipo_habitacion");
    }
}
