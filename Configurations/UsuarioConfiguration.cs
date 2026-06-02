using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(e => e.IdUsuario);

        builder.ToTable("usuario");

        builder.HasIndex(e => e.Username, "UQ_usuario_username").IsUnique();

        builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
        builder.Property(e => e.DebeCambiarPassword)
            .HasDefaultValue(true, "DF_usuario_cambio")
            .HasColumnName("debe_cambiar_password");
        builder.Property(e => e.EstaActivo)
            .HasDefaultValue(true, "DF_usuario_activo")
            .HasColumnName("esta_activo");
        builder.Property(e => e.FechaCreacion)
            .HasDefaultValueSql("(sysdatetime())", "DF_usuario_fecha")
            .HasColumnName("fecha_creacion");
        builder.Property(e => e.IdRol).HasColumnName("id_rol");
        builder.Property(e => e.PasswordHash)
            .HasMaxLength(255)
            .HasColumnName("password_hash");
        builder.Property(e => e.Username)
            .HasMaxLength(50)
            .HasColumnName("username");

        builder.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
            .HasForeignKey(d => d.IdRol)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_usuario_rol");
    }
}
