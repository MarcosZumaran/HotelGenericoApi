# Hotel Generico API - Documentacion de Endpoints

**Base URL:** `http://localhost:5000/api/v1`
**Auth:** Cookie `auth_token` (HttpOnly, se obtiene via login)
**Format:** JSON siempre

---

## Autenticacion

### POST /usuario/login
Auth: No

Request:
```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

Response 200 (cookie `auth_token` seteada automaticamente):
```json
{
  "idUsuario": 1,
  "username": "admin",
  "idRol": 1,
  "nombreRol": "Administrador",
  "estaActivo": true,
  "fechaCreacion": "2026-06-02T16:24:32.1554678"
}
```

Response 401: (sin body)

### POST /usuario/logout
Auth: Si

Response 200:
```json
{
  "message": "Sesion cerrada"
}
```

### GET /usuario/me
Auth: Si

Response 200:
```json
{
  "idUsuario": 1,
  "username": "admin",
  "idRol": 1,
  "nombreRol": "Administrador",
  "estaActivo": true,
  "fechaCreacion": "2026-06-02T16:24:32.1554678"
}
```

### GET /usuario
Auth: Si

Response 200:
```json
[
  {
    "idUsuario": 1,
    "username": "admin",
    "idRol": 1,
    "nombreRol": "Administrador",
    "estaActivo": true,
    "fechaCreacion": "2026-06-02T16:24:32.1554678"
  }
]
```

### GET /usuario/{id}
Auth: Si

Response 200 (mismo shape que arriba)

### POST /usuario
Auth: Si

Request:
```json
{
  "username": "nuevo",
  "password": "Pass123!",
  "idRol": 2
}
```

### PUT /usuario/{id}
Auth: Si

### DELETE /usuario/{id}
Auth: Si

Response 204: Sin contenido

---

## Reservas

### GET /reserva
Auth: Si

Response 200:
```json
[
  {
    "idReserva": 1,
    "idHabitacion": 1,
    "numeroHabitacion": "101",
    "clienteNombre": "Carlos Perez",
    "fechaEntradaPrevista": "2026-06-10T00:00:00",
    "fechaSalidaPrevista": "2026-06-12T00:00:00",
    "montoTotal": 500.0,
    "estado": "Confirmada",
    "documentoCliente": "12345678",
    "observaciones": null,
    "esNoShow": false
  }
]
```

---

## Estancia (Checkin / Checkout / Consumos / Traslados)

### GET /estancia
Auth: Si

Response 200:
```json
[
  {
    "idEstancia": 1,
    "idHabitacion": 1,
    "numeroHabitacion": "101",
    "idClienteTitular": 5,
    "clienteNombreCompleto": "Carlos Perez",
    "fechaCheckin": "2026-06-02T00:00:00",
    "fechaCheckoutPrevista": "2026-06-05T00:00:00",
    "fechaCheckoutReal": null,
    "montoTotal": 500.0,
    "estado": "Activa",
    "createdAt": "2026-06-02T00:00:00",
    "estaFuera": false,
    "horaSalidaTemporal": null,
    "horaRegresoTemporal": null,
    "llavesDejadas": null
  }
]
```

### GET /estancia/{id}
Auth: Si

Response 200: Mismo shape que arriba, un solo objeto.
Response 404: `{"type":"...","title":"Not Found","status":404}`

### POST /estancia/checkin
Auth: Si

Request:
```json
{
  "idHabitacion": 1,
  "fechaCheckoutPrevista": "2026-06-05T00:00:00",
  "tipoDocumento": "DNI",
  "documento": "12345678",
  "nombres": "Carlos",
  "apellidos": "Perez",
  "guardarCliente": true
}
```

Response 201: Objeto `Estancia` completo (con navegaciones).

### POST /estancia/{id}/checkout
Auth: Si

Response 200:
```json
{
  "totalHabitacion": 150.0,
  "totalConsumos": 25.5,
  "totalFinal": 175.5,
  "comprobanteId": 1
}
```

### POST /estancia/{id}/salida-temporal
Auth: Si

Request:
```json
{
  "llavesDejadas": true
}
```

Response 200:
```json
{
  "message": "Salida temporal registrada"
}
```

### POST /estancia/{id}/regreso
Auth: Si

Response 200:
```json
{
  "message": "Regreso registrado"
}
```

### POST /estancia/{id}/huespedes
Auth: Si

Request:
```json
{
  "tipoDocumento": "DNI",
  "documento": "87654321",
  "nombres": "Maria",
  "apellidos": "Lopez",
  "telefono": null
}
```

Response 200:
```json
{
  "idHuesped": 1,
  "message": "Huesped agregado"
}
```

### POST /estancia/{idEstancia}/consumo
Auth: Si

Request (body = `ItemEstancia`):
```json
{
  "idProducto": 1,
  "cantidad": 2,
  "precioUnitario": 2.5
}
```

Response 200: Sin contenido

### GET /estancia/{id}/consumos
Auth: Si

Response 200:
```json
[
  {
    "idItem": 1,
    "idProducto": 1,
    "nombreProducto": "Agua Mineral 500ml",
    "cantidad": 2,
    "precioUnitario": 2.5,
    "subtotal": 5.0,
    "fechaRegistro": "2026-06-02T00:00:00"
  }
]
```

### PUT /estancia/{id}/consumo/{idItem}
Auth: Si

Request:
```json
{
  "cantidad": 3
}
```

Response 200: Sin contenido
Response 404: Sin contenido

### DELETE /estancia/{id}/consumo/{idItem}
Auth: Si

Response 200: Sin contenido
Response 404: Sin contenido

### POST /estancia/{id}/trasladar
Auth: Si

Request:
```json
{
  "nuevaHabitacionId": 3,
  "motivo": "Cliente solicito cambio de habitacion"
}
```

Response 200:
```json
{
  "idEstancia": 1,
  "habitacionOrigenId": 1,
  "habitacionOrigenNumero": "101",
  "habitacionDestinoId": 3,
  "habitacionDestinoNumero": "103",
  "montoAnterior": 500.0,
  "montoNuevo": 700.0,
  "ajuste": 200.0,
  "motivo": "Cliente solicito cambio de habitacion"
}
```

### POST /estancia/reserva
Auth: Si

Request:
```json
{
  "idHabitacion": 1,
  "fechaEntradaPrevista": "2026-06-10T00:00:00",
  "fechaSalidaPrevista": "2026-06-12T00:00:00",
  "tipoDocumento": "DNI",
  "documento": "12345678",
  "nombres": "Carlos",
  "apellidos": "Perez",
  "guardarCliente": true
}
```

Response 201: Objeto `Reserva` completo (con navegaciones).

### PUT /estancia/reserva/{id}/cancelar
Auth: Si

Response 200: Sin contenido
Response 404: Sin contenido

### GET /estancia/reservas/{idHabitacion}
Auth: Si

Response 200: Lista de `ReservaResponseDto`.

---

## Habitaciones

### GET /habitacion
Auth: Si

Response 200:
```json
[
  {
    "idHabitacion": 1,
    "numeroHabitacion": "101",
    "piso": 1,
    "descripcion": null,
    "idTipo": 1,
    "nombreTipo": "Matrimonial",
    "precioNoche": 50.0,
    "idEstado": 1,
    "nombreEstado": "Disponible",
    "fechaUltimoCambio": "2026-06-02T10:11:45.2120028",
    "usuarioCambio": null,
    "caracteristicas": null,
    "amenidades": null
  }
]
```

### GET /habitacion/{id}
Auth: Si

### POST /habitacion
Auth: Si

### PUT /habitacion/{id}
Auth: Si

### DELETE /habitacion/{id}
Auth: Si

### GET /habitacion/disponibles
Auth: Si

Response 200: Lista de `HabitacionResponseDto` filtrada.

### GET /habitacion/estado-actual
Auth: Si

### PATCH /habitacion/{id}
Auth: Si

### PATCH /habitacion/{idHabitacion}/estado
Auth: Si

Request:
```json
{
  "idEstado": 3,
  "usuarioCambio": 1
}
```

### GET /habitacion/{id}/amenidades
Auth: Si

### PUT /habitacion/{id}/amenidades
Auth: Si

### GET /habitacion/{id}/caracteristicas
Auth: Si

### PUT /habitacion/{id}/caracteristicas
Auth: Si

---

## Clientes

### GET /cliente
Auth: Si

Response 200:
```json
[
  {
    "idCliente": 5,
    "tipoDocumento": "1",
    "documento": "12345678",
    "nombres": "Carlos",
    "apellidos": "Perez",
    "nacionalidad": "PERUANA",
    "fechaNacimiento": null,
    "telefono": null,
    "email": null,
    "direccion": null,
    "fechaRegistro": "2026-06-02T00:00:00",
    "fechaVerificacionReniec": null
  }
]
```

### GET /cliente/{id}
Auth: Si

### GET /cliente/documento/{tipo}/{documento}
Auth: Si

### POST /cliente
Auth: Si

Request:
```json
{
  "tipoDocumento": "1",
  "documento": "12345678",
  "nombres": "Carlos",
  "apellidos": "Perez",
  "nacionalidad": "PERUANA"
}
```

### PUT /cliente/{id}
Auth: Si

### DELETE /cliente/{id}
Auth: Si

### GET /cliente/reniec/{dni}
Auth: Si (rate limited)

---

## Productos

### GET /producto
Auth: Si

Response 200:
```json
[
  {
    "idProducto": 1,
    "codigoSunat": null,
    "nombre": "Agua Mineral 500ml",
    "descripcion": "Agua sin gas",
    "precioUnitario": 2.5,
    "idAfectacionIgv": "10",
    "nombreAfectacionIgv": "Gravado - Operacion Onerosa",
    "stock": 100,
    "stockMinimo": 5,
    "unidadMedida": "NIU",
    "createdAt": "2026-06-02T00:00:00",
    "imagenUrl": null,
    "esAmenidad": false,
    "esVendibleEnTienda": true,
    "stockPorHabitacion": null
  }
]
```

### GET /producto/{id}
Auth: Si

### POST /producto
Auth: Si

### PUT /producto/{id}
Auth: Si

### DELETE /producto/{id}
Auth: Si

### POST /producto/{id}/entrada-stock
Auth: Si

---

## Ventas

### GET /venta
Auth: Si

Response 200:
```json
[
  {
    "idVenta": 1,
    "idCliente": null,
    "clienteNombre": null,
    "fechaVenta": "2026-06-02T00:00:00",
    "total": 15.0,
    "metodoPago": "005",
    "items": [
      {
        "idItem": 1,
        "idProducto": 1,
        "nombreProducto": "Agua Mineral 500ml",
        "cantidad": 2,
        "precioUnitario": 2.5,
        "subtotal": 5.0
      }
    ]
  }
]
```

### GET /venta/{id}
Auth: Si

### POST /venta
Auth: Si

### DELETE /venta/{id}
Auth: Si

---

## Amenidades (Stock por Habitacion)

### GET /amenidad/habitacion/{idHabitacion}
Auth: Si

### POST /amenidad/habitacion/{idHabitacion}/consumir
Auth: Si

### POST /amenidad/habitacion/{idHabitacion}/reponer
Auth: Si

### POST /amenidad/habitacion/{idHabitacion}/reponer-todo
Auth: Si

---

## Incidentes y Objetos Perdidos

### GET /incidente/incidentes
Auth: Si

Response 200:
```json
[
  {
    "idIncidente": 1,
    "idEstancia": null,
    "idHabitacion": 1,
    "numeroHabitacion": "101",
    "tipo": "Dano",
    "descripcion": "Rotura de lampara",
    "costoEstimado": 50.0,
    "cobradoAlCliente": false,
    "resuelto": false,
    "fechaRegistro": "2026-06-02T00:00:00",
    "reportadoPorNombre": "admin",
    "imagenUrl": null
  }
]
```

### GET /incidente/incidentes/{id}
Auth: Si

### GET /incidente/incidentes/habitacion/{idHabitacion}
Auth: Si

### POST /incidente/incidentes
Auth: Si

### PATCH /incidente/incidentes/{id}/resolver
Auth: Si

### PATCH /incidente/incidentes/{id}/cobrar
Auth: Si

### GET /incidente/objetos
Auth: Si

### GET /incidente/objetos/pendientes
Auth: Si

### GET /incidente/objetos/{id}
Auth: Si

### POST /incidente/objetos
Auth: Si

### PATCH /incidente/objetos/{id}/entregar
Auth: Si

### PATCH /incidente/objetos/{id}/desechar
Auth: Si

---

## Reservas Corporativas

### GET /reserva-corporativa
Auth: Si

Response 200:
```json
[
  {
    "idReservaCorporativa": 1,
    "nombreEmpresa": "Corp SAC",
    "contactoNombre": "Juan Perez",
    "contactoTelefono": "999111222",
    "contactoEmail": "juan@corp.com",
    "fechaLlegada": "2026-06-10T00:00:00",
    "fechaSalida": "2026-06-15T00:00:00",
    "montoTotal": 2500.0,
    "observaciones": null,
    "createdAt": "2026-06-02T00:00:00",
    "estado": "Activa",
    "reservas": [
      { "idReserva": 1, "idHabitacion": 1, "numeroHabitacion": "101", "estado": "Confirmada" }
    ]
  }
]
```

### GET /reserva-corporativa/{id}
Auth: Si

Response 200: Mismo shape que arriba, un solo objeto.

### POST /reserva-corporativa
Auth: Si

Request:
```json
{
  "nombreEmpresa": "Corp SAC",
  "contactoNombre": "Juan Perez",
  "contactoTelefono": "999111222",
  "contactoEmail": "juan@corp.com",
  "fechaLlegada": "2026-06-10",
  "fechaSalida": "2026-06-15",
  "habitaciones": [
    { "idHabitacion": 1, "precioPersonalizado": null },
    { "idHabitacion": 3, "precioPersonalizado": 60.0 }
  ],
  "observaciones": "Facturar a nombre de Corp SAC"
}
```

### PUT /reserva-corporativa/{id}
Auth: Si

### DELETE /reserva-corporativa/{id}
Auth: Si

Response 204: Sin contenido

### POST /reserva-corporativa/{id}/finalizar
Auth: Si

---

## Comprobantes

### GET /comprobante
Auth: Si

Query params: `?page=1&pageSize=10`

Response 200:
```json
[
  {
    "idComprobante": 1,
    "idEstancia": 1,
    "idVenta": null,
    "tipoComprobante": "03",
    "serie": "B001",
    "correlativo": 1,
    "fechaEmision": "2026-06-02T00:00:00",
    "montoTotal": 175.5,
    "igvMonto": 31.59,
    "clienteDocumentoTipo": "1",
    "clienteDocumentoNum": "12345678",
    "clienteNombre": "Carlos Perez",
    "metodoPago": "005",
    "idEstadoSunat": 1,
    "nombreEstadoSunat": "Pendiente",
    "fechaEnvio": null,
    "intentosEnvio": 0
  }
]
```

### GET /comprobante/{id}
Auth: Si

Response 200: Mismo shape que arriba, un solo objeto.

### POST /comprobante/{id}/enviar
Auth: Si

Request (body opcional):
```json
"usuarioSunat"
```

Response 200:
```json
{
  "message": "Comprobante enviado a SUNAT exitosamente"
}
```

---

## PDF

### GET /pdf/Comprobante/{id}
Auth: Si

Response 200: `application/pdf` (archivo descargable)

### GET /pdf/Venta/{idVenta}
Auth: Si

Response 200: `application/pdf`

### GET /pdf/Estancia/{idEstancia}
Auth: Si

Response 200: `application/pdf`

### GET /pdf/CierreCaja
Auth: Si

Response 200: `application/pdf`

---

## Reportes

### GET /reporte/cierre-caja
Auth: Si

Response 200:
```json
[
  {
    "fecha": "2026-06-02",
    "totalHabitaciones": 500.0,
    "totalConsumos": 25.5,
    "totalVentas": 15.0,
    "totalIngresos": 540.5,
    "cantidadComprobantes": 1,
    "cantidadEstancias": 1
  }
]
```

### GET /reporte/estado-habitaciones
Auth: Si

Response 200:
```json
[
  {
    "numeroHabitacion": "101",
    "tipoHabitacion": "Matrimonial",
    "estado": "Disponible",
    "precioNoche": 50.0,
    "fechaUltimoCambio": "2026-06-02T10:11:45.2120028"
  }
]
```

### GET /reporte/ocupacion-diaria
Auth: Si

Response 200:
```json
[
  {
    "fecha": "2026-06-02",
    "totalHabitaciones": 8,
    "ocupadas": 3,
    "disponibles": 5,
    "porcentajeOcupacion": 37.5
  }
]
```

### GET /reporte/top-productos
Auth: Si

Response 200:
```json
[
  {
    "idProducto": 1,
    "nombreProducto": "Agua Mineral 500ml",
    "totalVendido": 50,
    "totalIngresos": 125.0
  }
]
```

---

## Catalogos (CRUD estandar)

Las rutas se generan con el nombre del controlador (PascalCase, aunque el ruteo es case-insensitive).
Ejemplo: `CatEstadoHabitacion` o `cat-estado-habitacion` funcionan igual.

### CatEstadoHabitacion

**GET** (listar): `GET /cat-estado-habitacion`
Auth: Si

Response 200:
```json
[
  { "idEstado": 1, "nombre": "Disponible", "descripcion": "Lista para ser ocupada" },
  { "idEstado": 2, "nombre": "Ocupada", "descripcion": "Con huéspedes actualmente" },
  { "idEstado": 3, "nombre": "Limpieza", "descripcion": "En proceso de limpieza" },
  { "idEstado": 4, "nombre": "Mantenimiento", "descripcion": "Fuera de servicio" },
  { "idEstado": 5, "nombre": "En Reserva", "descripcion": "Habitación reservada para hoy, esperando check-in" }
]
```

**POST** (crear): `POST /cat-estado-habitacion`
Request:
```json
{
  "nombre": "Bloqueado",
  "descripcion": "No disponible por orden administrativa",
  "permiteCheckin": false,
  "permiteCheckout": false,
  "esEstadoFinal": false,
  "colorUi": "#FF0000"
}
```

**GET by ID**: `GET /cat-estado-habitacion/{id}`

**PUT** (actualizar): `PUT /cat-estado-habitacion/{id}`

**DELETE**: `DELETE /cat-estado-habitacion/{id}` (Response 204)

### CatRolUsuario

**GET** (listar): `GET /cat-rol-usuario`
Auth: Si

Response 200:
```json
[
  { "idRol": 1, "nombre": "Administrador" },
  { "idRol": 2, "nombre": "Recepcionista" },
  { "idRol": 3, "nombre": "Limpieza" }
]
```

**GET by ID**: `GET /cat-rol-usuario/{id}`

**POST**: `POST /cat-rol-usuario`
```json
{ "nombre": "Cajero" }
```

**PUT / DELETE**: Mismo patrón.

### CatMetodoPago

**GET** (listar): `GET /cat-metodo-pago`
Auth: Si

Response 200:
```json
[
  { "codigo": "001", "descripcion": "Depósito en cuenta" },
  { "codigo": "005", "descripcion": "Efectivo" },
  { "codigo": "006", "descripcion": "Tarjeta de Crédito / Débito" },
  { "codigo": "008", "descripcion": "Transferencia bancaria (Yape/Plin)" },
  { "codigo": "999", "descripcion": "Otros" }
]
```

**GET by codigo**: `GET /cat-metodo-pago/{codigo}`

### CatTipoDocumento

**GET** (listar): `GET /cat-tipo-documento`
Auth: Si

Response 200:
```json
[
  { "codigo": "0", "descripcion": "Otros" },
  { "codigo": "1", "descripcion": "DNI" },
  { "codigo": "6", "descripcion": "RUC" },
  { "codigo": "7", "descripcion": "Pasaporte" }
]
```

### CatTipoComprobante

**GET** (listar): `GET /cat-tipo-comprobante`
Auth: Si

Response 200:
```json
[
  { "codigo": "01", "descripcion": "Factura" },
  { "codigo": "03", "descripcion": "Boleta de Venta" }
]
```

### CatAfectacionIgv

**GET** (listar): `GET /cat-afectacion-igv`
Auth: Si

Response 200:
```json
[
  { "codigo": "10", "descripcion": "Gravado - Operación Onerosa" },
  { "codigo": "20", "descripcion": "Exonerado" },
  { "codigo": "30", "descripcion": "Inafecto" },
  { "codigo": "40", "descripcion": "Exportación" }
]
```

### CatEstadoSunat

**GET** (listar): `GET /cat-estado-sunat`
Auth: Si

Response 200:
```json
[
  { "codigo": 1, "descripcion": "Pendiente", "descripcionLarga": "El comprobante se generó pero no se ha enviado." },
  { "codigo": 2, "descripcion": "Enviado", "descripcionLarga": "El comprobante fue enviado y se espera respuesta de SUNAT." },
  { "codigo": 3, "descripcion": "Aceptado", "descripcionLarga": "El comprobante fue validado exitosamente por SUNAT." },
  { "codigo": 4, "descripcion": "Rechazado", "descripcionLarga": "El comprobante fue RECHAZADO. No tiene validez tributaria." },
  { "codigo": 5, "descripcion": "Observado", "descripcionLarga": "Aceptado con observaciones menores." },
  { "codigo": 6, "descripcion": "Anulado", "descripcionLarga": "El comprobante fue dado de baja." }
]
```

### TiposHabitacion

**GET** (listar): `GET /tipos-habitacion`
Auth: Si

Response 200:
```json
[
  { "idTipo": 1, "nombre": "Matrimonial", "capacidad": 2, "descripcion": "Habitación estándar para dos personas", "precioBase": 50.0 },
  { "idTipo": 2, "nombre": "Doble", "capacidad": 3, "descripcion": "Habitación con dos camas individuales", "precioBase": 70.0 },
  { "idTipo": 3, "nombre": "Suite", "capacidad": 4, "descripcion": "Suite con sala de estar independiente", "precioBase": 120.0 }
]
```

**GET by ID**: `GET /tipos-habitacion/{id}`

### CategoriaProducto

**GET** (listar): `GET /categoria-producto`
Auth: Si

Response 200:
```json
[
  { "idCategoria": 1, "nombre": "Bebidas", "descripcion": "Bebidas alcohólicas y no alcohólicas", "mostrarEnVentas": true, "productos": [] },
  { "idCategoria": 2, "nombre": "Snacks", "descripcion": "Snacks y piqueos", "mostrarEnVentas": true, "productos": [] },
  { "idCategoria": 3, "nombre": "Servicios", "descripcion": "Servicios adicionales", "mostrarEnVentas": true, "productos": [] },
  { "idCategoria": 4, "nombre": "Amenidades", "descripcion": "Artículos de cortesía en la habitación", "mostrarEnVentas": true, "productos": [] }
]
```

---

## Configuracion

### GET /configuracion-hotel
Auth: Si

Response 200:
```json
{
  "nombre": "Mi Hotel",
  "direccion": "Av. Principal 123",
  "telefono": "999-999-999",
  "ruc": "12345678901",
  "tasaIgvHotel": 18.0,
  "tasaIgvProductos": 18.0
}
```

---

## Setup (solo desarrollo)

### GET /setup/estado
Auth: No

Response 200:
```json
{
  "requiereInicializacion": false
}
```

### POST /setup/crear-admin
Auth: No

Crea el usuario administrador por defecto.
Response 200:
```json
{
  "message": "Administrador creado exitosamente"
}
```

### POST /setup/crear-usuarios-defecto
Auth: No

Crea usuarios: admin, recepcion, limpieza.
Response 200:
```json
{
  "message": "Usuarios por defecto creados/verificados exitosamente"
}
```

---

## Backup

### POST /backup/full
Auth: Si

Response 200:
```json
{
  "fileName": "Full_20260602_164500.bak",
  "filePath": "/backups/Full_20260602_164500.bak",
  "sizeBytes": 1048576,
  "tipo": "Full",
  "fechaCreacion": "2026-06-02T16:45:00"
}
```

### POST /backup/differential
Auth: Si

### POST /backup/log
Auth: Si

### GET /backup/history
Auth: Si

Response 200:
```json
[]
```

### GET /backup/download/{fileName}
Auth: Si

Response 200: `application/octet-stream`

---

## Health Check

### GET /health
Auth: No

Response 200: `Healthy`

---

## Notas

### Convencion de rutas
- Las URLs usan kebab-case en esta documentacion, pero el ruteo es case-insensitive.
- Ejemplo: `/api/v1/cat-estado-habitacion` ≡ `/api/v1/CatEstadoHabitacion`
- Los valores en JSON siempre usan PascalCase (propiedades C#).

### TipoDocumento
Acepta tanto codigos ("1", "6", "7", "0") como nombres ("DNI", "RUC", "Pasaporte", "Otros"). El API normaliza automaticamente.

### MetodoPago
- `"005"` = Efectivo
- `"006"` = Tarjeta de Credito/Debito
- `"008"` = Yape/Plin / Transferencia
- `"001"` = Deposito en cuenta
- `"999"` = Otros

### Usuarios predefinidos
| Usuario | Password | Rol |
|---------|----------|-----|
| admin | Admin123! | Administrador |
| recepcion | Recepcion123! | Recepcionista |
| limpieza | Limpieza123! | Limpieza |

### Errores
Todas las responses de error usan formato ProblemDetails (RFC 7807):
