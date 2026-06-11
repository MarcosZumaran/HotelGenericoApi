using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.HasKey(e => e.IdPago);

        builder.ToTable("pago");

        builder.Property(e => e.IdPago).HasColumnName("id_pago");
        builder.Property(e => e.IdEstancia).HasColumnName("id_estancia");
        builder.Property(e => e.Monto)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("monto");
        builder.Property(e => e.MetodoPago)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("metodo_pago");
        builder.Property(e => e.FechaPago)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("fecha_pago");

        builder.HasOne(d => d.IdEstanciaNavigation)
            .WithMany(p => p.Pagos)
            .HasForeignKey(d => d.IdEstancia)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_pago_estancia");
    }
}
