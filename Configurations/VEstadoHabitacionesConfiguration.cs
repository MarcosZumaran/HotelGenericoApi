using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class VEstadoHabitacionsConfiguration : IEntityTypeConfiguration<VEstadoHabitacion>
{
    public void Configure(EntityTypeBuilder<VEstadoHabitacion> builder)
    {
        builder
            .HasNoKey()
            .ToView("v_estado_habitaciones");

        builder.Property(e => e.Estado)
            .HasMaxLength(30)
            .HasColumnName("estado");
        builder.Property(e => e.FechaUltimoCambio).HasColumnName("fecha_ultimo_cambio");
        builder.Property(e => e.NumeroHabitacion)
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("numero_habitacion");
        builder.Property(e => e.PrecioNoche)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("precio_noche");
        builder.Property(e => e.TipoHabitacion)
            .HasMaxLength(50)
            .HasColumnName("tipo_habitacion");
    }
}
