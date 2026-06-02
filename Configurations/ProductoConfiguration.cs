using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.HasKey(e => e.IdProducto);

        builder.ToTable("producto");

        builder.HasIndex(e => e.CodigoSunat, "IX_producto_codigo_sunat");

        builder.Property(e => e.IdProducto).HasColumnName("id_producto");
        builder.Property(e => e.CodigoSunat)
            .HasMaxLength(20)
            .HasColumnName("codigo_sunat");
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(sysdatetime())", "DF_producto_fecha")
            .HasColumnName("created_at");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(200)
            .HasColumnName("descripcion");
        builder.Property(e => e.EsAmenidad).HasColumnName("es_amenidad");
        builder.Property(e => e.EsVendibleEnTienda)
            .HasDefaultValue(true, "DF_producto_vendible")
            .HasColumnName("es_vendible_en_tienda");
        builder.Property(e => e.IdAfectacionIgv)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength()
            .HasDefaultValue("10", "DF_producto_afectacion")
            .HasColumnName("id_afectacion_igv");
        builder.Property(e => e.IdCategoria).HasColumnName("id_categoria");
        builder.Property(e => e.ImagenUrl)
            .HasMaxLength(255)
            .HasColumnName("imagen_url");
        builder.Property(e => e.Nombre)
            .HasMaxLength(100)
            .HasColumnName("nombre");
        builder.Property(e => e.PrecioUnitario)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("precio_unitario");
        builder.Property(e => e.Stock).HasColumnName("stock");
        builder.Property(e => e.StockMinimo)
            .HasDefaultValue(5, "DF_producto_stock_min")
            .HasColumnName("stock_minimo");
        builder.Property(e => e.StockPorHabitacion).HasColumnName("stock_por_habitacion");
        builder.Property(e => e.UnidadMedida)
            .HasMaxLength(3)
            .HasDefaultValue("NIU", "DF_producto_unidad")
            .HasColumnName("unidad_medida");

        builder.HasOne(d => d.IdAfectacionIgvNavigation).WithMany(p => p.Productos)
            .HasForeignKey(d => d.IdAfectacionIgv)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_producto_afectacion");

        builder.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
            .HasForeignKey(d => d.IdCategoria)
            .HasConstraintName("FK_producto_categoria");
    }
}
