using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class CategoriaProductoConfiguration : IEntityTypeConfiguration<CategoriaProducto>
{
    public void Configure(EntityTypeBuilder<CategoriaProducto> builder)
    {
        builder.HasKey(e => e.IdCategoria);

        builder.ToTable("categoria_producto");

        builder.Property(e => e.IdCategoria).HasColumnName("id_categoria");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(100)
            .HasColumnName("descripcion");
        builder.Property(e => e.MostrarEnVentas)
            .HasDefaultValue(true, "DF_categoria_producto_mostrar")
            .HasColumnName("mostrar_en_ventas");
        builder.Property(e => e.Nombre)
            .HasMaxLength(50)
            .HasColumnName("nombre");
    }
}
