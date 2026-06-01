-- ================================================================
--  Nombre: HotelGenericoDB_Create.sql
--  Motor : SQL Server 2019+
--  Descripción:
--    Script único para la creación completa de la base de datos
--    del sistema hotelero genérico. Nomenclatura unificada,
--    restricciones explícitas, índices estratégicos.
--    Incluye estado "En Reserva" y columna es_no_show.
-- ================================================================

USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HotelDB')
    CREATE DATABASE [HotelDB];
GO

USE [HotelDB];
GO

-- =============================================
-- TABLAS DE CATÁLOGO Y CONFIGURACIÓN
-- =============================================

-- Configuración global del hotel (única fila)
CREATE TABLE configuracion (
    id_configuracion INT PRIMARY KEY DEFAULT 1 CHECK (id_configuracion = 1),
    nombre NVARCHAR(100) NOT NULL,
    direccion NVARCHAR(200),
    telefono NVARCHAR(20),
    ruc NVARCHAR(11),
    tasa_igv_hotel DECIMAL(5,2) NOT NULL DEFAULT 18.00,
    tasa_igv_productos DECIMAL(5,2) NOT NULL DEFAULT 18.00
);

-- Documentos de identidad (SUNAT)
CREATE TABLE tipo_documento (
    codigo CHAR(1) PRIMARY KEY,
    descripcion NVARCHAR(60) NOT NULL
);

-- Métodos de pago (SUNAT)
CREATE TABLE metodo_pago (
    codigo CHAR(3) PRIMARY KEY,
    descripcion NVARCHAR(60) NOT NULL
);

-- Tipos de comprobante (SUNAT)
CREATE TABLE tipo_comprobante (
    codigo CHAR(2) PRIMARY KEY,
    descripcion NVARCHAR(60) NOT NULL
);

-- Afectación al IGV (SUNAT)
CREATE TABLE afectacion_igv (
    codigo CHAR(2) PRIMARY KEY,
    descripcion NVARCHAR(60) NOT NULL
);

-- Categorías de productos
CREATE TABLE categoria_producto (
    id_categoria INT PRIMARY KEY IDENTITY(1,1),
    nombre NVARCHAR(50) NOT NULL,
    descripcion NVARCHAR(100)
);

-- Estados de habitación (reglas de negocio)
CREATE TABLE estado_habitacion (
    id_estado INT PRIMARY KEY IDENTITY(1,1),
    nombre NVARCHAR(30) NOT NULL,
    descripcion NVARCHAR(100),
    permite_checkin BIT NOT NULL DEFAULT 0,
    permite_checkout BIT NOT NULL DEFAULT 0,
    es_estado_final BIT NOT NULL DEFAULT 0,
    color_ui VARCHAR(20)
);

-- Roles de usuario
CREATE TABLE rol_usuario (
    id_rol INT PRIMARY KEY IDENTITY(1,1),
    nombre NVARCHAR(30) NOT NULL
);

-- Estados de comprobante electrónico (SUNAT)
CREATE TABLE estado_sunat (
    codigo INT PRIMARY KEY,
    descripcion NVARCHAR(60) NOT NULL,
    descripcion_larga NVARCHAR(200)
);

-- Temporadas (tarifas variables)
CREATE TABLE temporada (
    id_temporada INT PRIMARY KEY IDENTITY(1,1),
    nombre NVARCHAR(50) NOT NULL,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    multiplicador DECIMAL(3,2) NOT NULL DEFAULT 1.00
);

-- =============================================
-- TABLAS DE NEGOCIO PRINCIPALES
-- =============================================

-- Usuarios del sistema
CREATE TABLE usuario (
    id_usuario INT PRIMARY KEY IDENTITY(1,1),
    username NVARCHAR(50) UNIQUE NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    id_rol INT NOT NULL,
    fecha_creacion DATETIME DEFAULT GETDATE(),
    esta_activo BIT DEFAULT 1,
    debe_cambiar_password BIT NOT NULL DEFAULT 1,
    CONSTRAINT fk_usuario_rol FOREIGN KEY (id_rol) REFERENCES rol_usuario(id_rol)
);

