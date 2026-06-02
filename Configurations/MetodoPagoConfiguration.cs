using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class MetodoPagoConfiguration : IEntityTypeConfiguration<MetodoPago>
{
    public void Configure(EntityTypeBuilder<MetodoPago> builder)
    {
        builder.HasKey(e => e.Codigo);

        builder.ToTable("metodo_pago");

        builder.Property(e => e.Codigo)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("codigo");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(60)
            .HasColumnName("descripcion");
    }
}
