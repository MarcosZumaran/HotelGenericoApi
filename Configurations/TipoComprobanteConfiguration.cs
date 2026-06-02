using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class TipoComprobanteConfiguration : IEntityTypeConfiguration<TipoComprobante>
{
    public void Configure(EntityTypeBuilder<TipoComprobante> builder)
    {
        builder.HasKey(e => e.Codigo);

        builder.ToTable("tipo_comprobante");

        builder.Property(e => e.Codigo)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("codigo");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(60)
            .HasColumnName("descripcion");
    }
}
