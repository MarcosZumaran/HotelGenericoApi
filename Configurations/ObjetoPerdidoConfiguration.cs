using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class ObjetoPerdidoConfiguration : IEntityTypeConfiguration<ObjetoPerdido>
{
    public void Configure(EntityTypeBuilder<ObjetoPerdido> builder)
    {
        builder.HasKey(e => e.IdObjeto);

        builder.ToTable("objeto_perdido");

        builder.HasIndex(e => e.Estado, "IX_objeto_estado");

        builder.HasIndex(e => e.FechaHallazgo, "IX_objeto_fecha").IsDescending();

        builder.Property(e => e.IdObjeto).HasColumnName("id_objeto");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(200)
            .HasColumnName("descripcion");
        builder.Property(e => e.EntregadoA)
            .HasMaxLength(100)
            .HasColumnName("entregado_a");
        builder.Property(e => e.Estado)
            .HasMaxLength(20)
            .HasDefaultValue("pendiente", "DF_objeto_perdido_estado")
            .HasColumnName("estado");
        builder.Property(e => e.FechaEntregado).HasColumnName("fecha_entregado");
        builder.Property(e => e.FechaHallazgo)
            .HasDefaultValueSql("(sysdatetime())", "DF_objeto_perdido_fecha")
            .HasColumnName("fecha_hallazgo");
        builder.Property(e => e.IdEstancia).HasColumnName("id_estancia");
        builder.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
        builder.Property(e => e.ImagenUrl)
            .HasMaxLength(255)
            .HasColumnName("imagen_url");

        builder.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.ObjetoPerdidos)
            .HasForeignKey(d => d.IdEstancia)
            .HasConstraintName("FK_objeto_perdido_estancia");

        builder.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.ObjetoPerdidos)
            .HasForeignKey(d => d.IdHabitacion)
            .HasConstraintName("FK_objeto_perdido_habitacion");
    }
}
