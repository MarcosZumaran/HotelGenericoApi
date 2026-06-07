-- ============================================================
-- Migration 001: Agregar columna metodo_pago a tabla estancia
-- ============================================================
-- Fecha: 2026-06-07
-- Motivo: RF03 - Registrar método de pago en el check-in
--         Los endpoints GET /Habitacion/estado-actual,
--         GET /Estancia, GET /ReservaCorporativa fallan con 500
--         porque EF Core espera la columna metodo_pago.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.estancia') AND name = 'metodo_pago')
BEGIN
    ALTER TABLE dbo.estancia
    ADD metodo_pago CHAR(3) NULL
        CONSTRAINT FK_estancia_metodo_pago FOREIGN KEY (metodo_pago)
        REFERENCES dbo.metodo_pago(codigo);

    PRINT 'OK: Columna metodo_pago agregada a dbo.estancia.';
END
ELSE
BEGIN
    PRINT 'OK: La columna metodo_pago ya existe en dbo.estancia.';
END
GO
