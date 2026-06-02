using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class VCierreCajaDiarioConfiguration : IEntityTypeConfiguration<VCierreCajaDiario>
{
    public void Configure(EntityTypeBuilder<VCierreCajaDiario> builder)
    {
        builder
            .HasNoKey()
            .ToView("v_cierre_caja_diario");

        builder.Property(e => e.Concepto)
            .HasMaxLength(9)
            .HasColumnName("concepto");
        builder.Property(e => e.Fecha).HasColumnName("fecha");
        builder.Property(e => e.Ingresos)
            .HasColumnType("decimal(38, 2)")
            .HasColumnName("ingresos");
        builder.Property(e => e.MetodoPago)
            .HasMaxLength(60)
            .HasColumnName("metodo_pago");
    }
}