-- Clientes (huéspedes)
CREATE TABLE cliente (
    id_cliente INT PRIMARY KEY IDENTITY(1,1),
    tipo_documento CHAR(1) NOT NULL,
    documento NVARCHAR(20) NOT NULL,
    nombres NVARCHAR(100) NOT NULL,
    apellidos NVARCHAR(100) NOT NULL,
    nacionalidad NVARCHAR(50) DEFAULT 'PERUANA',
    fecha_nacimiento DATE,
    telefono NVARCHAR(15),
    email NVARCHAR(100),
    direccion NVARCHAR(200),
    fecha_registro DATETIME DEFAULT GETDATE(),
    fecha_verificacion_reniec DATETIME NULL,
    CONSTRAINT uq_cliente_documento UNIQUE (tipo_documento, documento),
    CONSTRAINT fk_cliente_tipo_documento FOREIGN KEY (tipo_documento) REFERENCES tipo_documento(codigo)
);

-- Tipos de habitación
CREATE TABLE tipo_habitacion (
    id_tipo INT PRIMARY KEY IDENTITY(1,1),
    nombre NVARCHAR(50) NOT NULL,
    capacidad INT DEFAULT 2,
    descripcion NVARCHAR(200),
    precio_base DECIMAL(10,2) DEFAULT 50.00
);

-- Tarifas por tipo de habitación y temporada
CREATE TABLE tarifa (
    id_tarifa INT PRIMARY KEY IDENTITY(1,1),
    id_tipo_habitacion INT NOT NULL,
    id_temporada INT NULL,
    precio DECIMAL(10,2) NOT NULL,
    fecha_inicio DATE NULL,
    fecha_fin DATE NULL,
    CONSTRAINT fk_tarifa_tipo_habitacion FOREIGN KEY (id_tipo_habitacion) REFERENCES tipo_habitacion(id_tipo),
    CONSTRAINT fk_tarifa_temporada FOREIGN KEY (id_temporada) REFERENCES temporada(id_temporada)
);

-- Habitaciones
CREATE TABLE habitacion (
    id_habitacion INT PRIMARY KEY IDENTITY(1,1),
    numero_habitacion VARCHAR(10) UNIQUE NOT NULL,
    piso INT DEFAULT 1,
    descripcion NVARCHAR(200),
    id_tipo INT NOT NULL,
    precio_noche DECIMAL(10,2) NOT NULL DEFAULT 50.00,
    id_estado INT NOT NULL DEFAULT 1,
    fecha_ultimo_cambio DATETIME DEFAULT GETDATE(),
    usuario_cambio INT,
    CONSTRAINT fk_habitacion_tipo FOREIGN KEY (id_tipo) REFERENCES tipo_habitacion(id_tipo),
    CONSTRAINT fk_habitacion_estado FOREIGN KEY (id_estado) REFERENCES estado_habitacion(id_estado),
    CONSTRAINT fk_habitacion_usuario FOREIGN KEY (usuario_cambio) REFERENCES usuario(id_usuario)
);

-- Histórico de cambios de estado
CREATE TABLE historial_estado_habitacion (
    id_historial INT PRIMARY KEY IDENTITY(1,1),
    id_habitacion INT NOT NULL,
    id_estado_anterior INT,
    id_estado_nuevo INT,
    fecha_cambio DATETIME DEFAULT GETDATE(),
    id_usuario INT,
    observacion NVARCHAR(200),
    CONSTRAINT fk_historial_habitacion FOREIGN KEY (id_habitacion) REFERENCES habitacion(id_habitacion),
    CONSTRAINT fk_historial_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario)
);

CREATE INDEX ix_historial_habitacion_fecha
    ON historial_estado_habitacion(id_habitacion, fecha_cambio DESC);

-- Transiciones permitidas entre estados (máquina de estados)
CREATE TABLE transicion_estado (
    id_transicion INT PRIMARY KEY IDENTITY(1,1),
    id_estado_actual INT NOT NULL,
    id_estado_siguiente INT NOT NULL,
    CONSTRAINT uq_transicion UNIQUE (id_estado_actual, id_estado_siguiente),
    CONSTRAINT fk_transicion_actual FOREIGN KEY (id_estado_actual) REFERENCES estado_habitacion(id_estado),
    CONSTRAINT fk_transicion_siguiente FOREIGN KEY (id_estado_siguiente) REFERENCES estado_habitacion(id_estado)
);

