using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.HasKey(e => e.IdCliente);

        builder.ToTable("cliente");

        builder.HasIndex(e => e.CodigoInterno, "UQ_cliente_codigo").IsUnique();

        builder.HasIndex(e => new { e.TipoDocumento, e.Documento }, "UX_cliente_documento")
            .IsUnique()
            .HasFilter("([tipo_documento] IS NOT NULL AND [documento] IS NOT NULL)");

        builder.Property(e => e.IdCliente).HasColumnName("id_cliente");
        builder.Property(e => e.Alias)
            .HasMaxLength(120)
            .HasColumnName("alias");
        builder.Property(e => e.Apellidos)
            .HasMaxLength(100)
            .HasColumnName("apellidos");
        builder.Property(e => e.CodigoInterno)
            .HasMaxLength(40)
            .HasDefaultValueSql("(concat(N'CLI-',replace(CONVERT([varchar](36),newid()),'-','')))", "DF_cliente_codigo")
            .HasColumnName("codigo_interno");
        builder.Property(e => e.Direccion)
            .HasMaxLength(200)
            .HasColumnName("direccion");
        builder.Property(e => e.Documento)
            .HasMaxLength(20)
            .HasColumnName("documento");
        builder.Property(e => e.Email)
            .HasMaxLength(100)
            .HasColumnName("email");
        builder.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
        builder.Property(e => e.FechaRegistro)
            .HasDefaultValueSql("(sysdatetime())", "DF_cliente_fecha")
            .HasColumnName("fecha_registro");
        builder.Property(e => e.FechaVerificacionReniec).HasColumnName("fecha_verificacion_reniec");
        builder.Property(e => e.Nacionalidad)
            .HasMaxLength(50)
            .HasDefaultValue("PERUANA", "DF_cliente_nacionalidad")
            .HasColumnName("nacionalidad");
        builder.Property(e => e.Nombres)
            .HasMaxLength(100)
            .HasColumnName("nombres");
        builder.Property(e => e.Telefono)
            .HasMaxLength(15)
            .HasColumnName("telefono");
        builder.Property(e => e.TipoDocumento)
            .HasMaxLength(1)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("tipo_documento");

        builder.HasOne(d => d.TipoDocumentoNavigation).WithMany(p => p.Clientes)
            .HasForeignKey(d => d.TipoDocumento)
            .HasConstraintName("FK_cliente_tipo_documento");
    }
}
