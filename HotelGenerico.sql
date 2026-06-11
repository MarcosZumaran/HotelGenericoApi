-- ================================================================
--  Nombre: HotelGenericoDB.sql
--  Motor : SQL Server 2019+
--  Descripción:
--    Script de creación completo, reorganizado e idempotente.
-- ================================================================

USE [master];
GO

IF DB_ID(N'HotelDB') IS NULL
BEGIN
    CREATE DATABASE [HotelDB];
END
GO

USE [HotelDB];
GO

/* ================================================================
   TABLAS DE CATÁLOGO Y CONFIGURACIÓN
   ================================================================ */

IF OBJECT_ID(N'dbo.configuracion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.configuracion (
        id_configuracion INT NOT NULL CONSTRAINT PK_configuracion PRIMARY KEY
            CONSTRAINT DF_configuracion_id DEFAULT (1)
            CONSTRAINT CK_configuracion_unica CHECK (id_configuracion = 1),
        nombre NVARCHAR(100) NOT NULL,
        direccion NVARCHAR(200) NULL,
        telefono NVARCHAR(20) NULL,
        ruc NVARCHAR(11) NULL,
        tasa_igv_hotel DECIMAL(5,2) NOT NULL CONSTRAINT DF_configuracion_igv_hotel DEFAULT (18.00),
        tasa_igv_productos DECIMAL(5,2) NOT NULL CONSTRAINT DF_configuracion_igv_productos DEFAULT (18.00),
        fecha_actualizacion DATETIME2 NOT NULL CONSTRAINT DF_configuracion_fecha DEFAULT (SYSDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.tipo_documento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tipo_documento (
        codigo CHAR(1) NOT NULL CONSTRAINT PK_tipo_documento PRIMARY KEY,
        descripcion NVARCHAR(60) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.metodo_pago', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.metodo_pago (
        codigo CHAR(3) NOT NULL CONSTRAINT PK_metodo_pago PRIMARY KEY,
        descripcion NVARCHAR(60) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.tipo_comprobante', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tipo_comprobante (
        codigo CHAR(2) NOT NULL CONSTRAINT PK_tipo_comprobante PRIMARY KEY,
        descripcion NVARCHAR(60) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.afectacion_igv', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.afectacion_igv (
        codigo CHAR(2) NOT NULL CONSTRAINT PK_afectacion_igv PRIMARY KEY,
        descripcion NVARCHAR(60) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.categoria_producto', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.categoria_producto (
        id_categoria INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_categoria_producto PRIMARY KEY,
        nombre NVARCHAR(50) NOT NULL,
        descripcion NVARCHAR(100) NULL,
        mostrar_en_ventas BIT NOT NULL CONSTRAINT DF_categoria_producto_mostrar DEFAULT (1)
    );
END
GO

IF OBJECT_ID(N'dbo.estado_habitacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.estado_habitacion (
        id_estado INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_estado_habitacion PRIMARY KEY,
        nombre NVARCHAR(30) NOT NULL,
        descripcion NVARCHAR(100) NULL,
        permite_checkin BIT NOT NULL CONSTRAINT DF_estado_habitacion_checkin DEFAULT (0),
        permite_checkout BIT NOT NULL CONSTRAINT DF_estado_habitacion_checkout DEFAULT (0),
        es_estado_final BIT NOT NULL CONSTRAINT DF_estado_habitacion_final DEFAULT (0),
        color_ui VARCHAR(20) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.rol_usuario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.rol_usuario (
        id_rol INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_rol_usuario PRIMARY KEY,
        nombre NVARCHAR(30) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.estado_sunat', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.estado_sunat (
        codigo INT NOT NULL CONSTRAINT PK_estado_sunat PRIMARY KEY,
        descripcion NVARCHAR(60) NOT NULL,
        descripcion_larga NVARCHAR(200) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.estado_reserva', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.estado_reserva (
        id_estado_reserva INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_estado_reserva PRIMARY KEY,
        codigo NVARCHAR(20) NOT NULL CONSTRAINT UQ_estado_reserva_codigo UNIQUE,
        descripcion NVARCHAR(100) NULL,
        es_final BIT NOT NULL CONSTRAINT DF_estado_reserva_final DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.estado_estancia', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.estado_estancia (
        id_estado_estancia INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_estado_estancia PRIMARY KEY,
        codigo NVARCHAR(20) NOT NULL CONSTRAINT UQ_estado_estancia_codigo UNIQUE,
        descripcion NVARCHAR(100) NULL,
        es_final BIT NOT NULL CONSTRAINT DF_estado_estancia_final DEFAULT (0)
    );
END
GO

IF OBJECT_ID(N'dbo.tipo_movimiento_stock', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tipo_movimiento_stock (
        codigo NVARCHAR(20) NOT NULL CONSTRAINT PK_tipo_movimiento_stock PRIMARY KEY,
        descripcion NVARCHAR(100) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.temporada', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.temporada (
        id_temporada INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_temporada PRIMARY KEY,
        nombre NVARCHAR(50) NOT NULL,
        fecha_inicio DATE NOT NULL,
        fecha_fin DATE NOT NULL,
        multiplicador DECIMAL(5,2) NOT NULL CONSTRAINT DF_temporada_mult DEFAULT (1.00),
        CONSTRAINT CK_temporada_fechas CHECK (fecha_fin >= fecha_inicio)
    );
END
GO

IF OBJECT_ID(N'dbo.parametro_hotel', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.parametro_hotel (
        id_parametro INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_parametro_hotel PRIMARY KEY,
        clave NVARCHAR(100) NOT NULL CONSTRAINT UQ_parametro_hotel_clave UNIQUE,
        valor NVARCHAR(500) NOT NULL,
        descripcion NVARCHAR(200) NULL,
        fecha_actualizacion DATETIME2 NOT NULL CONSTRAINT DF_parametro_hotel_fecha DEFAULT (SYSDATETIME())
    );
END
GO

IF OBJECT_ID(N'dbo.login_attempt', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.login_attempt (
        id_login_attempt INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_login_attempt PRIMARY KEY,
        ip_address NVARCHAR(50) NOT NULL,
        username NVARCHAR(100) NULL,
        attempted_at DATETIME2 NOT NULL CONSTRAINT DF_login_attempt_fecha DEFAULT (SYSDATETIME()),
        succeeded BIT NOT NULL CONSTRAINT DF_login_attempt_ok DEFAULT (0),
        user_agent NVARCHAR(500) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.audit_log', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.audit_log (
        id_audit BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_audit_log PRIMARY KEY,
        tabla NVARCHAR(128) NOT NULL,
        id_registro NVARCHAR(100) NULL,
        accion NVARCHAR(20) NOT NULL,
        usuario NVARCHAR(100) NULL,
        fecha DATETIME2 NOT NULL CONSTRAINT DF_audit_log_fecha DEFAULT (SYSDATETIME()),
        datos_anteriores NVARCHAR(MAX) NULL,
        datos_nuevos NVARCHAR(MAX) NULL,
        ip_address NVARCHAR(50) NULL,
        modulo NVARCHAR(50) NULL
    );
END
GO

/* ================================================================
   TABLAS DE NEGOCIO PRINCIPAL
   ================================================================ */

IF OBJECT_ID(N'dbo.usuario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.usuario (
        id_usuario INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_usuario PRIMARY KEY,
        username NVARCHAR(50) NOT NULL CONSTRAINT UQ_usuario_username UNIQUE,
        password_hash NVARCHAR(255) NOT NULL,
        id_rol INT NOT NULL,
        fecha_creacion DATETIME2 NOT NULL CONSTRAINT DF_usuario_fecha DEFAULT (SYSDATETIME()),
        esta_activo BIT NOT NULL CONSTRAINT DF_usuario_activo DEFAULT (1),
        debe_cambiar_password BIT NOT NULL CONSTRAINT DF_usuario_cambio DEFAULT (1),
        CONSTRAINT FK_usuario_rol FOREIGN KEY (id_rol) REFERENCES dbo.rol_usuario(id_rol)
    );
END
GO

IF OBJECT_ID(N'dbo.cliente', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.cliente (
        id_cliente INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_cliente PRIMARY KEY,
        codigo_interno NVARCHAR(40) NOT NULL CONSTRAINT DF_cliente_codigo DEFAULT (CONCAT(N'CLI-', REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''))) CONSTRAINT UQ_cliente_codigo UNIQUE,
        tipo_documento CHAR(1) NULL,
        documento NVARCHAR(20) NULL,
        nombres NVARCHAR(100) NULL,
        apellidos NVARCHAR(100) NULL,
        alias NVARCHAR(120) NULL,
        nacionalidad NVARCHAR(50) NOT NULL CONSTRAINT DF_cliente_nacionalidad DEFAULT (N'PERUANA'),
        fecha_nacimiento DATE NULL,
        telefono NVARCHAR(15) NULL,
        email NVARCHAR(100) NULL,
        direccion NVARCHAR(200) NULL,
        fecha_registro DATETIME2 NOT NULL CONSTRAINT DF_cliente_fecha DEFAULT (SYSDATETIME()),
        fecha_verificacion_reniec DATETIME2 NULL,
        CONSTRAINT FK_cliente_tipo_documento FOREIGN KEY (tipo_documento) REFERENCES dbo.tipo_documento(codigo)
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_cliente_documento'
      AND object_id = OBJECT_ID(N'dbo.cliente')
)
BEGIN
    CREATE UNIQUE INDEX UX_cliente_documento
    ON dbo.cliente(tipo_documento, documento)
    WHERE tipo_documento IS NOT NULL AND documento IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.tipo_habitacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tipo_habitacion (
        id_tipo INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tipo_habitacion PRIMARY KEY,
        nombre NVARCHAR(50) NOT NULL CONSTRAINT UQ_tipo_habitacion_nombre UNIQUE,
        capacidad INT NOT NULL CONSTRAINT DF_tipo_habitacion_capacidad DEFAULT (2),
        descripcion NVARCHAR(200) NULL,
        precio_base DECIMAL(10,2) NOT NULL CONSTRAINT DF_tipo_habitacion_precio DEFAULT (50.00),
        CONSTRAINT CK_tipo_habitacion_capacidad CHECK (capacidad > 0),
        CONSTRAINT CK_tipo_habitacion_precio CHECK (precio_base >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.tarifa', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tarifa (
        id_tarifa INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tarifa PRIMARY KEY,
        id_tipo_habitacion INT NOT NULL,
        id_temporada INT NULL,
        precio DECIMAL(10,2) NOT NULL,
        fecha_inicio DATE NULL,
        fecha_fin DATE NULL,
        CONSTRAINT FK_tarifa_tipo_habitacion FOREIGN KEY (id_tipo_habitacion) REFERENCES dbo.tipo_habitacion(id_tipo),
        CONSTRAINT FK_tarifa_temporada FOREIGN KEY (id_temporada) REFERENCES dbo.temporada(id_temporada),
        CONSTRAINT CK_tarifa_precio CHECK (precio >= 0),
        CONSTRAINT CK_tarifa_fechas CHECK (fecha_inicio IS NULL OR fecha_fin IS NULL OR fecha_fin >= fecha_inicio)
    );
END
GO

IF OBJECT_ID(N'dbo.habitacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.habitacion (
        id_habitacion INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_habitacion PRIMARY KEY,
        numero_habitacion VARCHAR(10) NOT NULL CONSTRAINT UQ_habitacion_numero UNIQUE,
        piso INT NOT NULL CONSTRAINT DF_habitacion_piso DEFAULT (1),
        descripcion NVARCHAR(200) NULL,
        caracteristicas NVARCHAR(MAX) NULL,
        id_tipo INT NOT NULL,
        precio_noche DECIMAL(10,2) NOT NULL CONSTRAINT DF_habitacion_precio DEFAULT (50.00),
        id_estado INT NOT NULL,
        fecha_ultimo_cambio DATETIME2 NOT NULL CONSTRAINT DF_habitacion_fecha DEFAULT (SYSDATETIME()),
        usuario_cambio INT NULL,
        CONSTRAINT FK_habitacion_tipo FOREIGN KEY (id_tipo) REFERENCES dbo.tipo_habitacion(id_tipo),
        CONSTRAINT FK_habitacion_estado FOREIGN KEY (id_estado) REFERENCES dbo.estado_habitacion(id_estado),
        CONSTRAINT FK_habitacion_usuario FOREIGN KEY (usuario_cambio) REFERENCES dbo.usuario(id_usuario),
        CONSTRAINT CK_habitacion_precio CHECK (precio_noche >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.historial_estado_habitacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.historial_estado_habitacion (
        id_historial INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_historial_estado_habitacion PRIMARY KEY,
        id_habitacion INT NOT NULL,
        id_estado_anterior INT NULL,
        id_estado_nuevo INT NOT NULL,
        fecha_cambio DATETIME2 NOT NULL CONSTRAINT DF_historial_estado_fecha DEFAULT (SYSDATETIME()),
        id_usuario INT NULL,
        observacion NVARCHAR(200) NULL,
        CONSTRAINT FK_historial_habitacion FOREIGN KEY (id_habitacion) REFERENCES dbo.habitacion(id_habitacion),
        CONSTRAINT FK_historial_estado_anterior FOREIGN KEY (id_estado_anterior) REFERENCES dbo.estado_habitacion(id_estado),
        CONSTRAINT FK_historial_estado_nuevo FOREIGN KEY (id_estado_nuevo) REFERENCES dbo.estado_habitacion(id_estado),
        CONSTRAINT FK_historial_usuario FOREIGN KEY (id_usuario) REFERENCES dbo.usuario(id_usuario)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_historial_habitacion_fecha'
      AND object_id = OBJECT_ID(N'dbo.historial_estado_habitacion')
)
BEGIN
    CREATE INDEX IX_historial_habitacion_fecha
    ON dbo.historial_estado_habitacion(id_habitacion, fecha_cambio DESC);
END
GO

IF OBJECT_ID(N'dbo.transicion_estado', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.transicion_estado (
        id_transicion INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_transicion_estado PRIMARY KEY,
        id_estado_actual INT NOT NULL,
        id_estado_siguiente INT NOT NULL,
        CONSTRAINT UQ_transicion_estado UNIQUE (id_estado_actual, id_estado_siguiente),
        CONSTRAINT FK_transicion_actual FOREIGN KEY (id_estado_actual) REFERENCES dbo.estado_habitacion(id_estado),
        CONSTRAINT FK_transicion_siguiente FOREIGN KEY (id_estado_siguiente) REFERENCES dbo.estado_habitacion(id_estado),
        CONSTRAINT CK_transicion_distinta CHECK (id_estado_actual <> id_estado_siguiente)
    );
END
GO

IF OBJECT_ID(N'dbo.reserva_corporativa', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.reserva_corporativa (
        id_reserva_corporativa INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_reserva_corporativa PRIMARY KEY,
        id_cliente_empresa INT NOT NULL,
        fecha_inicio DATE NOT NULL,
        fecha_fin DATE NOT NULL,
        numero_habitaciones INT NOT NULL,
        estado NVARCHAR(20) NOT NULL CONSTRAINT DF_reserva_corporativa_estado DEFAULT (N'Pendiente'),
        observaciones NVARCHAR(300) NULL,
        fecha_registro DATETIME2 NOT NULL CONSTRAINT DF_reserva_corporativa_fecha DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_reserva_corporativa_cliente FOREIGN KEY (id_cliente_empresa) REFERENCES dbo.cliente(id_cliente),
        CONSTRAINT CK_reserva_corporativa_fechas CHECK (fecha_fin >= fecha_inicio),
        CONSTRAINT CK_reserva_corporativa_numero CHECK (numero_habitaciones > 0)
    );
END
GO

GO

IF OBJECT_ID(N'dbo.reserva', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.reserva (
        id_reserva INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_reserva PRIMARY KEY,
        id_cliente INT NOT NULL,
        id_habitacion INT NOT NULL,
        id_usuario INT NOT NULL,
        id_estado_reserva INT NOT NULL CONSTRAINT DF_reserva_estado DEFAULT (1),
        id_reserva_corporativa INT NULL,
        fecha_registro DATETIME2 NOT NULL CONSTRAINT DF_reserva_fecha DEFAULT (SYSDATETIME()),
        fecha_entrada_prevista DATETIME2 NOT NULL,
        fecha_salida_prevista DATETIME2 NOT NULL,
        monto_total DECIMAL(10,2) NOT NULL,
        observaciones NVARCHAR(300) NULL,
        es_no_show BIT NOT NULL CONSTRAINT DF_reserva_no_show DEFAULT (0),
        CONSTRAINT FK_reserva_cliente FOREIGN KEY (id_cliente) REFERENCES dbo.cliente(id_cliente),
        CONSTRAINT FK_reserva_habitacion FOREIGN KEY (id_habitacion) REFERENCES dbo.habitacion(id_habitacion),
        CONSTRAINT FK_reserva_usuario FOREIGN KEY (id_usuario) REFERENCES dbo.usuario(id_usuario),
        CONSTRAINT FK_reserva_estado FOREIGN KEY (id_estado_reserva) REFERENCES dbo.estado_reserva(id_estado_reserva),
        CONSTRAINT FK_reserva_corporativa FOREIGN KEY (id_reserva_corporativa) REFERENCES dbo.reserva_corporativa(id_reserva_corporativa),
        CONSTRAINT CK_reserva_fechas CHECK (fecha_salida_prevista > fecha_entrada_prevista),
        CONSTRAINT CK_reserva_monto CHECK (monto_total >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.estancia', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.estancia (
        id_estancia INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_estancia PRIMARY KEY,
        id_reserva INT NULL,
        id_reserva_corporativa INT NULL,
        id_habitacion INT NOT NULL,
        id_cliente_titular INT NOT NULL,
        id_estado_estancia INT NOT NULL CONSTRAINT DF_estancia_estado DEFAULT (1),
        fecha_checkin DATETIME2 NOT NULL,
        fecha_checkout_prevista DATETIME2 NOT NULL,
        fecha_checkout_real DATETIME2 NULL,
        monto_total DECIMAL(10,2) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_estancia_fecha DEFAULT (SYSDATETIME()),
        esta_fuera BIT NOT NULL CONSTRAINT DF_estancia_fuera DEFAULT (0),
        hora_salida_temporal DATETIME2 NULL,
        hora_regreso_temporal DATETIME2 NULL,
        llaves_dejadas BIT NULL,
        CONSTRAINT FK_estancia_reserva FOREIGN KEY (id_reserva) REFERENCES dbo.reserva(id_reserva),
        CONSTRAINT FK_estancia_reserva_corporativa FOREIGN KEY (id_reserva_corporativa) REFERENCES dbo.reserva_corporativa(id_reserva_corporativa),
        CONSTRAINT FK_estancia_habitacion FOREIGN KEY (id_habitacion) REFERENCES dbo.habitacion(id_habitacion),
        CONSTRAINT FK_estancia_cliente FOREIGN KEY (id_cliente_titular) REFERENCES dbo.cliente(id_cliente),
        CONSTRAINT FK_estancia_estado FOREIGN KEY (id_estado_estancia) REFERENCES dbo.estado_estancia(id_estado_estancia),
        CONSTRAINT CK_estancia_fechas CHECK (fecha_checkout_prevista >= fecha_checkin),
        CONSTRAINT CK_estancia_monto CHECK (monto_total >= 0),
        CONSTRAINT CK_estancia_llaves CHECK (llaves_dejadas IS NULL OR llaves_dejadas IN (0,1))
    );
END
GO

IF OBJECT_ID(N'dbo.huesped', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.huesped (
        id_huesped INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_huesped PRIMARY KEY,
        id_estancia INT NOT NULL,
        id_cliente INT NOT NULL,
        es_titular BIT NOT NULL CONSTRAINT DF_huesped_titular DEFAULT (0),
        fecha_registro DATETIME2 NOT NULL CONSTRAINT DF_huesped_fecha DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_huesped_estancia FOREIGN KEY (id_estancia) REFERENCES dbo.estancia(id_estancia),
        CONSTRAINT FK_huesped_cliente FOREIGN KEY (id_cliente) REFERENCES dbo.cliente(id_cliente)
    );
END
GO


IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_reserva_id_reserva_corporativa'
      AND object_id = OBJECT_ID(N'dbo.reserva')
)
BEGIN
    CREATE INDEX IX_reserva_id_reserva_corporativa
    ON dbo.reserva(id_reserva_corporativa);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_estancia_id_reserva_corporativa'
      AND object_id = OBJECT_ID(N'dbo.estancia')
)
BEGIN
    CREATE INDEX IX_estancia_id_reserva_corporativa
    ON dbo.estancia(id_reserva_corporativa)
    WHERE id_reserva_corporativa IS NOT NULL;
END


IF OBJECT_ID(N'dbo.producto', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.producto (
        id_producto INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_producto PRIMARY KEY,
        codigo_sunat NVARCHAR(20) NULL,
        nombre NVARCHAR(100) NOT NULL,
        descripcion NVARCHAR(200) NULL,
        imagen_url NVARCHAR(255) NULL,
        precio_unitario DECIMAL(10,2) NOT NULL,
        id_afectacion_igv CHAR(2) NOT NULL CONSTRAINT DF_producto_afectacion DEFAULT ('10'),
        id_categoria INT NULL,
        stock INT NOT NULL CONSTRAINT DF_producto_stock DEFAULT (0),
        stock_minimo INT NOT NULL CONSTRAINT DF_producto_stock_min DEFAULT (5),
        unidad_medida NVARCHAR(3) NOT NULL CONSTRAINT DF_producto_unidad DEFAULT (N'NIU'),
        es_amenidad BIT NOT NULL CONSTRAINT DF_producto_amenidad DEFAULT (0),
        es_vendible_en_tienda BIT NOT NULL CONSTRAINT DF_producto_vendible DEFAULT (1),
        stock_por_habitacion INT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_producto_fecha DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_producto_afectacion FOREIGN KEY (id_afectacion_igv) REFERENCES dbo.afectacion_igv(codigo),
        CONSTRAINT FK_producto_categoria FOREIGN KEY (id_categoria) REFERENCES dbo.categoria_producto(id_categoria),
        CONSTRAINT CK_producto_precio CHECK (precio_unitario >= 0),
        CONSTRAINT CK_producto_stock CHECK (stock >= 0),
        CONSTRAINT CK_producto_stock_min CHECK (stock_minimo >= 0),
        CONSTRAINT CK_producto_stock_por_habitacion CHECK (stock_por_habitacion IS NULL OR stock_por_habitacion >= 0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_producto_codigo_sunat'
      AND object_id = OBJECT_ID(N'dbo.producto')
)
BEGIN
    CREATE INDEX IX_producto_codigo_sunat ON dbo.producto(codigo_sunat);
END
GO

IF OBJECT_ID(N'dbo.stock_habitacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.stock_habitacion (
        id_stock INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_stock_habitacion PRIMARY KEY,
        id_habitacion INT NOT NULL,
        id_producto INT NOT NULL,
        cantidad_actual INT NOT NULL,
        fecha_actualizacion DATETIME2 NOT NULL CONSTRAINT DF_stock_habitacion_fecha DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_stock_habitacion UNIQUE (id_habitacion, id_producto),
        CONSTRAINT FK_stock_habitacion_habitacion FOREIGN KEY (id_habitacion) REFERENCES dbo.habitacion(id_habitacion),
        CONSTRAINT FK_stock_habitacion_producto FOREIGN KEY (id_producto) REFERENCES dbo.producto(id_producto),
        CONSTRAINT CK_stock_habitacion_cantidad CHECK (cantidad_actual >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.item_estancia', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.item_estancia (
        id_item INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_item_estancia PRIMARY KEY,
        id_estancia INT NOT NULL,
        id_producto INT NOT NULL,
        cantidad INT NOT NULL,
        precio_unitario DECIMAL(10,2) NOT NULL,
        subtotal AS (cantidad * precio_unitario) PERSISTED,
        fecha_registro DATETIME2 NOT NULL CONSTRAINT DF_item_estancia_fecha DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_item_estancia_estancia FOREIGN KEY (id_estancia) REFERENCES dbo.estancia(id_estancia),
        CONSTRAINT FK_item_estancia_producto FOREIGN KEY (id_producto) REFERENCES dbo.producto(id_producto),
        CONSTRAINT CK_item_estancia_cantidad CHECK (cantidad > 0),
        CONSTRAINT CK_item_estancia_precio CHECK (precio_unitario >= 0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_item_estancia_estancia'
      AND object_id = OBJECT_ID(N'dbo.item_estancia')
)
BEGIN
    CREATE INDEX IX_item_estancia_estancia ON dbo.item_estancia(id_estancia);
END
GO

IF OBJECT_ID(N'dbo.venta', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.venta (
        id_venta INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_venta PRIMARY KEY,
        id_cliente INT NULL,
        id_usuario INT NOT NULL,
        fecha_venta DATETIME2 NOT NULL CONSTRAINT DF_venta_fecha DEFAULT (SYSDATETIME()),
        total DECIMAL(10,2) NOT NULL,
        metodo_pago CHAR(3) NOT NULL,
        CONSTRAINT FK_venta_cliente FOREIGN KEY (id_cliente) REFERENCES dbo.cliente(id_cliente),
        CONSTRAINT FK_venta_usuario FOREIGN KEY (id_usuario) REFERENCES dbo.usuario(id_usuario),
        CONSTRAINT FK_venta_metodo_pago FOREIGN KEY (metodo_pago) REFERENCES dbo.metodo_pago(codigo),
        CONSTRAINT CK_venta_total CHECK (total >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.item_venta', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.item_venta (
        id_item INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_item_venta PRIMARY KEY,
        id_venta INT NOT NULL,
        id_producto INT NOT NULL,
        cantidad INT NOT NULL,
        precio_unitario DECIMAL(10,2) NOT NULL,
        subtotal AS (cantidad * precio_unitario) PERSISTED,
        CONSTRAINT FK_item_venta_venta FOREIGN KEY (id_venta) REFERENCES dbo.venta(id_venta),
        CONSTRAINT FK_item_venta_producto FOREIGN KEY (id_producto) REFERENCES dbo.producto(id_producto),
        CONSTRAINT CK_item_venta_cantidad CHECK (cantidad > 0),
        CONSTRAINT CK_item_venta_precio CHECK (precio_unitario >= 0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_item_venta_venta'
      AND object_id = OBJECT_ID(N'dbo.item_venta')
)
BEGIN
    CREATE INDEX IX_item_venta_venta ON dbo.item_venta(id_venta);
END
GO

IF OBJECT_ID(N'dbo.comprobante', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.comprobante (
        id_comprobante INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comprobante PRIMARY KEY,
        id_estancia INT NULL,
        id_venta INT NULL,
        tipo_comprobante CHAR(2) NOT NULL,
        serie NVARCHAR(4) NOT NULL,
        correlativo INT NOT NULL,
        fecha_emision DATETIME2 NOT NULL CONSTRAINT DF_comprobante_fecha DEFAULT (SYSDATETIME()),
        monto_total DECIMAL(10,2) NOT NULL,
        igv_monto DECIMAL(10,2) NOT NULL,
        cliente_documento_tipo CHAR(1) NULL,
        cliente_documento_num NVARCHAR(20) NULL,
        cliente_nombre NVARCHAR(200) NULL,
        metodo_pago CHAR(3) NULL,
        id_estado_sunat INT NOT NULL CONSTRAINT DF_comprobante_estado DEFAULT (1),
        xml_firmado NVARCHAR(MAX) NULL,
        cdr_zip VARBINARY(MAX) NULL,
        fecha_envio DATETIME2 NULL,
        intentos_envio INT NOT NULL CONSTRAINT DF_comprobante_intentos DEFAULT (0),
        hash_xml NVARCHAR(64) NULL,
        CONSTRAINT UQ_comprobante_serie_correlativo UNIQUE (serie, correlativo),
        CONSTRAINT FK_comprobante_estancia FOREIGN KEY (id_estancia) REFERENCES dbo.estancia(id_estancia),
        CONSTRAINT FK_comprobante_venta FOREIGN KEY (id_venta) REFERENCES dbo.venta(id_venta),
        CONSTRAINT FK_comprobante_tipo FOREIGN KEY (tipo_comprobante) REFERENCES dbo.tipo_comprobante(codigo),
        CONSTRAINT FK_comprobante_cliente_tipo FOREIGN KEY (cliente_documento_tipo) REFERENCES dbo.tipo_documento(codigo),
        CONSTRAINT FK_comprobante_metodo_pago FOREIGN KEY (metodo_pago) REFERENCES dbo.metodo_pago(codigo),
        CONSTRAINT FK_comprobante_estado_sunat FOREIGN KEY (id_estado_sunat) REFERENCES dbo.estado_sunat(codigo),
        CONSTRAINT CK_comprobante_un_solo_origen CHECK (
            (CASE WHEN id_estancia IS NOT NULL THEN 1 ELSE 0 END) +
            (CASE WHEN id_venta IS NOT NULL THEN 1 ELSE 0 END) = 1
        ),
        CONSTRAINT CK_comprobante_importe CHECK (monto_total >= 0 AND igv_monto >= 0)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_comprobante_fecha_emision'
      AND object_id = OBJECT_ID(N'dbo.comprobante')
)
BEGIN
    CREATE INDEX IX_comprobante_fecha_emision ON dbo.comprobante(fecha_emision);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_comprobante_cliente'
      AND object_id = OBJECT_ID(N'dbo.comprobante')
)
BEGIN
    CREATE INDEX IX_comprobante_cliente ON dbo.comprobante(cliente_documento_tipo, cliente_documento_num);
END
GO

IF OBJECT_ID(N'dbo.cierre_caja_envio', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.cierre_caja_envio (
        fecha DATE NOT NULL CONSTRAINT PK_cierre_caja_envio PRIMARY KEY,
        id_estado_sunat INT NOT NULL CONSTRAINT DF_cierre_caja_envio_estado DEFAULT (1),
        fecha_envio DATETIME2 NULL,
        intentos_envio INT NOT NULL CONSTRAINT DF_cierre_caja_envio_intentos DEFAULT (0),
        hash_xml NVARCHAR(64) NULL,
        CONSTRAINT FK_cierre_estado_sunat FOREIGN KEY (id_estado_sunat) REFERENCES dbo.estado_sunat(codigo)
    );
END
GO

IF OBJECT_ID(N'dbo.movimiento_stock', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.movimiento_stock (
        id_movimiento BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_movimiento_stock PRIMARY KEY,
        id_producto INT NOT NULL,
        id_habitacion INT NULL,
        id_estancia INT NULL,
        id_venta INT NULL,
        codigo_tipo_movimiento NVARCHAR(20) NOT NULL,
        id_usuario INT NOT NULL,
        cantidad INT NOT NULL,
        stock_anterior INT NULL,
        stock_nuevo INT NULL,
        costo_unitario DECIMAL(10,2) NULL,
        motivo NVARCHAR(300) NULL,
        fecha_movimiento DATETIME2 NOT NULL CONSTRAINT DF_movimiento_stock_fecha DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_movimiento_stock_producto FOREIGN KEY (id_producto) REFERENCES dbo.producto(id_producto),
        CONSTRAINT FK_movimiento_stock_habitacion FOREIGN KEY (id_habitacion) REFERENCES dbo.habitacion(id_habitacion),
        CONSTRAINT FK_movimiento_stock_estancia FOREIGN KEY (id_estancia) REFERENCES dbo.estancia(id_estancia),
        CONSTRAINT FK_movimiento_stock_venta FOREIGN KEY (id_venta) REFERENCES dbo.venta(id_venta),
        CONSTRAINT FK_movimiento_stock_usuario FOREIGN KEY (id_usuario) REFERENCES dbo.usuario(id_usuario),
        CONSTRAINT FK_movimiento_stock_tipo FOREIGN KEY (codigo_tipo_movimiento) REFERENCES dbo.tipo_movimiento_stock(codigo),
        CONSTRAINT CK_movimiento_stock_cantidad CHECK (cantidad > 0),
        CONSTRAINT CK_movimiento_stock_stock CHECK (
            (stock_anterior IS NULL OR stock_anterior >= 0) AND
            (stock_nuevo IS NULL OR stock_nuevo >= 0)
        )
    );
END
GO

IF OBJECT_ID(N'dbo.incidente', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.incidente (
        id_incidente INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_incidente PRIMARY KEY,
        id_estancia INT NULL,
        id_habitacion INT NOT NULL,
        tipo NVARCHAR(50) NOT NULL,
        descripcion NVARCHAR(500) NOT NULL,
        imagen_url NVARCHAR(255) NULL,
        costo_estimado DECIMAL(10,2) NULL,
        cobrado_al_cliente BIT NOT NULL CONSTRAINT DF_incidente_cobrado DEFAULT (0),
        resuelto BIT NOT NULL CONSTRAINT DF_incidente_resuelto DEFAULT (0),
        fecha_registro DATETIME2 NOT NULL CONSTRAINT DF_incidente_fecha DEFAULT (SYSDATETIME()),
        reportado_por INT NULL,
        CONSTRAINT FK_incidente_estancia FOREIGN KEY (id_estancia) REFERENCES dbo.estancia(id_estancia),
        CONSTRAINT FK_incidente_habitacion FOREIGN KEY (id_habitacion) REFERENCES dbo.habitacion(id_habitacion),
        CONSTRAINT FK_incidente_usuario FOREIGN KEY (reportado_por) REFERENCES dbo.usuario(id_usuario)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_incidente_habitacion_fecha'
      AND object_id = OBJECT_ID(N'dbo.incidente')
)
BEGIN
    CREATE INDEX IX_incidente_habitacion_fecha ON dbo.incidente(id_habitacion, fecha_registro DESC);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_incidente_estancia'
      AND object_id = OBJECT_ID(N'dbo.incidente')
)
BEGIN
    CREATE INDEX IX_incidente_estancia ON dbo.incidente(id_estancia);
END
GO

IF OBJECT_ID(N'dbo.objeto_perdido', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.objeto_perdido (
        id_objeto INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_objeto_perdido PRIMARY KEY,
        id_habitacion INT NULL,
        id_estancia INT NULL,
        descripcion NVARCHAR(200) NOT NULL,
        imagen_url NVARCHAR(255) NULL,
        fecha_hallazgo DATETIME2 NOT NULL CONSTRAINT DF_objeto_perdido_fecha DEFAULT (SYSDATETIME()),
        estado NVARCHAR(20) NOT NULL CONSTRAINT DF_objeto_perdido_estado DEFAULT (N'pendiente'),
        entregado_a NVARCHAR(100) NULL,
        fecha_entregado DATETIME2 NULL,
        CONSTRAINT FK_objeto_perdido_habitacion FOREIGN KEY (id_habitacion) REFERENCES dbo.habitacion(id_habitacion),
        CONSTRAINT FK_objeto_perdido_estancia FOREIGN KEY (id_estancia) REFERENCES dbo.estancia(id_estancia)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_objeto_estado'
      AND object_id = OBJECT_ID(N'dbo.objeto_perdido')
)
BEGIN
    CREATE INDEX IX_objeto_estado ON dbo.objeto_perdido(estado);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_objeto_fecha'
      AND object_id = OBJECT_ID(N'dbo.objeto_perdido')
)
BEGIN
    CREATE INDEX IX_objeto_fecha ON dbo.objeto_perdido(fecha_hallazgo DESC);
END
GO

IF OBJECT_ID(N'dbo.historial_traslado', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.historial_traslado (
        id_traslado INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_historial_traslado PRIMARY KEY,
        id_estancia INT NOT NULL,
        id_habitacion_origen INT NOT NULL,
        id_habitacion_destino INT NOT NULL,
        motivo NVARCHAR(200) NULL,
        fecha_traslado DATETIME2 NOT NULL CONSTRAINT DF_historial_traslado_fecha DEFAULT (SYSDATETIME()),
        usuario_id INT NOT NULL,
        ajuste_monto DECIMAL(10,2) NULL,
        CONSTRAINT FK_traslado_estancia FOREIGN KEY (id_estancia) REFERENCES dbo.estancia(id_estancia),
        CONSTRAINT FK_traslado_habitacion_origen FOREIGN KEY (id_habitacion_origen) REFERENCES dbo.habitacion(id_habitacion),
        CONSTRAINT FK_traslado_habitacion_destino FOREIGN KEY (id_habitacion_destino) REFERENCES dbo.habitacion(id_habitacion),
        CONSTRAINT FK_traslado_usuario FOREIGN KEY (usuario_id) REFERENCES dbo.usuario(id_usuario)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_traslado_estancia'
      AND object_id = OBJECT_ID(N'dbo.historial_traslado')
)
BEGIN
    CREATE INDEX IX_traslado_estancia ON dbo.historial_traslado(id_estancia);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_traslado_fecha'
      AND object_id = OBJECT_ID(N'dbo.historial_traslado')
)
BEGIN
    CREATE INDEX IX_traslado_fecha ON dbo.historial_traslado(fecha_traslado DESC);
END
GO

IF OBJECT_ID(N'dbo.habitacion_amenidad', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.habitacion_amenidad (
        id_habitacion_amenidad INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_habitacion_amenidad PRIMARY KEY,
        id_habitacion INT NOT NULL,
        id_producto INT NOT NULL,
        cantidad_base INT NOT NULL CONSTRAINT DF_habitacion_amenidad_cantidad DEFAULT (1),
        CONSTRAINT UQ_habitacion_amenidad UNIQUE (id_habitacion, id_producto),
        CONSTRAINT FK_habitacion_amenidad_habitacion FOREIGN KEY (id_habitacion) REFERENCES dbo.habitacion(id_habitacion),
        CONSTRAINT FK_habitacion_amenidad_producto FOREIGN KEY (id_producto) REFERENCES dbo.producto(id_producto),
        CONSTRAINT CK_habitacion_amenidad_cantidad CHECK (cantidad_base > 0)
    );
END
GO

/* ================================================================
   ÍNDICES ADICIONALES
   ================================================================ */

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_login_attempt_ip_fecha'
      AND object_id = OBJECT_ID(N'dbo.login_attempt')
)
BEGIN
    CREATE INDEX IX_login_attempt_ip_fecha ON dbo.login_attempt(ip_address, attempted_at);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_login_attempt_username_at'
      AND object_id = OBJECT_ID(N'dbo.login_attempt')
)
BEGIN
    CREATE INDEX IX_login_attempt_username_at ON dbo.login_attempt(username, attempted_at);
END
GO

/* ================================================================
   VISTAS DE REPORTE
   ================================================================ */

CREATE OR ALTER VIEW dbo.v_cierre_caja_diario
AS
SELECT
    CAST(c.fecha_emision AS DATE) AS fecha,
    mp.descripcion AS metodo_pago,
    SUM(c.monto_total) AS ingresos,
    N'Hospedaje' AS concepto
FROM dbo.comprobante c
INNER JOIN dbo.metodo_pago mp ON c.metodo_pago = mp.codigo
WHERE c.tipo_comprobante = '03'
GROUP BY CAST(c.fecha_emision AS DATE), mp.descripcion
UNION ALL
SELECT
    CAST(v.fecha_venta AS DATE) AS fecha,
    mp.descripcion AS metodo_pago,
    SUM(v.total) AS ingresos,
    N'Productos' AS concepto
FROM dbo.venta v
INNER JOIN dbo.metodo_pago mp ON v.metodo_pago = mp.codigo
GROUP BY CAST(v.fecha_venta AS DATE), mp.descripcion;
GO

CREATE OR ALTER VIEW dbo.v_estado_habitaciones
AS
SELECT
    h.numero_habitacion,
    th.nombre AS tipo_habitacion,
    eh.nombre AS estado,
    h.precio_noche,
    h.fecha_ultimo_cambio
FROM dbo.habitacion h
INNER JOIN dbo.estado_habitacion eh ON h.id_estado = eh.id_estado
INNER JOIN dbo.tipo_habitacion th ON h.id_tipo = th.id_tipo;
GO

CREATE OR ALTER VIEW dbo.v_ocupacion_diaria
AS
SELECT
    CAST(e.fecha_checkin AS DATE) AS fecha,
    COUNT(*) AS ocupadas,
    (SELECT COUNT(*) FROM dbo.habitacion) AS total,
    CAST(COUNT(*) * 100.0 / NULLIF((SELECT COUNT(*) FROM dbo.habitacion), 0) AS DECIMAL(5,2)) AS porcentaje_ocupacion
FROM dbo.estancia e
INNER JOIN dbo.estado_estancia ee ON e.id_estado_estancia = ee.id_estado_estancia
WHERE ee.codigo = N'Activa'
GROUP BY CAST(e.fecha_checkin AS DATE);
GO

/* ================================================================
   DATOS SEMILLA
   ================================================================ */

IF NOT EXISTS (SELECT 1 FROM dbo.configuracion)
BEGIN
    INSERT INTO dbo.configuracion (id_configuracion, nombre, direccion, telefono, ruc)
    VALUES (1, N'Mi Hotel', N'Av. Principal 123', N'999-999-999', N'12345678901');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tipo_documento WHERE codigo = '1')
    INSERT INTO dbo.tipo_documento (codigo, descripcion) VALUES ('1', N'DNI');
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_documento WHERE codigo = '6')
    INSERT INTO dbo.tipo_documento (codigo, descripcion) VALUES ('6', N'RUC');
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_documento WHERE codigo = '7')
    INSERT INTO dbo.tipo_documento (codigo, descripcion) VALUES ('7', N'Pasaporte');
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_documento WHERE codigo = '0')
    INSERT INTO dbo.tipo_documento (codigo, descripcion) VALUES ('0', N'Otros');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.metodo_pago WHERE codigo = '005')
    INSERT INTO dbo.metodo_pago (codigo, descripcion) VALUES ('005', N'Efectivo');
IF NOT EXISTS (SELECT 1 FROM dbo.metodo_pago WHERE codigo = '006')
    INSERT INTO dbo.metodo_pago (codigo, descripcion) VALUES ('006', N'Tarjeta de Crédito / Débito');
IF NOT EXISTS (SELECT 1 FROM dbo.metodo_pago WHERE codigo = '008')
    INSERT INTO dbo.metodo_pago (codigo, descripcion) VALUES ('008', N'Transferencia bancaria (Yape/Plin)');
IF NOT EXISTS (SELECT 1 FROM dbo.metodo_pago WHERE codigo = '001')
    INSERT INTO dbo.metodo_pago (codigo, descripcion) VALUES ('001', N'Depósito en cuenta');
IF NOT EXISTS (SELECT 1 FROM dbo.metodo_pago WHERE codigo = '999')
    INSERT INTO dbo.metodo_pago (codigo, descripcion) VALUES ('999', N'Otros');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tipo_comprobante WHERE codigo = '03')
    INSERT INTO dbo.tipo_comprobante (codigo, descripcion) VALUES ('03', N'Boleta de Venta');
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_comprobante WHERE codigo = '01')
    INSERT INTO dbo.tipo_comprobante (codigo, descripcion) VALUES ('01', N'Factura');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.afectacion_igv WHERE codigo = '10')
    INSERT INTO dbo.afectacion_igv (codigo, descripcion) VALUES ('10', N'Gravado - Operación Onerosa');
IF NOT EXISTS (SELECT 1 FROM dbo.afectacion_igv WHERE codigo = '20')
    INSERT INTO dbo.afectacion_igv (codigo, descripcion) VALUES ('20', N'Exonerado');
IF NOT EXISTS (SELECT 1 FROM dbo.afectacion_igv WHERE codigo = '30')
    INSERT INTO dbo.afectacion_igv (codigo, descripcion) VALUES ('30', N'Inafecto');
IF NOT EXISTS (SELECT 1 FROM dbo.afectacion_igv WHERE codigo = '40')
    INSERT INTO dbo.afectacion_igv (codigo, descripcion) VALUES ('40', N'Exportación');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.categoria_producto WHERE nombre = N'Bebidas')
    INSERT INTO dbo.categoria_producto (nombre, descripcion) VALUES (N'Bebidas', N'Bebidas alcohólicas y no alcohólicas');
IF NOT EXISTS (SELECT 1 FROM dbo.categoria_producto WHERE nombre = N'Snacks')
    INSERT INTO dbo.categoria_producto (nombre, descripcion) VALUES (N'Snacks', N'Snacks y piqueos');
IF NOT EXISTS (SELECT 1 FROM dbo.categoria_producto WHERE nombre = N'Servicios')
    INSERT INTO dbo.categoria_producto (nombre, descripcion) VALUES (N'Servicios', N'Servicios adicionales');
IF NOT EXISTS (SELECT 1 FROM dbo.categoria_producto WHERE nombre = N'Amenidades')
    INSERT INTO dbo.categoria_producto (nombre, descripcion) VALUES (N'Amenidades', N'Artículos de cortesía en la habitación');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.estado_habitacion WHERE nombre = N'Disponible')
    INSERT INTO dbo.estado_habitacion (nombre, descripcion, permite_checkin, permite_checkout, es_estado_final, color_ui)
    VALUES (N'Disponible', N'Lista para ser ocupada', 1, 0, 0, 'success');
IF NOT EXISTS (SELECT 1 FROM dbo.estado_habitacion WHERE nombre = N'Ocupada')
    INSERT INTO dbo.estado_habitacion (nombre, descripcion, permite_checkin, permite_checkout, es_estado_final, color_ui)
    VALUES (N'Ocupada', N'Con huéspedes actualmente', 0, 1, 0, 'warning');
IF NOT EXISTS (SELECT 1 FROM dbo.estado_habitacion WHERE nombre = N'Limpieza')
    INSERT INTO dbo.estado_habitacion (nombre, descripcion, permite_checkin, permite_checkout, es_estado_final, color_ui)
    VALUES (N'Limpieza', N'En proceso de limpieza', 0, 0, 0, 'info');
IF NOT EXISTS (SELECT 1 FROM dbo.estado_habitacion WHERE nombre = N'Mantenimiento')
    INSERT INTO dbo.estado_habitacion (nombre, descripcion, permite_checkin, permite_checkout, es_estado_final, color_ui)
    VALUES (N'Mantenimiento', N'Fuera de servicio', 0, 0, 0, 'error');
IF NOT EXISTS (SELECT 1 FROM dbo.estado_habitacion WHERE nombre = N'En Reserva')
    INSERT INTO dbo.estado_habitacion (nombre, descripcion, permite_checkin, permite_checkout, es_estado_final, color_ui)
    VALUES (N'En Reserva', N'Habitación reservada para hoy, esperando check-in', 1, 0, 0, 'warning');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.rol_usuario WHERE nombre = N'Administrador')
    INSERT INTO dbo.rol_usuario (nombre) VALUES (N'Administrador');
IF NOT EXISTS (SELECT 1 FROM dbo.rol_usuario WHERE nombre = N'Recepcion')
    INSERT INTO dbo.rol_usuario (nombre) VALUES (N'Recepcion');
IF NOT EXISTS (SELECT 1 FROM dbo.rol_usuario WHERE nombre = N'Limpieza')
    INSERT INTO dbo.rol_usuario (nombre) VALUES (N'Limpieza');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.estado_sunat WHERE codigo = 1)
    INSERT INTO dbo.estado_sunat (codigo, descripcion, descripcion_larga) VALUES (1, N'Pendiente', N'El comprobante se generó pero no se ha enviado.');
IF NOT EXISTS (SELECT 1 FROM dbo.estado_sunat WHERE codigo = 2)
    INSERT INTO dbo.estado_sunat (codigo, descripcion, descripcion_larga) VALUES (2, N'Enviado', N'El comprobante fue enviado y se espera respuesta de SUNAT.');
IF NOT EXISTS (SELECT 1 FROM dbo.estado_sunat WHERE codigo = 3)
    INSERT INTO dbo.estado_sunat (codigo, descripcion, descripcion_larga) VALUES (3, N'Aceptado', N'El comprobante fue validado exitosamente por SUNAT.');
IF NOT EXISTS (SELECT 1 FROM dbo.estado_sunat WHERE codigo = 4)
    INSERT INTO dbo.estado_sunat (codigo, descripcion, descripcion_larga) VALUES (4, N'Rechazado', N'El comprobante fue RECHAZADO. No tiene validez tributaria.');
IF NOT EXISTS (SELECT 1 FROM dbo.estado_sunat WHERE codigo = 5)
    INSERT INTO dbo.estado_sunat (codigo, descripcion, descripcion_larga) VALUES (5, N'Observado', N'Aceptado con observaciones menores.');
IF NOT EXISTS (SELECT 1 FROM dbo.estado_sunat WHERE codigo = 6)
    INSERT INTO dbo.estado_sunat (codigo, descripcion, descripcion_larga) VALUES (6, N'Anulado', N'El comprobante fue dado de baja.');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.estado_reserva WHERE codigo = N'Pendiente')
    INSERT INTO dbo.estado_reserva (codigo, descripcion, es_final) VALUES (N'Pendiente', N'Pendiente de confirmación', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.estado_reserva WHERE codigo = N'Confirmada')
    INSERT INTO dbo.estado_reserva (codigo, descripcion, es_final) VALUES (N'Confirmada', N'Reserva confirmada', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.estado_reserva WHERE codigo = N'Cancelada')
    INSERT INTO dbo.estado_reserva (codigo, descripcion, es_final) VALUES (N'Cancelada', N'Reserva cancelada', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.estado_reserva WHERE codigo = N'NoShow')
    INSERT INTO dbo.estado_reserva (codigo, descripcion, es_final) VALUES (N'NoShow', N'El cliente no se presentó', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.estado_reserva WHERE codigo = N'Finalizada')
    INSERT INTO dbo.estado_reserva (codigo, descripcion, es_final) VALUES (N'Finalizada', N'Reserva completada', 1);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.estado_estancia WHERE codigo = N'Activa')
    INSERT INTO dbo.estado_estancia (codigo, descripcion, es_final) VALUES (N'Activa', N'Estancia en curso', 0);
IF NOT EXISTS (SELECT 1 FROM dbo.estado_estancia WHERE codigo = N'Finalizada')
    INSERT INTO dbo.estado_estancia (codigo, descripcion, es_final) VALUES (N'Finalizada', N'Estancia cerrada', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.estado_estancia WHERE codigo = N'Cancelada')
    INSERT INTO dbo.estado_estancia (codigo, descripcion, es_final) VALUES (N'Cancelada', N'Estancia cancelada', 1);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tipo_movimiento_stock WHERE codigo = N'ENTRADA')
    INSERT INTO dbo.tipo_movimiento_stock (codigo, descripcion) VALUES (N'ENTRADA', N'Entrada de stock');
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_movimiento_stock WHERE codigo = N'SALIDA')
    INSERT INTO dbo.tipo_movimiento_stock (codigo, descripcion) VALUES (N'SALIDA', N'Salida de stock');
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_movimiento_stock WHERE codigo = N'AJUSTE')
    INSERT INTO dbo.tipo_movimiento_stock (codigo, descripcion) VALUES (N'AJUSTE', N'Ajuste de inventario');
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_movimiento_stock WHERE codigo = N'MERMA')
    INSERT INTO dbo.tipo_movimiento_stock (codigo, descripcion) VALUES (N'MERMA', N'Merma o pérdida');
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_movimiento_stock WHERE codigo = N'VENTA')
    INSERT INTO dbo.tipo_movimiento_stock (codigo, descripcion) VALUES (N'VENTA', N'Salida por venta');
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_movimiento_stock WHERE codigo = N'AMENIDAD')
    INSERT INTO dbo.tipo_movimiento_stock (codigo, descripcion) VALUES (N'AMENIDAD', N'Salida por amenidad');
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_movimiento_stock WHERE codigo = N'CONSUMO')
    INSERT INTO dbo.tipo_movimiento_stock (codigo, descripcion) VALUES (N'CONSUMO', N'Consumo en estancia');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.temporada WHERE nombre = N'Alta')
    INSERT INTO dbo.temporada (nombre, fecha_inicio, fecha_fin, multiplicador)
    VALUES (N'Alta', '2026-06-01', '2026-08-31', 1.20);
