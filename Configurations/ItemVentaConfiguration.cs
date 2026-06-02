using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class ItemVentaConfiguration : IEntityTypeConfiguration<ItemVentum>
{
    public void Configure(EntityTypeBuilder<ItemVentum> builder)
    {
        builder.HasKey(e => e.IdItem);

        builder.ToTable("item_venta");

        builder.HasIndex(e => e.IdVenta, "IX_item_venta_venta");

        builder.Property(e => e.IdItem).HasColumnName("id_item");
        builder.Property(e => e.Cantidad).HasColumnName("cantidad");
        builder.Property(e => e.IdProducto).HasColumnName("id_producto");
        builder.Property(e => e.IdVenta).HasColumnName("id_venta");
        builder.Property(e => e.PrecioUnitario)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("precio_unitario");
        builder.Property(e => e.Subtotal)
            .HasComputedColumnSql("([cantidad]*[precio_unitario])", true)
            .HasColumnType("decimal(21, 2)")
            .HasColumnName("subtotal");

        builder.HasOne(d => d.IdProductoNavigation).WithMany(p => p.ItemVenta)
            .HasForeignKey(d => d.IdProducto)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_item_venta_producto");

        builder.HasOne(d => d.IdVentaNavigation).WithMany(p => p.ItemVenta)
            .HasForeignKey(d => d.IdVenta)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_item_venta_venta");
    }
}
