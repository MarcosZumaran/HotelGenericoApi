using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(e => e.IdAudit);

        builder.ToTable("audit_log");

        builder.Property(e => e.IdAudit).HasColumnName("id_audit");
        builder.Property(e => e.Accion)
            .HasMaxLength(20)
            .HasColumnName("accion");
        builder.Property(e => e.DatosAnteriores).HasColumnName("datos_anteriores");
        builder.Property(e => e.DatosNuevos).HasColumnName("datos_nuevos");
        builder.Property(e => e.Fecha)
            .HasDefaultValueSql("(sysdatetime())", "DF_audit_log_fecha")
            .HasColumnName("fecha");
        builder.Property(e => e.IdRegistro)
            .HasMaxLength(100)
            .HasColumnName("id_registro");
        builder.Property(e => e.IpAddress)
            .HasMaxLength(50)
            .HasColumnName("ip_address");
        builder.Property(e => e.Modulo)
            .HasMaxLength(50)
            .HasColumnName("modulo");
        builder.Property(e => e.Tabla)
            .HasMaxLength(128)
            .HasColumnName("tabla");
        builder.Property(e => e.Usuario)
            .HasMaxLength(100)
            .HasColumnName("usuario");
    }
}