IF NOT EXISTS (SELECT 1 FROM dbo.temporada WHERE nombre = N'Baja')
    INSERT INTO dbo.temporada (nombre, fecha_inicio, fecha_fin, multiplicador)
    VALUES (N'Baja', '2026-09-01', '2026-11-30', 0.85);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tipo_habitacion WHERE nombre = N'Matrimonial')
    INSERT INTO dbo.tipo_habitacion (nombre, capacidad, descripcion, precio_base) VALUES (N'Matrimonial', 2, N'Habitación estándar para dos personas', 50.00);
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_habitacion WHERE nombre = N'Doble')
    INSERT INTO dbo.tipo_habitacion (nombre, capacidad, descripcion, precio_base) VALUES (N'Doble', 3, N'Habitación con dos camas individuales', 70.00);
IF NOT EXISTS (SELECT 1 FROM dbo.tipo_habitacion WHERE nombre = N'Suite')
    INSERT INTO dbo.tipo_habitacion (nombre, capacidad, descripcion, precio_base) VALUES (N'Suite', 4, N'Suite con sala de estar independiente', 120.00);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tarifa WHERE id_tipo_habitacion = 1 AND id_temporada IS NULL AND precio = 50.00)
    INSERT INTO dbo.tarifa (id_tipo_habitacion, id_temporada, precio) VALUES (1, NULL, 50.00);
