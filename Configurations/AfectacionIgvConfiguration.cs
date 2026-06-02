using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class AfectacionIgvConfiguration : IEntityTypeConfiguration<AfectacionIgv>
{
    public void Configure(EntityTypeBuilder<AfectacionIgv> builder)
    {
        builder.HasKey(e => e.Codigo);

        builder.ToTable("afectacion_igv");

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