-- Reservas
CREATE TABLE reserva (
    id_reserva INT PRIMARY KEY IDENTITY(1,1),
    id_cliente INT NOT NULL,
    id_habitacion INT NOT NULL,
    id_usuario INT NOT NULL,
    fecha_registro DATETIME DEFAULT GETDATE(),
    fecha_entrada_prevista DATETIME NOT NULL,
    fecha_salida_prevista DATETIME NOT NULL,
    monto_total DECIMAL(10,2) NOT NULL,
    estado NVARCHAR(20) DEFAULT 'Pendiente',
    observaciones NVARCHAR(300),
    es_no_show BIT NOT NULL DEFAULT 0,
    CONSTRAINT fk_reserva_cliente FOREIGN KEY (id_cliente) REFERENCES cliente(id_cliente),
    CONSTRAINT fk_reserva_habitacion FOREIGN KEY (id_habitacion) REFERENCES habitacion(id_habitacion),
    CONSTRAINT fk_reserva_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario)
);

-- Estancias (Check-in real)
CREATE TABLE estancia (
    id_estancia INT PRIMARY KEY IDENTITY(1,1),
    id_reserva INT NULL,
    id_habitacion INT NOT NULL,
    id_cliente_titular INT NOT NULL,
    fecha_checkin DATETIME NOT NULL,
    fecha_checkout_prevista DATETIME NOT NULL,
    fecha_checkout_real DATETIME NULL,
    monto_total DECIMAL(10,2) NOT NULL,
    estado NVARCHAR(20) DEFAULT 'Activa',
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_estancia_reserva FOREIGN KEY (id_reserva) REFERENCES reserva(id_reserva),
    CONSTRAINT fk_estancia_habitacion FOREIGN KEY (id_habitacion) REFERENCES habitacion(id_habitacion),
    CONSTRAINT fk_estancia_cliente FOREIGN KEY (id_cliente_titular) REFERENCES cliente(id_cliente)
);

-- Huéspedes adicionales en la estancia
CREATE TABLE huesped (
    id_huesped INT PRIMARY KEY IDENTITY(1,1),
    id_estancia INT NOT NULL,
    id_cliente INT NOT NULL,
    es_titular BIT DEFAULT 0,
    fecha_registro DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_huesped_estancia FOREIGN KEY (id_estancia) REFERENCES estancia(id_estancia),
    CONSTRAINT fk_huesped_cliente FOREIGN KEY (id_cliente) REFERENCES cliente(id_cliente)
);

-- Productos
CREATE TABLE producto (
    id_producto INT PRIMARY KEY IDENTITY(1,1),
    codigo_sunat NVARCHAR(20) NULL,
    nombre NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(200),
    precio_unitario DECIMAL(10,2) NOT NULL,
    id_afectacion_igv CHAR(2) NOT NULL DEFAULT '10',
    id_categoria INT NULL,
    stock INT DEFAULT 0,
    stock_minimo INT DEFAULT 5,
    unidad_medida NVARCHAR(3) DEFAULT 'NIU',
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_producto_afectacion FOREIGN KEY (id_afectacion_igv) REFERENCES afectacion_igv(codigo),
    CONSTRAINT fk_producto_categoria FOREIGN KEY (id_categoria) REFERENCES categoria_producto(id_categoria)
);

CREATE INDEX ix_producto_codigo_sunat ON producto(codigo_sunat);

-- Consumos durante la estancia
CREATE TABLE item_estancia (
    id_item INT PRIMARY KEY IDENTITY(1,1),
    id_estancia INT NOT NULL,
    id_producto INT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,
    subtotal AS (cantidad * precio_unitario) PERSISTED,
    fecha_registro DATETIME DEFAULT GETDATE(),
    CONSTRAINT fk_item_estancia_estancia FOREIGN KEY (id_estancia) REFERENCES estancia(id_estancia),
    CONSTRAINT fk_item_estancia_producto FOREIGN KEY (id_producto) REFERENCES producto(id_producto)
);

