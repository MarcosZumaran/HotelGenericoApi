using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class ConfiguracionConfiguration : IEntityTypeConfiguration<Configuracion>
{
    public void Configure(EntityTypeBuilder<Configuracion> builder)
    {
        builder.HasKey(e => e.IdConfiguracion);

        builder.ToTable("configuracion");

        builder.Property(e => e.IdConfiguracion)
            .HasDefaultValue(1, "DF_configuracion_id")
            .HasColumnName("id_configuracion");
        builder.Property(e => e.Direccion)
            .HasMaxLength(200)
            .HasColumnName("direccion");
        builder.Property(e => e.FechaActualizacion)
            .HasDefaultValueSql("(sysdatetime())", "DF_configuracion_fecha")
            .HasColumnName("fecha_actualizacion");
        builder.Property(e => e.Nombre)
            .HasMaxLength(100)
            .HasColumnName("nombre");
        builder.Property(e => e.Ruc)
            .HasMaxLength(11)
            .HasColumnName("ruc");
        builder.Property(e => e.TasaIgvHotel)
            .HasDefaultValue(18.00m, "DF_configuracion_igv_hotel")
            .HasColumnType("decimal(5, 2)")
            .HasColumnName("tasa_igv_hotel");
        builder.Property(e => e.TasaIgvProductos)
            .HasDefaultValue(18.00m, "DF_configuracion_igv_productos")
            .HasColumnType("decimal(5, 2)")
            .HasColumnName("tasa_igv_productos");
        builder.Property(e => e.Telefono)
            .HasMaxLength(20)
            .HasColumnName("telefono");
    }
}
