using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class RolUsuarioConfiguration : IEntityTypeConfiguration<RolUsuario>
{
    public void Configure(EntityTypeBuilder<RolUsuario> builder)
    {
        builder.HasKey(e => e.IdRol);

        builder.ToTable("rol_usuario");

        builder.Property(e => e.IdRol).HasColumnName("id_rol");
        builder.Property(e => e.Nombre)
            .HasMaxLength(30)
            .HasColumnName("nombre");
    }
}