CREATE INDEX ix_item_estancia_estancia ON item_estancia(id_estancia);

-- Ventas directas
CREATE TABLE venta (
    id_venta INT PRIMARY KEY IDENTITY(1,1),
    id_cliente INT NULL,
    id_usuario INT NOT NULL,
    fecha_venta DATETIME DEFAULT GETDATE(),
    total DECIMAL(10,2) NOT NULL,
    metodo_pago CHAR(3) NOT NULL,
    CONSTRAINT fk_venta_cliente FOREIGN KEY (id_cliente) REFERENCES cliente(id_cliente),
    CONSTRAINT fk_venta_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario),
    CONSTRAINT fk_venta_metodo_pago FOREIGN KEY (metodo_pago) REFERENCES metodo_pago(codigo)
);

-- Detalle de venta directa
CREATE TABLE item_venta (
    id_item INT PRIMARY KEY IDENTITY(1,1),
    id_venta INT NOT NULL,
    id_producto INT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,
    subtotal AS (cantidad * precio_unitario) PERSISTED,
    CONSTRAINT fk_item_venta_venta FOREIGN KEY (id_venta) REFERENCES venta(id_venta),
    CONSTRAINT fk_item_venta_producto FOREIGN KEY (id_producto) REFERENCES producto(id_producto)
);

CREATE INDEX ix_item_venta_venta ON item_venta(id_venta);

-- Comprobantes electrónicos
CREATE TABLE comprobante (
    id_comprobante INT PRIMARY KEY IDENTITY(1,1),
    id_estancia INT NULL,
    id_venta INT NULL,
    tipo_comprobante CHAR(2) NOT NULL,
    serie NVARCHAR(4) NOT NULL,
    correlativo INT NOT NULL,
    fecha_emision DATETIME DEFAULT GETDATE(),
    monto_total DECIMAL(10,2) NOT NULL,
    igv_monto DECIMAL(10,2) NOT NULL,
    cliente_documento_tipo CHAR(1) NULL,
    cliente_documento_num NVARCHAR(20) NULL,
    cliente_nombre NVARCHAR(200) NULL,
    metodo_pago CHAR(3) NULL,
    id_estado_sunat INT NOT NULL DEFAULT 1,
    xml_firmado NVARCHAR(MAX),
    cdr_zip VARBINARY(MAX),
    fecha_envio DATETIME NULL,
    intentos_envio INT DEFAULT 0,
    hash_xml NVARCHAR(64) NULL,
    CONSTRAINT uq_comprobante_serie_correlativo UNIQUE (serie, correlativo),
    CONSTRAINT fk_comprobante_estancia FOREIGN KEY (id_estancia) REFERENCES estancia(id_estancia),
    CONSTRAINT fk_comprobante_venta FOREIGN KEY (id_venta) REFERENCES venta(id_venta),
    CONSTRAINT fk_comprobante_tipo FOREIGN KEY (tipo_comprobante) REFERENCES tipo_comprobante(codigo),
    CONSTRAINT fk_comprobante_cliente_tipo FOREIGN KEY (cliente_documento_tipo) REFERENCES tipo_documento(codigo),
    CONSTRAINT fk_comprobante_metodo_pago FOREIGN KEY (metodo_pago) REFERENCES metodo_pago(codigo),
    CONSTRAINT fk_comprobante_estado_sunat FOREIGN KEY (id_estado_sunat) REFERENCES estado_sunat(codigo)
);

CREATE INDEX ix_comprobante_fecha_emision ON comprobante(fecha_emision);
CREATE INDEX ix_comprobante_cliente ON comprobante(cliente_documento_tipo, cliente_documento_num);

-- Envíos de cierre de caja a SUNAT
CREATE TABLE cierre_caja_envio (
    fecha DATE PRIMARY KEY,
    id_estado_sunat INT NOT NULL DEFAULT 1,
    fecha_envio DATETIME NULL,
    intentos_envio INT DEFAULT 0,
    hash_xml NVARCHAR(64) NULL,
    CONSTRAINT fk_cierre_estado_sunat FOREIGN KEY (id_estado_sunat) REFERENCES estado_sunat(codigo)
);
GO

