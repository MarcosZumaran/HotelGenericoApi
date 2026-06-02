using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class TipoDocumentoConfiguration : IEntityTypeConfiguration<TipoDocumento>
{
    public void Configure(EntityTypeBuilder<TipoDocumento> builder)
    {
        builder.HasKey(e => e.Codigo);

        builder.ToTable("tipo_documento");

        builder.Property(e => e.Codigo)
            .HasMaxLength(1)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("codigo");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(60)
            .HasColumnName("descripcion");
    }
}
