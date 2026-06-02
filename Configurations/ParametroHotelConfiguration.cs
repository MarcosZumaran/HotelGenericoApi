using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class ParametroHotelConfiguration : IEntityTypeConfiguration<ParametroHotel>
{
    public void Configure(EntityTypeBuilder<ParametroHotel> builder)
    {
        builder.HasKey(e => e.IdParametro);

        builder.ToTable("parametro_hotel");

        builder.HasIndex(e => e.Clave, "UQ_parametro_hotel_clave").IsUnique();

        builder.Property(e => e.IdParametro).HasColumnName("id_parametro");
        builder.Property(e => e.Clave)
            .HasMaxLength(100)
            .HasColumnName("clave");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(200)
            .HasColumnName("descripcion");
        builder.Property(e => e.FechaActualizacion)
            .HasDefaultValueSql("(sysdatetime())", "DF_parametro_hotel_fecha")
            .HasColumnName("fecha_actualizacion");
        builder.Property(e => e.Valor)
            .HasMaxLength(500)
            .HasColumnName("valor");
    }
}