-- =============================================
-- DATOS SEMILLA (modificables por el hotel)
-- =============================================

INSERT INTO configuracion (nombre, direccion, telefono, ruc)
VALUES ('Mi Hotel', 'Av. Principal 123', '999-999-999', '12345678901');

INSERT INTO tipo_documento (codigo, descripcion) VALUES
('1','DNI'), ('6','RUC'), ('7','Pasaporte'), ('0','Otros');

INSERT INTO metodo_pago (codigo, descripcion) VALUES
('005','Efectivo'), ('006','Tarjeta de Crédito / Débito'),
('008','Transferencia bancaria (Yape/Plin)'), ('001','Depósito en cuenta'), ('999','Otros');

INSERT INTO tipo_comprobante (codigo, descripcion) VALUES
('03','Boleta de Venta'), ('01','Factura');

INSERT INTO afectacion_igv (codigo, descripcion) VALUES
('10','Gravado - Operación Onerosa'), ('20','Exonerado'), ('30','Inafecto'), ('40','Exportación');

INSERT INTO categoria_producto (nombre, descripcion) VALUES
('Bebidas','Bebidas alcohólicas y no alcohólicas'),
('Snacks','Snacks y piqueos'),
('Servicios','Servicios adicionales');

INSERT INTO estado_habitacion (nombre, descripcion, permite_checkin, permite_checkout, es_estado_final, color_ui) VALUES
('Disponible','Lista para ser ocupada',1,0,0,'success'),
('Ocupada','Con huéspedes actualmente',0,1,0,'warning'),
('Limpieza','En proceso de limpieza',0,0,0,'info'),
('Mantenimiento','Fuera de servicio',0,0,0,'error'),
('En Reserva','Habitación reservada para hoy, esperando check-in',1,0,0,'warning');

INSERT INTO rol_usuario (nombre) VALUES
('Administrador'), ('Recepcionista'), ('Limpieza');

INSERT INTO estado_sunat (codigo, descripcion, descripcion_larga) VALUES
(1,'Pendiente','El comprobante se generó pero no se ha enviado.'),
(2,'Enviado','El comprobante fue enviado y se espera respuesta de SUNAT.'),
(3,'Aceptado','El comprobante fue validado exitosamente por SUNAT.'),
(4,'Rechazado','El comprobante fue RECHAZADO. No tiene validez tributaria.'),
(5,'Observado','Aceptado con observaciones menores.'),
(6,'Anulado','El comprobante fue dado de baja.');

INSERT INTO temporada (nombre, fecha_inicio, fecha_fin, multiplicador) VALUES
('Alta','2026-06-01','2026-08-31',1.20),
('Baja','2026-09-01','2026-11-30',0.85);

INSERT INTO tipo_habitacion (nombre, capacidad, descripcion, precio_base)
VALUES ('Matrimonial',2,'Habitación estándar para dos personas',50.00);

INSERT INTO tarifa (id_tipo_habitacion, id_temporada, precio)
VALUES (1,NULL,50.00); -- tarifa base sin temporada

INSERT INTO cliente (tipo_documento, documento, nombres, apellidos, nacionalidad)
VALUES ('0','00000000','CLIENTE','ANONIMO','PERUANA');

INSERT INTO transicion_estado (id_estado_actual, id_estado_siguiente) VALUES
(1,2),(2,3),(3,1),(1,4),(4,1),
(1,5),(5,2),(5,1);
GO

-- Insertar tipos de habitación adicionales
INSERT INTO tipo_habitacion (nombre, capacidad, descripcion, precio_base) VALUES
('Doble',3,'Habitación con dos camas individuales',70.00),
('Suite',4,'Suite con sala de estar independiente',120.00);
GO

-- Insertar habitaciones
INSERT INTO habitacion (numero_habitacion, piso, id_tipo, precio_noche, id_estado) VALUES
('101',1,1,50.00,1), ('102',1,1,50.00,1),
('103',1,2,70.00,1), ('104',1,2,70.00,1),
('201',2,1,60.00,1), ('202',2,1,60.00,1),
('203',2,3,120.00,1), ('204',2,3,120.00,1);
GO

