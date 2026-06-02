using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.HasKey(e => e.IdLoginAttempt);

        builder.ToTable("login_attempt");

        builder.HasIndex(e => new { e.IpAddress, e.AttemptedAt }, "IX_login_attempt_ip_fecha");

        builder.HasIndex(e => new { e.Username, e.AttemptedAt }, "IX_login_attempt_username_at");

        builder.Property(e => e.IdLoginAttempt).HasColumnName("id_login_attempt");
        builder.Property(e => e.AttemptedAt)
            .HasDefaultValueSql("(sysdatetime())", "DF_login_attempt_fecha")
            .HasColumnName("attempted_at");
        builder.Property(e => e.IpAddress)
            .HasMaxLength(50)
            .HasColumnName("ip_address");
        builder.Property(e => e.Succeeded).HasColumnName("succeeded");
        builder.Property(e => e.UserAgent)
            .HasMaxLength(500)
            .HasColumnName("user_agent");
        builder.Property(e => e.Username)
            .HasMaxLength(100)
            .HasColumnName("username");
    }
}
