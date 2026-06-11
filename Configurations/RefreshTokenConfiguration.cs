using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(e => e.IdRefreshToken);

        builder.ToTable("refresh_token");

        builder.HasIndex(e => e.Token, "UQ_refresh_token_token").IsUnique();

        builder.Property(e => e.IdRefreshToken).HasColumnName("id_refresh_token");
        builder.Property(e => e.IdUsuario).HasColumnName("id_usuario");
        builder.Property(e => e.Token)
            .HasMaxLength(512)
            .HasColumnName("token");
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(sysdatetime())")
            .HasColumnName("created_at");
        builder.Property(e => e.RevokedAt).HasColumnName("revoked_at");

        builder.HasOne(d => d.IdUsuarioNavigation)
            .WithMany(p => p.RefreshTokens)
            .HasForeignKey(d => d.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_refresh_token_usuario");
    }
}