IF NOT EXISTS (SELECT 1 FROM dbo.tarifa WHERE id_tipo_habitacion = 1 AND id_temporada = 1 AND precio = 60.00)
    INSERT INTO dbo.tarifa (id_tipo_habitacion, id_temporada, precio) VALUES (1, 1, 60.00);
IF NOT EXISTS (SELECT 1 FROM dbo.tarifa WHERE id_tipo_habitacion = 1 AND id_temporada = 2 AND precio = 42.50)
    INSERT INTO dbo.tarifa (id_tipo_habitacion, id_temporada, precio) VALUES (1, 2, 42.50);
IF NOT EXISTS (SELECT 1 FROM dbo.tarifa WHERE id_tipo_habitacion = 2 AND id_temporada = 1 AND precio = 84.00)
    INSERT INTO dbo.tarifa (id_tipo_habitacion, id_temporada, precio) VALUES (2, 1, 84.00);
IF NOT EXISTS (SELECT 1 FROM dbo.tarifa WHERE id_tipo_habitacion = 2 AND id_temporada = 2 AND precio = 59.50)
    INSERT INTO dbo.tarifa (id_tipo_habitacion, id_temporada, precio) VALUES (2, 2, 59.50);
IF NOT EXISTS (SELECT 1 FROM dbo.tarifa WHERE id_tipo_habitacion = 3 AND id_temporada = 1 AND precio = 144.00)
    INSERT INTO dbo.tarifa (id_tipo_habitacion, id_temporada, precio) VALUES (3, 1, 144.00);
