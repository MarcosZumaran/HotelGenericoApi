using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class IncidenteConfiguration : IEntityTypeConfiguration<Incidente>
{
    public void Configure(EntityTypeBuilder<Incidente> builder)
    {
        builder.HasKey(e => e.IdIncidente);

        builder.ToTable("incidente");

        builder.HasIndex(e => e.IdEstancia, "IX_incidente_estancia");

        builder.HasIndex(e => new { e.IdHabitacion, e.FechaRegistro }, "IX_incidente_habitacion_fecha").IsDescending(false, true);

        builder.Property(e => e.IdIncidente).HasColumnName("id_incidente");
        builder.Property(e => e.CobradoAlCliente).HasColumnName("cobrado_al_cliente");
        builder.Property(e => e.CostoEstimado)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("costo_estimado");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(500)
            .HasColumnName("descripcion");
        builder.Property(e => e.FechaRegistro)
            .HasDefaultValueSql("(sysdatetime())", "DF_incidente_fecha")
            .HasColumnName("fecha_registro");
        builder.Property(e => e.IdEstancia).HasColumnName("id_estancia");
        builder.Property(e => e.IdHabitacion).HasColumnName("id_habitacion");
        builder.Property(e => e.ImagenUrl)
            .HasMaxLength(255)
            .HasColumnName("imagen_url");
        builder.Property(e => e.ReportadoPor).HasColumnName("reportado_por");
        builder.Property(e => e.Resuelto).HasColumnName("resuelto");
        builder.Property(e => e.Tipo)
            .HasMaxLength(50)
            .HasColumnName("tipo");

        builder.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.Incidentes)
            .HasForeignKey(d => d.IdEstancia)
            .HasConstraintName("FK_incidente_estancia");

        builder.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Incidentes)
            .HasForeignKey(d => d.IdHabitacion)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_incidente_habitacion");

        builder.HasOne(d => d.ReportadoPorNavigation).WithMany(p => p.Incidentes)
            .HasForeignKey(d => d.ReportadoPor)
            .HasConstraintName("FK_incidente_usuario");
    }
}
