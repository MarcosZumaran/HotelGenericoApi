using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class CierreCajaEnvioConfiguration : IEntityTypeConfiguration<CierreCajaEnvio>
{
    public void Configure(EntityTypeBuilder<CierreCajaEnvio> builder)
    {
        builder.HasKey(e => e.Fecha);

        builder.ToTable("cierre_caja_envio");

        builder.Property(e => e.Fecha).HasColumnName("fecha");
        builder.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");
        builder.Property(e => e.HashXml)
            .HasMaxLength(64)
            .HasColumnName("hash_xml");
        builder.Property(e => e.IdEstadoSunat)
            .HasDefaultValue(1, "DF_cierre_caja_envio_estado")
            .HasColumnName("id_estado_sunat");
        builder.Property(e => e.IntentosEnvio).HasColumnName("intentos_envio");

        builder.HasOne(d => d.IdEstadoSunatNavigation).WithMany(p => p.CierreCajaEnvios)
            .HasForeignKey(d => d.IdEstadoSunat)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_cierre_estado_sunat");
    }
}