IF NOT EXISTS (SELECT 1 FROM dbo.tarifa WHERE id_tipo_habitacion = 3 AND id_temporada = 2 AND precio = 102.00)
    INSERT INTO dbo.tarifa (id_tipo_habitacion, id_temporada, precio) VALUES (3, 2, 102.00);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.cliente WHERE codigo_interno = N'CLI-ANONIMO')
BEGIN
    INSERT INTO dbo.cliente (codigo_interno, tipo_documento, documento, nombres, apellidos, alias, nacionalidad)
    VALUES (N'CLI-ANONIMO', N'0', N'00000000', N'CLIENTE', N'ANONIMO', N'Cliente anónimo', N'PERUANA');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.habitacion WHERE numero_habitacion = '101')
    INSERT INTO dbo.habitacion (numero_habitacion, piso, id_tipo, precio_noche, id_estado) VALUES ('101', 1, 1, 50.00, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.habitacion WHERE numero_habitacion = '102')
    INSERT INTO dbo.habitacion (numero_habitacion, piso, id_tipo, precio_noche, id_estado) VALUES ('102', 1, 1, 50.00, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.habitacion WHERE numero_habitacion = '103')
    INSERT INTO dbo.habitacion (numero_habitacion, piso, id_tipo, precio_noche, id_estado) VALUES ('103', 1, 2, 70.00, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.habitacion WHERE numero_habitacion = '104')
    INSERT INTO dbo.habitacion (numero_habitacion, piso, id_tipo, precio_noche, id_estado) VALUES ('104', 1, 2, 70.00, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.habitacion WHERE numero_habitacion = '201')
    INSERT INTO dbo.habitacion (numero_habitacion, piso, id_tipo, precio_noche, id_estado) VALUES ('201', 2, 1, 60.00, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.habitacion WHERE numero_habitacion = '202')
    INSERT INTO dbo.habitacion (numero_habitacion, piso, id_tipo, precio_noche, id_estado) VALUES ('202', 2, 1, 60.00, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.habitacion WHERE numero_habitacion = '203')
    INSERT INTO dbo.habitacion (numero_habitacion, piso, id_tipo, precio_noche, id_estado) VALUES ('203', 2, 3, 120.00, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.habitacion WHERE numero_habitacion = '204')
    INSERT INTO dbo.habitacion (numero_habitacion, piso, id_tipo, precio_noche, id_estado) VALUES ('204', 2, 3, 120.00, 1);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.producto WHERE nombre = N'Agua Mineral 500ml')
    INSERT INTO dbo.producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock)
    VALUES (N'Agua Mineral 500ml', N'Agua sin gas', 2.50, '10', 1, 100);
