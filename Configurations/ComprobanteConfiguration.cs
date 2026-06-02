using HotelGenericoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelGenericoApi.Configurations;

public class ComprobanteConfiguration : IEntityTypeConfiguration<Comprobante>
{
    public void Configure(EntityTypeBuilder<Comprobante> builder)
    {
        builder.HasKey(e => e.IdComprobante);

        builder.ToTable("comprobante");

        builder.HasIndex(e => new { e.ClienteDocumentoTipo, e.ClienteDocumentoNum }, "IX_comprobante_cliente");

        builder.HasIndex(e => e.FechaEmision, "IX_comprobante_fecha_emision");

        builder.HasIndex(e => new { e.Serie, e.Correlativo }, "UQ_comprobante_serie_correlativo").IsUnique();

        builder.Property(e => e.IdComprobante).HasColumnName("id_comprobante");
        builder.Property(e => e.CdrZip).HasColumnName("cdr_zip");
        builder.Property(e => e.ClienteDocumentoNum)
            .HasMaxLength(20)
            .HasColumnName("cliente_documento_num");
        builder.Property(e => e.ClienteDocumentoTipo)
            .HasMaxLength(1)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("cliente_documento_tipo");
        builder.Property(e => e.ClienteNombre)
            .HasMaxLength(200)
            .HasColumnName("cliente_nombre");
        builder.Property(e => e.Correlativo).HasColumnName("correlativo");
        builder.Property(e => e.FechaEmision)
            .HasDefaultValueSql("(sysdatetime())", "DF_comprobante_fecha")
            .HasColumnName("fecha_emision");
        builder.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");
        builder.Property(e => e.HashXml)
            .HasMaxLength(64)
            .HasColumnName("hash_xml");
        builder.Property(e => e.IdEstadoSunat)
            .HasDefaultValue(1, "DF_comprobante_estado")
            .HasColumnName("id_estado_sunat");
        builder.Property(e => e.IdEstancia).HasColumnName("id_estancia");
        builder.Property(e => e.IdVenta).HasColumnName("id_venta");
        builder.Property(e => e.IgvMonto)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("igv_monto");
        builder.Property(e => e.IntentosEnvio).HasColumnName("intentos_envio");
        builder.Property(e => e.MetodoPago)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("metodo_pago");
        builder.Property(e => e.MontoTotal)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("monto_total");
        builder.Property(e => e.Serie)
            .HasMaxLength(4)
            .HasColumnName("serie");
        builder.Property(e => e.TipoComprobante)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("tipo_comprobante");
        builder.Property(e => e.XmlFirmado).HasColumnName("xml_firmado");

        builder.HasOne(d => d.ClienteDocumentoTipoNavigation).WithMany(p => p.Comprobantes)
            .HasForeignKey(d => d.ClienteDocumentoTipo)
            .HasConstraintName("FK_comprobante_cliente_tipo");

        builder.HasOne(d => d.IdEstadoSunatNavigation).WithMany(p => p.Comprobantes)
            .HasForeignKey(d => d.IdEstadoSunat)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_comprobante_estado_sunat");

        builder.HasOne(d => d.IdEstanciaNavigation).WithMany(p => p.Comprobantes)
            .HasForeignKey(d => d.IdEstancia)
            .HasConstraintName("FK_comprobante_estancia");

        builder.HasOne(d => d.IdVentaNavigation).WithMany(p => p.Comprobantes)
            .HasForeignKey(d => d.IdVenta)
            .HasConstraintName("FK_comprobante_venta");

        builder.HasOne(d => d.MetodoPagoNavigation).WithMany(p => p.Comprobantes)
            .HasForeignKey(d => d.MetodoPago)
            .HasConstraintName("FK_comprobante_metodo_pago");

        builder.HasOne(d => d.TipoComprobanteNavigation).WithMany(p => p.Comprobantes)
            .HasForeignKey(d => d.TipoComprobante)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_comprobante_tipo");
    }
}
