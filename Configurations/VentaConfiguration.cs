using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Ventum>
{
    public void Configure(EntityTypeBuilder<Ventum> builder)
    {
        builder.HasKey(e => e.IdVenta);

        builder.ToTable("venta");

        builder.Property(e => e.IdVenta).HasColumnName("id_venta");
        builder.Property(e => e.FechaVenta)
            .HasDefaultValueSql("(sysdatetime())", "DF_venta_fecha")
            .HasColumnName("fecha_venta");
        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
        builder.Property(e => e.MetodoPago)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("metodo_pago");
        builder.Property(e => e.Total)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("total");

        builder.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
            .HasForeignKey(d => d.IdCliente)
            .HasConstraintName("FK_venta_cliente");

        builder.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
            .HasForeignKey(d => d.IdUsuario)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_venta_usuario");

        builder.HasOne(d => d.MetodoPagoNavigation).WithMany(p => p.Venta)
            .HasForeignKey(d => d.MetodoPago)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_venta_metodo_pago");
    }
}
