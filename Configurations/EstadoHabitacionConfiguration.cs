using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class EstadoHabitacionConfiguration : IEntityTypeConfiguration<EstadoHabitacion>
{
    public void Configure(EntityTypeBuilder<EstadoHabitacion> builder)
    {
        builder.HasKey(e => e.IdEstado);

        builder.ToTable("estado_habitacion");

        builder.Property(e => e.IdEstado).HasColumnName("id_estado");
        builder.Property(e => e.ColorUi)
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasColumnName("color_ui");
        builder.Property(e => e.Descripcion)
            .HasMaxLength(100)
            .HasColumnName("descripcion");
        builder.Property(e => e.EsEstadoFinal).HasColumnName("es_estado_final");
        builder.Property(e => e.Nombre)
            .HasMaxLength(30)
            .HasColumnName("nombre");
        builder.Property(e => e.PermiteCheckin).HasColumnName("permite_checkin");
        builder.Property(e => e.PermiteCheckout).HasColumnName("permite_checkout");
    }
}
