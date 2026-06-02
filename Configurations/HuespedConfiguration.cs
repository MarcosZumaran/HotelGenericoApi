using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class HuespedConfiguration : IEntityTypeConfiguration<Huesped>
{
    public void Configure(EntityTypeBuilder<Huesped> builder)
    {
        builder.HasKey(e => e.IdHuesped);

        builder.ToTable("huesped");

        builder.Property(e => e.IdHuesped).HasColumnName("id_huesped");
        builder.Property(e => e.EsTitular).HasColumnName("es_titular");
        builder.Property(e => e.FechaRegistro)
            .HasDefaultValueSql("(sysdatetime())", "DF_huesped_fecha")
            .HasColumnName("fecha_registro");
        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.IdEstancia).HasColumnName("id_estancia");

        builder.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Huespedes)
            .HasForeignKey(d => d.IdCliente)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_huesped_cliente");

        builder.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.Huespedes)
            .HasForeignKey(d => d.IdEstancia)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_huesped_estancia");
    }
}
