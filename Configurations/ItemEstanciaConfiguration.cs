using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class ItemEstanciaConfiguration : IEntityTypeConfiguration<ItemEstancia>
{
    public void Configure(EntityTypeBuilder<ItemEstancia> builder)
    {
        builder.HasKey(e => e.IdItem);

        builder.ToTable("item_estancia");

        builder.HasIndex(e => e.IdEstancia, "IX_item_estancia_estancia");

        builder.Property(e => e.IdItem).HasColumnName("id_item");
        builder.Property(e => e.Cantidad).HasColumnName("cantidad");
        builder.Property(e => e.FechaRegistro)
            .HasDefaultValueSql("(sysdatetime())", "DF_item_estancia_fecha")
            .HasColumnName("fecha_registro");
        builder.Property(e => e.IdEstancia).HasColumnName("id_estancia");
        builder.Property(e => e.IdProducto).HasColumnName("id_producto");
        builder.Property(e => e.PrecioUnitario)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("precio_unitario");
        builder.Property(e => e.Subtotal)
            .HasComputedColumnSql("([cantidad]*[precio_unitario])", true)
            .HasColumnType("decimal(21, 2)")
            .HasColumnName("subtotal");

        builder.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.ItemsEstancia)
            .HasForeignKey(d => d.IdEstancia)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_item_estancia_estancia");

        builder.HasOne(d => d.IdProductoNavigation).WithMany(p => p.ItemsEstancia)
            .HasForeignKey(d => d.IdProducto)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_item_estancia_producto");
    }
}