IF NOT EXISTS (SELECT 1 FROM dbo.producto WHERE nombre = N'Gaseosa Coca-Cola 355ml')
    INSERT INTO dbo.producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock)
    VALUES (N'Gaseosa Coca-Cola 355ml', N'Gaseosa personal', 3.00, '10', 1, 80);
IF NOT EXISTS (SELECT 1 FROM dbo.producto WHERE nombre = N'Cerveza Cusqueña 330ml')
    INSERT INTO dbo.producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock)
    VALUES (N'Cerveza Cusqueña 330ml', N'Cerveza artesanal', 6.00, '10', 1, 50);
IF NOT EXISTS (SELECT 1 FROM dbo.producto WHERE nombre = N'Papas Lays 120g')
    INSERT INTO dbo.producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock)
    VALUES (N'Papas Lays 120g', N'Snack de papas fritas', 4.00, '10', 2, 60);
IF NOT EXISTS (SELECT 1 FROM dbo.producto WHERE nombre = N'Chocolate Sublime 50g')
    INSERT INTO dbo.producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock)
    VALUES (N'Chocolate Sublime 50g', N'Chocolate con leche', 3.50, '10', 2, 40);
IF NOT EXISTS (SELECT 1 FROM dbo.producto WHERE nombre = N'Servicio de Lavandería')
    INSERT INTO dbo.producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock, es_vendible_en_tienda)
    VALUES (N'Servicio de Lavandería', N'Lavado y planchado por prenda', 15.00, '10', 3, 999, 1);
