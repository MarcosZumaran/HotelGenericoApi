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

### GET /reserva-corporativa/{id}
Auth: Si

### POST /reserva-corporativa
Auth: Si

### PUT /reserva-corporativa/{id}
Auth: Si

### DELETE /reserva-corporativa/{id}
Auth: Si

### POST /reserva-corporativa/{id}/finalizar
Auth: Si

---

## Comprobantes

### GET /comprobante
Auth: Si

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

### POST /comprobante/{id}/enviar
Auth: Si

---

## PDF

### GET /pdf/Comprobante/{id}
Auth: Si

Response 200: `application/pdf`

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

### GET /reporte/estado-habitaciones
Auth: Si

### GET /reporte/ocupacion-diaria
Auth: Si

### GET /reporte/top-productos
Auth: Si

---

## Catalogos (CRUD estandar)

### GET /cat-estado-habitacion
### GET /cat-estado-habitacion/{id}
### POST /cat-estado-habitacion
### PUT /cat-estado-habitacion/{id}
### DELETE /cat-estado-habitacion/{id}

### GET /cat-rol-usuario
### GET /cat-rol-usuario/{id}

### GET /cat-metodo-pago
### GET /cat-metodo-pago/{codigo}

### GET /cat-tipo-documento
### GET /cat-tipo-documento/{codigo}

### GET /cat-tipo-comprobante
### GET /cat-tipo-comprobante/{codigo}

### GET /cat-afectacion-igv
### GET /cat-afectacion-igv/{codigo}

### GET /cat-estado-sunat
### GET /cat-estado-sunat/{codigo}

### GET /tipos-habitacion
### GET /tipos-habitacion/{id}

### GET /categoria-producto

---

## Configuracion

### GET /configuracion-hotel
Auth: Si

---

## Setup (solo desarrollo)

### GET /setup/estado
Auth: No

### POST /setup/crear-admin
Auth: No

### POST /setup/crear-usuarios-defecto
Auth: No

---

## Backup

### POST /backup/full
Auth: Si

### POST /backup/differential
Auth: Si

### POST /backup/log
Auth: Si

### GET /backup/history
Auth: Si

### GET /backup/download/{fileName}
Auth: Si

---

## Health Check

### GET /health
Auth: No

Response 200: `Healthy`

---

## Notas

- **TipoDocumento** acepta tanto codigos ("1", "6", "7", "0") como nombres ("DNI", "RUC", "Pasaporte", "Otros")
- **MetodoPago:** "005"=Efectivo, "006"=Tarjeta, "008"=Yape/Plin, "001"=Deposito, "999"=Otros
- **Usuario predefinido:** admin / Admin123! (Rol: Administrador)
- **Usuarios predefinidos:** recepcion / Recepcion123! (Rol: Recepcionista), limpieza / Limpieza123! (Rol: Limpieza)
- Errores con formato ProblemDetails (RFC 7807)
