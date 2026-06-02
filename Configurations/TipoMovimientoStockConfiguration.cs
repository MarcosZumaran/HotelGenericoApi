using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class TipoMovimientoStockConfiguration : IEntityTypeConfiguration<TipoMovimientoStock>
{
    public void Configure(EntityTypeBuilder<TipoMovimientoStock> builder)
    {
        builder.HasKey(e => e.Codigo);

        builder.ToTable("tipo_movimiento_stock");

        builder.Property(e => e.Codigo)
            .HasMaxLength(20)
            .HasColumnName("codigo");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(100)
            .HasColumnName("descripcion");
    }
}