IF NOT EXISTS (SELECT 1 FROM dbo.producto WHERE nombre = N'Llamada Nacional')
    INSERT INTO dbo.producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock, es_vendible_en_tienda)
    VALUES (N'Llamada Nacional', N'Por minuto', 0.50, '10', 3, 999, 1);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.producto WHERE nombre = N'Jabón de cortesía')
BEGIN
    INSERT INTO dbo.producto
        (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock, es_amenidad, es_vendible_en_tienda, stock_por_habitacion)
    VALUES
        (N'Jabón de cortesía', N'Jabón pequeño para huéspedes', 0, '10',
         (SELECT TOP 1 id_categoria FROM dbo.categoria_producto WHERE nombre = N'Amenidades'),
         500, 1, 0, 2);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.producto WHERE nombre = N'Champú de cortesía')
BEGIN
    INSERT INTO dbo.producto
        (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock, es_amenidad, es_vendible_en_tienda, stock_por_habitacion)
    VALUES
        (N'Champú de cortesía', N'Sobre de champú', 0, '10',
         (SELECT TOP 1 id_categoria FROM dbo.categoria_producto WHERE nombre = N'Amenidades'),
         500, 1, 0, 2);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.producto WHERE nombre = N'Toalla mediana')
BEGIN
    INSERT INTO dbo.producto
        (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock, es_amenidad, es_vendible_en_tienda, stock_por_habitacion)
    VALUES
        (N'Toalla mediana', N'Toalla de baño', 0, '10',
         (SELECT TOP 1 id_categoria FROM dbo.categoria_producto WHERE nombre = N'Amenidades'),
         100, 1, 0, 2);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.reserva_corporativa WHERE id_cliente_empresa = 1 AND fecha_inicio = '2026-06-01' AND fecha_fin = '2026-06-02')