-- Insertar productos
INSERT INTO producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock) VALUES
('Agua Mineral 500ml','Agua sin gas',2.50,'10',1,100),
('Gaseosa Coca-Cola 355ml','Gaseosa personal',3.00,'10',1,80),
('Cerveza Cusqueña 330ml','Cerveza artesanal',6.00,'10',1,50),
('Papas Lays 120g','Snack de papas fritas',4.00,'10',2,60),
('Chocolate Sublime 50g','Chocolate con leche',3.50,'10',2,40),
('Servicio de Lavandería','Lavado y planchado por prenda',15.00,'10',3,999),
('Llamada Nacional','Por minuto',0.50,'10',3,999);
GO

INSERT INTO tarifa (id_tipo_habitacion, id_temporada, precio) VALUES
(1,1,60.00),(1,2,42.50),
(2,1,84.00),(2,2,59.50),
(3,1,144.00),(3,2,102.00);
GO

-- Usuarios por defecto: los crea SetupService.cs al iniciar en desarrollo
-- Credenciales: admin/Admin123!, recepcion/Recepcion123!, limpieza/Limpieza123!

-- =============================================
-- VISTAS PARA REPORTES
-- =============================================

CREATE OR ALTER VIEW v_cierre_caja_diario AS
SELECT
    CAST(c.fecha_emision AS DATE) AS fecha,
    mp.descripcion AS metodo_pago,
    SUM(c.monto_total) AS ingresos,
    'Hospedaje' AS concepto
FROM comprobante c
INNER JOIN metodo_pago mp ON c.metodo_pago = mp.codigo
WHERE c.tipo_comprobante = '03'
GROUP BY CAST(c.fecha_emision AS DATE), mp.descripcion
UNION ALL
SELECT
    CAST(v.fecha_venta AS DATE) AS fecha,
    mp.descripcion AS metodo_pago,
    SUM(v.total) AS ingresos,
    'Productos' AS concepto
FROM venta v
INNER JOIN metodo_pago mp ON v.metodo_pago = mp.codigo
GROUP BY CAST(v.fecha_venta AS DATE), mp.descripcion;
GO

CREATE OR ALTER VIEW v_estado_habitaciones AS
SELECT
    h.numero_habitacion,
    th.nombre AS tipo_habitacion,
    eh.nombre AS estado,
    h.precio_noche,
    h.fecha_ultimo_cambio
FROM habitacion h
INNER JOIN estado_habitacion eh ON h.id_estado = eh.id_estado
INNER JOIN tipo_habitacion th ON h.id_tipo = th.id_tipo;
GO

CREATE OR ALTER VIEW v_ocupacion_diaria AS
SELECT
    CAST(fecha_checkin AS DATE) AS fecha,
    COUNT(*) AS ocupadas,
    (SELECT COUNT(*) FROM habitacion) AS total,
    CAST(COUNT(*) * 100.0 / NULLIF((SELECT COUNT(*) FROM habitacion), 0) AS DECIMAL(5,2)) AS porcentaje_ocupacion
FROM estancia
WHERE estado = 'Activa'
  AND CAST(fecha_checkin AS DATE) <= CAST(GETDATE() AS DATE)
  AND (fecha_checkout_real IS NULL OR CAST(fecha_checkout_real AS DATE) >= CAST(GETDATE() AS DATE))
GROUP BY CAST(fecha_checkin AS DATE);
GO

-- Tabla de intentos de inicio de sesión
CREATE TABLE login_attempt (
    id_login_attempt INT IDENTITY(1,1) PRIMARY KEY,
    ip_address NVARCHAR(50) NOT NULL,
    username NVARCHAR(100) NULL,
    attempted_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    succeeded BIT NOT NULL DEFAULT 0,
    user_agent NVARCHAR(500) NULL
);

CREATE INDEX ix_login_attempt_ip_fecha ON login_attempt(ip_address, attempted_at);
CREATE INDEX IX_login_attempt_username_at ON login_attempt(username, attempted_at);
GO

--  campo imagen_url
ALTER TABLE producto ADD imagen_url NVARCHAR(255) NULL;

--  campo mostrar_en_ventas
ALTER TABLE categoria_producto ADD mostrar_en_ventas BIT NOT NULL DEFAULT 1;

GO


