using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class TransicionEstadoConfiguration : IEntityTypeConfiguration<TransicionEstado>
{
    public void Configure(EntityTypeBuilder<TransicionEstado> builder)
    {
        builder.HasKey(e => e.IdTransicion);

        builder.ToTable("transicion_estado");

        builder.HasIndex(e => new { e.IdEstadoActual, e.IdEstadoSiguiente }, "UQ_transicion_estado").IsUnique();

        builder.Property(e => e.IdTransicion).HasColumnName("id_transicion");
        builder.Property(e => e.IdEstadoActual).HasColumnName("id_estado_actual");
        builder.Property(e => e.IdEstadoSiguiente).HasColumnName("id_estado_siguiente");

        builder.HasOne(d => d.IdEstadoActualNavigation).WithMany(p => p.TransicionEstadoIdEstadoActualNavigations)
            .HasForeignKey(d => d.IdEstadoActual)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_transicion_actual");

        builder.HasOne(d => d.IdEstadoSiguienteNavigation).WithMany(p => p.TransicionEstadoIdEstadoSiguienteNavigations)
            .HasForeignKey(d => d.IdEstadoSiguiente)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_transicion_siguiente");
    }
}