BEGIN
    INSERT INTO dbo.reserva_corporativa (id_cliente_empresa, fecha_inicio, fecha_fin, numero_habitaciones, estado, observaciones)
    VALUES (1, '2026-06-01', '2026-06-02', 1, N'Pendiente', N'Dato semilla');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.estancia')
      AND name = N'metodo_pago'
)
BEGIN
    ALTER TABLE dbo.estancia
        ADD metodo_pago CHAR(3) NULL;
END
GO

IF OBJECT_ID(N'dbo.pago', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.pago (
        id_pago      INT IDENTITY(1,1) NOT NULL,
        id_estancia  INT NOT NULL,
        monto        DECIMAL(10,2) NOT NULL,
        metodo_pago  CHAR(3) NOT NULL,
        fecha_pago   DATETIME2 NOT NULL CONSTRAINT DF_pago_fecha_pago DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_pago PRIMARY KEY CLUSTERED (id_pago),
        CONSTRAINT FK_pago_estancia
            FOREIGN KEY (id_estancia) REFERENCES dbo.estancia (id_estancia)
            ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.refresh_token', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.refresh_token (
        id_refresh_token INT IDENTITY(1,1) NOT NULL,
        id_usuario       INT NOT NULL,
        token            VARCHAR(512) NOT NULL,
        expires_at       DATETIME2 NOT NULL,
        created_at       DATETIME2 NOT NULL CONSTRAINT DF_refresh_token_created_at DEFAULT (SYSDATETIME()),
        revoked_at       DATETIME2 NULL,

        CONSTRAINT PK_refresh_token PRIMARY KEY CLUSTERED (id_refresh_token),
        CONSTRAINT UQ_refresh_token_token UNIQUE (token),
        CONSTRAINT FK_refresh_token_usuario
            FOREIGN KEY (id_usuario) REFERENCES dbo.usuario (id_usuario)
            ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_refresh_token_token
        ON dbo.refresh_token (token);
END
GO

PRINT N'Base de datos HotelDB creada con éxito.';
GO