-- Nuevos campos a la tabla estancia
ALTER TABLE estancia ADD
    esta_fuera BIT NOT NULL DEFAULT 0,
    hora_salida_temporal DATETIME NULL,
    hora_regreso_temporal DATETIME NULL,
    llaves_dejadas BIT NULL;  -- NULL: nunca se registró salida; 1: dejó; 0: no dejó

-- Tabla de parámetros globales del hotel
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'parametro_hotel')
BEGIN
    CREATE TABLE parametro_hotel (
        id_parametro INT PRIMARY KEY IDENTITY(1,1),
        clave NVARCHAR(100) UNIQUE NOT NULL,
        valor NVARCHAR(500) NOT NULL,
        descripcion NVARCHAR(200) NULL,
        fecha_actualizacion DATETIME DEFAULT GETDATE()
    );
END

-- Insertar el parámetro por defecto
IF NOT EXISTS (SELECT 1 FROM parametro_hotel WHERE clave = 'requerir_llaves_salida_temporal')
BEGIN
    INSERT INTO parametro_hotel (clave, valor, descripcion)
    VALUES ('requerir_llaves_salida_temporal', 'false',
            'Si es true, al registrar una salida temporal se mostrará alerta si el huésped no deja las llaves');
END
GO

-- 1. Agregar columnas a la tabla producto
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('producto') AND name = 'es_amenidad')
    ALTER TABLE producto ADD es_amenidad BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('producto') AND name = 'es_vendible_en_tienda')
    ALTER TABLE producto ADD es_vendible_en_tienda BIT NOT NULL DEFAULT 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('producto') AND name = 'stock_por_habitacion')
    ALTER TABLE producto ADD stock_por_habitacion INT NULL;
GO

-- 2. Tabla stock_habitacion (inventario actual por habitación)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'stock_habitacion')
BEGIN
    CREATE TABLE stock_habitacion (
        id_stock INT PRIMARY KEY IDENTITY(1,1),
        id_habitacion INT NOT NULL,
        id_producto INT NOT NULL,
        cantidad_actual INT NOT NULL,
        CONSTRAINT uq_stock_habitacion UNIQUE (id_habitacion, id_producto),
        CONSTRAINT fk_stock_habitacion FOREIGN KEY (id_habitacion) REFERENCES habitacion(id_habitacion),
        CONSTRAINT fk_stock_producto FOREIGN KEY (id_producto) REFERENCES producto(id_producto)
    );
END
GO


-- 3. Insertar productos de amenidad (jabones, champú, toallas)
-- Primero, insertar una categoría "Amenidades" si no existe
IF NOT EXISTS (SELECT 1 FROM categoria_producto WHERE nombre = 'Amenidades')
    INSERT INTO categoria_producto (nombre, descripcion) VALUES ('Amenidades', 'Artículos de cortesía en la habitación');
GO

-- Insertar productos amenidad (solo si no existen por nombre)
IF NOT EXISTS (SELECT 1 FROM producto WHERE nombre = 'Jabón de cortesía')
    INSERT INTO producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock, es_amenidad, es_vendible_en_tienda, stock_por_habitacion)
    VALUES ('Jabón de cortesía', 'Jabón pequeño para huéspedes', 0, '10', (SELECT id_categoria FROM categoria_producto WHERE nombre = 'Amenidades'), 500, 1, 0, 2);
GO

IF NOT EXISTS (SELECT 1 FROM producto WHERE nombre = 'Champú de cortesía')
    INSERT INTO producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock, es_amenidad, es_vendible_en_tienda, stock_por_habitacion)
    VALUES ('Champú de cortesía', 'Sobre de champú', 0, '10', (SELECT id_categoria FROM categoria_producto WHERE nombre = 'Amenidades'), 500, 1, 0, 2);
GO

IF NOT EXISTS (SELECT 1 FROM producto WHERE nombre = 'Toalla mediana')
    INSERT INTO producto (nombre, descripcion, precio_unitario, id_afectacion_igv, id_categoria, stock, es_amenidad, es_vendible_en_tienda, stock_por_habitacion)
    VALUES ('Toalla mediana', 'Toalla de baño', 0, '10', (SELECT id_categoria FROM categoria_producto WHERE nombre = 'Amenidades'), 100, 1, 0, 2);
GO

-- Tabla reserva_corporativa (agrupa múltiples habitaciones)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'reserva_corporativa')
BEGIN
    CREATE TABLE reserva_corporativa (
        id_reserva_corporativa INT PRIMARY KEY IDENTITY(1,1),
        id_cliente_empresa INT NOT NULL,
        fecha_inicio DATE NOT NULL,
        fecha_fin DATE NOT NULL,
        numero_habitaciones INT NOT NULL,
        estado NVARCHAR(20) NOT NULL DEFAULT 'Pendiente', -- Pendiente, Confirmada, CheckInParcial, Finalizada
        observaciones NVARCHAR(300) NULL,
        fecha_registro DATETIME DEFAULT GETDATE(),
        CONSTRAINT fk_reserva_corporativa_cliente FOREIGN KEY (id_cliente_empresa) REFERENCES cliente(id_cliente)
    );
END
GO

-- Agregar columna a estancia para vincular a reserva corporativa
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('estancia') AND name = 'id_reserva_corporativa')
BEGIN
    ALTER TABLE estancia ADD id_reserva_corporativa INT NULL;
    ALTER TABLE estancia ADD CONSTRAINT fk_estancia_reserva_corporativa
        FOREIGN KEY (id_reserva_corporativa) REFERENCES reserva_corporativa(id_reserva_corporativa);
END
GO

-- Índice para búsquedas rápidas
CREATE INDEX ix_estancia_reserva_corporativa ON estancia(id_reserva_corporativa) WHERE id_reserva_corporativa IS NOT NULL;
GO

-- Tabla: incidente (daños, manchas, roturas)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'incidente')
BEGIN
    CREATE TABLE incidente (
        id_incidente INT PRIMARY KEY IDENTITY(1,1),
        id_estancia INT NULL,
        id_habitacion INT NOT NULL,
        tipo NVARCHAR(50) NOT NULL,              -- 'daño', 'mancha', 'rotura'
        descripcion NVARCHAR(500) NOT NULL,
        costo_estimado DECIMAL(10,2) NULL,
        cobrado_al_cliente BIT NOT NULL DEFAULT 0,
        resuelto BIT NOT NULL DEFAULT 0,
        fecha_registro DATETIME DEFAULT GETDATE(),
        reportado_por INT NULL,
        CONSTRAINT fk_incidente_estancia FOREIGN KEY (id_estancia) REFERENCES estancia(id_estancia),
        CONSTRAINT fk_incidente_habitacion FOREIGN KEY (id_habitacion) REFERENCES habitacion(id_habitacion),
        CONSTRAINT fk_incidente_usuario FOREIGN KEY (reportado_por) REFERENCES usuario(id_usuario)
    );
END
GO

CREATE INDEX ix_incidente_habitacion_fecha ON incidente(id_habitacion, fecha_registro DESC);
CREATE INDEX ix_incidente_estancia ON incidente(id_estancia);

-- Tabla: objeto_perdido
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'objeto_perdido')
BEGIN
    CREATE TABLE objeto_perdido (
        id_objeto INT PRIMARY KEY IDENTITY(1,1),
        id_habitacion INT NULL,
        id_estancia INT NULL,
        descripcion NVARCHAR(200) NOT NULL,
        fecha_hallazgo DATETIME DEFAULT GETDATE(),
        estado NVARCHAR(20) NOT NULL DEFAULT 'pendiente',  -- 'pendiente', 'entregado', 'desechado'
        entregado_a NVARCHAR(100) NULL,
        fecha_entregado DATETIME NULL,
        CONSTRAINT fk_objeto_habitacion FOREIGN KEY (id_habitacion) REFERENCES habitacion(id_habitacion),
        CONSTRAINT fk_objeto_estancia FOREIGN KEY (id_estancia) REFERENCES estancia(id_estancia)
    );
END
GO

CREATE INDEX ix_objeto_estado ON objeto_perdido(estado);
CREATE INDEX ix_objeto_fecha ON objeto_perdido(fecha_hallazgo DESC);

PRINT 'Base de datos HotelDB creada con éxito.';
