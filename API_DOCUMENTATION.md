# Hotel Generico API - Documentacion de Endpoints

**Base URL:** `http://localhost:5054/api/v1`
**Auth:** Cookie `auth_token` (HttpOnly, se obtiene via login)
**Format:** JSON siempre (excepto PDFs que son `application/pdf`)
**Case:** Las URLs son case-insensitive (`/api/v1/Estancia` ≡ `/api/v1/estancia`)

---

## Autenticacion (Usuario)

### POST /usuario/login
Inicia sesion y devuelve una cookie `auth_token` (HttpOnly).

- Auth: No
- Request: `application/json`
```json
{
  "username": "admin",
  "password": "Admin123!"
}
```
- Response 200: Cookie `auth_token` seteada + body:
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
- Response 401: Sin body

### POST /usuario/logout
Cierra la sesion activa.

- Auth: Si
- Response 200:
```json
{ "message": "Sesion cerrada" }
```

### GET /usuario/me
Devuelve el usuario autenticado actual.

- Auth: Si
- Response 200:
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
Lista todos los usuarios registrados.

- Auth: Si
- Response 200: Array de usuarios (mismo shape que `/me`)
- Si no hay usuarios: `[]`

### GET /usuario/{id}
Obtiene un usuario por ID.

- Auth: Si
- Response 200: Objeto usuario
- Response 404: `ProblemDetails`

### POST /usuario
Crea un nuevo usuario.

- Auth: Si
- Request:
```json
{
  "username": "nuevo",
  "password": "Pass123!",
  "idRol": 2
}
```
- Response 201: Objeto usuario creado

### PUT /usuario/{id}
Actualiza un usuario existente.

- Auth: Si
- Request: Mismo shape que POST
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

### DELETE /usuario/{id}
Elimina un usuario.

- Auth: Si
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

---

## Reservas

### GET /reserva
Lista todas las reservas.

- Auth: Si
- Response 200:
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
- Si no hay reservas: `[]`

---

## Estancia (Checkin / Checkout / Consumos / Traslados)

### GET /estancia
Lista todas las estancias activas e historicas.

- Auth: Si
- Response 200:
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
- Si no hay estancias: `[]`

### GET /estancia/{id}
Obtiene una estancia por ID.

- Auth: Si
- Response 200: Objeto estancia (mismo shape que listado)
- Response 404: `ProblemDetails`

### POST /estancia/checkin
Registra la entrada (check-in) de un huesped a una habitacion. Si el cliente no existe, lo crea automaticamente.

- Auth: Si
- Request:
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
- Response 201: Objeto `Estancia` completo
- Response 400: `ProblemDetails` (habitacion no disponible, etc.)

### POST /estancia/{id}/checkout
Realiza el checkout de una estancia: calcula totales, genera comprobante y libera la habitacion.

- Auth: Si
- Response 200:
```json
{
  "totalHabitacion": 150.0,
  "totalConsumos": 25.5,
  "totalFinal": 175.5,
  "comprobanteId": 1
}
```
- Response 404: Estancia no encontrada

### POST /estancia/{id}/salida-temporal
Registra una salida temporal del huesped (ej: sale a pasear, deja llaves o no).

- Auth: Si
- Request:
```json
{ "llavesDejadas": true }
```
- Response 200:
```json
{ "message": "Salida temporal registrada" }
```
- Response 404: Estancia no encontrada

### POST /estancia/{id}/regreso
Registra el regreso del huesped despues de una salida temporal.

- Auth: Si
- Response 200:
```json
{ "message": "Regreso registrado" }
```
- Response 404: Estancia no encontrada

### POST /estancia/{id}/huespedes
Agrega un huesped adicional a una estancia existente.

- Auth: Si
- Request:
```json
{
  "tipoDocumento": "DNI",
  "documento": "87654321",
  "nombres": "Maria",
  "apellidos": "Lopez",
  "telefono": null
}
```
- Response 200:
```json
{
  "idHuesped": 1,
  "message": "Huesped agregado"
}
```
- Response 404: Estancia no encontrada

### POST /estancia/{idEstancia}/consumo
Agrega un consumo (producto/servicio) a la estancia.

- Auth: Si
- Request:
```json
{
  "idProducto": 1,
  "cantidad": 2,
  "precioUnitario": 2.5
}
```
- Response 200: Sin contenido
- Response 404: Estancia no encontrada

### GET /estancia/{id}/consumos
Obtiene la lista de consumos de una estancia.

- Auth: Si
- Response 200:
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
- Si no hay consumos: `[]`
- Response 404: Estancia no encontrada

### PUT /estancia/{id}/consumo/{idItem}
Actualiza la cantidad de un consumo existente.

- Auth: Si
- Request:
```json
{ "cantidad": 3 }
```
- Response 200: Sin contenido
- Response 404: Sin contenido

### DELETE /estancia/{id}/consumo/{idItem}
Elimina un consumo de la estancia.

- Auth: Si
- Response 200: Sin contenido
- Response 404: Sin contenido

### POST /estancia/{id}/trasladar
Traslada al huesped a otra habitacion (cambia de cuarto, recalcula tarifa).

- Auth: Si
- Request:
```json
{
  "nuevaHabitacionId": 3,
  "motivo": "Cliente solicito cambio de habitacion"
}
```
- Response 200:
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
- Response 400: `ProblemDetails` (habitacion destino no disponible, etc.)

### POST /estancia/reserva
Crea una reserva desde el modulo de estancia (check-in futuro). El cliente puede ser nuevo o existente.

- Auth: Si
- Request:
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
- Response 201: Objeto `Reserva` completo
- Response 400: `ProblemDetails`

### PUT /estancia/reserva/{id}/cancelar
Cancela una reserva existente.

- Auth: Si
- Response 200: Sin contenido
- Response 404: Sin contenido

### GET /estancia/reservas/{idHabitacion}
Obtiene las reservas asociadas a una habitacion especifica.

- Auth: Si
- Response 200: Array de `ReservaResponseDto`
- Si no hay reservas: `[]`
- Response 404: Habitacion no encontrada

---

## Habitaciones

### GET /habitacion
Obtiene todas las habitaciones registradas.

- Auth: Si
- Response 200:
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
- Si no hay habitaciones: `[]`

### GET /habitacion/{id}
Obtiene una habitacion por ID.

- Auth: Si
- Response 200: Objeto habitacion
- Response 404: `ProblemDetails`

### POST /habitacion
Crea una nueva habitacion.

- Auth: Si
- Request:
```json
{
  "numeroHabitacion": "301",
  "piso": 3,
  "idTipo": 1,
  "precioNoche": 80.0,
  "descripcion": "Habitacion en el tercer piso"
}
```
- Response 201: Objeto `Habitacion`

### PUT /habitacion/{id}
Actualiza los datos de una habitacion existente.

- Auth: Si
- Request: Mismo shape que POST
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

### DELETE /habitacion/{id}
Elimina una habitacion por su ID.

- Auth: Si
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

### GET /habitacion/disponibles
Obtiene las habitaciones disponibles en un rango de fechas.

- Auth: Si
- Query params: `?fechaEntrada=2026-06-10&fechaSalida=2026-06-12`
- Response 200: Array de `HabitacionResponseDto` filtrada
- Si no hay disponibles: `[]`

### GET /habitacion/estado-actual
Obtiene el estado actual de todas las habitaciones con datos en tiempo real.

- Auth: Si
- Response 200: Array con estado de cada habitacion

### PATCH /habitacion/{id}
Parchea una habitacion: cambia estado o actualiza datos segun el body.

- Auth: Si
- Request:
```json
{
  "idEstado": 3,
  "usuarioCambio": 1
}
```
- Response 200: Objeto `Habitacion` actualizado

### PATCH /habitacion/{idHabitacion}/estado
Cambia el estado de una habitacion validando transiciones permitidas.

- Auth: Si
- Request:
```json
{ "idEstado": 3, "usuarioCambio": 1 }
```
- Response 200: Objeto `Habitacion` actualizado
- Response 400: `ProblemDetails` (transicion no valida)

### GET /habitacion/{id}/amenidades
Obtiene las amenidades personalizadas de una habitacion.

- Auth: Si
- Response 200: Array de amenidades
- Response 404: `ProblemDetails`

### PUT /habitacion/{id}/amenidades
Actualiza las amenidades personalizadas de una habitacion.

- Auth: Si
- Request: Array de `{ idProducto, cantidad }`
- Response 200: Array actualizado

### GET /habitacion/{id}/caracteristicas
Obtiene las caracteristicas extra de una habitacion.

- Auth: Si
- Response 200: Array de caracteristicas

### PUT /habitacion/{id}/caracteristicas
Actualiza las caracteristicas extra de una habitacion.

- Auth: Si
- Response 200: Array actualizado

---

## Clientes

### GET /cliente
Obtiene todos los clientes registrados con paginacion.

- Auth: Si
- Query params: `?page=1&pageSize=10`
- Response 200:
```json
{
  "items": [
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
  ],
  "totalItems": 1,
  "page": 1,
  "pageSize": 10
}
```
- Si no hay clientes: `{ "items": [], "totalItems": 0, "page": 1, "pageSize": 10 }`

### GET /cliente/{id}
Obtiene un cliente por ID.

- Auth: Si
- Response 200: Objeto `ClienteResponseDto`
- Response 404: `ProblemDetails`

### GET /cliente/documento/{tipo}/{documento}
Obtiene un cliente por tipo y numero de documento.

- Auth: Si
- Parameters: `tipo` (1=DNI, 6=RUC, etc.), `documento` (numero)
- Response 200: Objeto `ClienteResponseDto`
- Response 404: `ProblemDetails`

### GET /cliente/buscar
Busca clientes por nombre, documento u otros criterios.

- Auth: Si
- Query param: `?termino=carlos`
- Response 200: Array de `ClienteResponseDto`
- Si no hay resultados: `[]`

### POST /cliente
Crea un nuevo cliente.

- Auth: Si
- Request:
```json
{
  "tipoDocumento": "1",
  "documento": "12345678",
  "nombres": "Carlos",
  "apellidos": "Perez",
  "nacionalidad": "PERUANA"
}
```
- Response 201: Objeto `ClienteResponseDto`
- Response 409: `ProblemDetails` (cliente duplicado)
- Response 400: `ProblemDetails`

### PUT /cliente/{id}
Actualiza los datos de un cliente existente.

- Auth: Si
- Request: Mismo shape que POST
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

### DELETE /cliente/{id}
Elimina un cliente por ID.

- Auth: Si
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

### GET /cliente/reniec/{dni}
Consulta los datos de un DNI en RENIEC (servicio VerificaPE).

- Auth: Si (rate limited)
- Parameters: `dni` (8 digitos)
- Response 200: Datos encontrados correctamente
- Response 502: Error al contactar con el servicio RENIEC

---

## Productos

### GET /producto
Obtiene todos los productos.

- Auth: Si
- Response 200:
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
- Si no hay productos: `[]`

### GET /producto/{id}
Obtiene un producto por ID.

- Auth: Si
- Response 200: Objeto `Producto`
- Response 404: `ProblemDetails`

### POST /producto
Crea un nuevo producto.

- Auth: Si
- Request:
```json
{
  "nombre": "Gaseosa Cola 500ml",
  "descripcion": "Gaseosa carbonatada",
  "precioUnitario": 3.0,
  "stock": 50,
  "stockMinimo": 5,
  "unidadMedida": "NIU",
  "idAfectacionIgv": "10",
  "esAmenidad": false,
  "esVendibleEnTienda": true
}
```
- Response 201: Objeto `Producto`

### PUT /producto/{id}
Actualiza un producto existente.

- Auth: Si
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

### DELETE /producto/{id}
Elimina un producto.

- Auth: Si
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

### POST /producto/{id}/entrada-stock
Registra una entrada de stock para un producto.

- Auth: Si
- Request:
```json
{
  "cantidad": 20,
  "motivo": "Reposicion de inventario"
}
```
- Response 200: Stock actualizado

---

## Ventas

### GET /venta
Obtiene todas las ventas registradas.

- Auth: Si
- Response 200:
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
- Si no hay ventas: `[]`

### GET /venta/{id}
Obtiene una venta por ID con items y producto.

- Auth: Si
- Response 200: Objeto `Venta` (mismo shape que listado)
- Response 404: `ProblemDetails`

### POST /venta
Registra una nueva venta con sus items.

- Auth: Si
- Request:
```json
{
  "items": [
    { "idProducto": 1, "cantidad": 2, "precioUnitario": 2.5 },
    { "idProducto": 2, "cantidad": 1, "precioUnitario": 5.0 }
  ],
  "metodoPago": "005",
  "idCliente": null
}
```
- Response 201: Objeto `Venta`

### DELETE /venta/{id}
Elimina una venta por su ID.

- Auth: Si
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

---

## Amenidades (Stock por Habitacion)

### GET /amenidad/habitacion/{idHabitacion}
Obtiene el stock actual de amenidades en una habitacion.

- Auth: Si
- Response 200: Array de amenidades con stock actual
- Si no hay amenidades en la habitacion: `[]`

### POST /amenidad/habitacion/{idHabitacion}/consumir
Consume una amenidad (reduce stock y opcionalmente lo cobra al huesped).

- Auth: Si
- Request:
```json
{
  "idProducto": 1,
  "cantidad": 1,
  "cobrarAlHuesped": true,
  "idEstancia": 1
}
```
- Response 200: Stock actualizado

### POST /amenidad/habitacion/{idHabitacion}/reponer
Repone una amenidad especifica en una habitacion (usado por limpieza o reposicion manual).

- Auth: Si
- Request:
```json
{ "idProducto": 1, "cantidad": 5 }
```
- Response 200: Stock actualizado

### POST /amenidad/habitacion/{idHabitacion}/reponer-todo
Repone todas las amenidades de una habitacion a su cantidad base (ejecutado tras limpieza).

- Auth: Si
- Response 200: Amenidades repuestas

---

## Incidentes y Objetos Perdidos

### GET /incidente/incidentes
Obtiene todos los incidentes registrados.

- Auth: Si
- Response 200:
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
- Si no hay incidentes: `[]`

### GET /incidente/incidentes/{id}
Obtiene un incidente por ID.

- Auth: Si
- Response 200: Objeto incidente
- Response 404: `ProblemDetails`

### GET /incidente/incidentes/habitacion/{idHabitacion}
Obtiene los incidentes de una habitacion especifica.

- Auth: Si
- Response 200: Array de incidentes
- Si no hay: `[]`

### POST /incidente/incidentes
Registra un nuevo incidente.

- Auth: Si
- Request:
```json
{
  "idHabitacion": 1,
  "tipo": "Dano",
  "descripcion": "Rotura de lampara",
  "costoEstimado": 50.0
}
```
- Response 201: Objeto incidente

### PATCH /incidente/incidentes/{id}/resolver
Marca un incidente como resuelto.

- Auth: Si
- Response 200: Incidente actualizado
- Response 404: `ProblemDetails`

### PATCH /incidente/incidentes/{id}/cobrar
Marca el costo del incidente como cobrado al cliente.

- Auth: Si
- Response 200: Incidente actualizado
- Response 404: `ProblemDetails`

### GET /incidente/objetos
Obtiene todos los objetos perdidos registrados.

- Auth: Si
- Response 200: Array de objetos perdidos
- Si no hay: `[]`

### GET /incidente/objetos/pendientes
Obtiene solo los objetos perdidos aun no entregados.

- Auth: Si
- Response 200: Array filtrado
- Si no hay pendientes: `[]`

### GET /incidente/objetos/{id}
Obtiene un objeto perdido por ID.

- Auth: Si
- Response 200: Objeto `ObjetoPerdido`
- Response 404: `ProblemDetails`

### POST /incidente/objetos
Registra un nuevo objeto perdido.

- Auth: Si
- Request:
```json
{
  "descripcion": "Celular Samsung Galaxy",
  "lugarEncontrado": "Habitacion 101",
  "entregadoPor": "Recepcionista Juan"
}
```
- Response 201: Objeto creado

### PATCH /incidente/objetos/{id}/entregar
Marca un objeto como entregado al dueno.

- Auth: Si
- Response 200: Objeto actualizado
- Response 404: `ProblemDetails`

### PATCH /incidente/objetos/{id}/desechar
Marca un objeto como desechado.

- Auth: Si
- Response 200: Objeto actualizado
- Response 404: `ProblemDetails`

---

## Reservas Corporativas

### GET /reserva-corporativa
Obtiene todas las reservas corporativas.

- Auth: Si
- Response 200:
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
- Si no hay: `[]`

### GET /reserva-corporativa/{id}
Obtiene una reserva corporativa por ID.

- Auth: Si
- Response 200: Mismo shape que arriba
- Response 404: `ProblemDetails`

### POST /reserva-corporativa
Crea una nueva reserva corporativa con multiples habitaciones.

- Auth: Si
- Request:
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
- Response 201: Objeto `ReservaCorporativa` creado

### PUT /reserva-corporativa/{id}
Actualiza una reserva corporativa.

- Auth: Si
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

### DELETE /reserva-corporativa/{id}
Elimina una reserva corporativa.

- Auth: Si
- Response 204: Sin contenido
- Response 404: `ProblemDetails`

### POST /reserva-corporativa/{id}/finalizar
Finaliza (checkout) una reserva corporativa y genera comprobantes para las habitaciones check-in.

- Auth: Si
- Response 200: Reserva finalizada

---

## Comprobantes

### GET /comprobante
Obtiene todos los comprobantes con paginacion.

- Auth: Si
- Query params: `?page=1&pageSize=10`
- Response 200:
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
- Si no hay comprobantes: `[]`

### GET /comprobante/{id}
Obtiene un comprobante por ID.

- Auth: Si
- Response 200: Objeto comprobante
- Response 404: `ProblemDetails`

### POST /comprobante/{id}/enviar
Envia un comprobante a SUNAT para su validacion. El body opcional contiene el usuario que autoriza el envio.

- Auth: Si
- Request (body opcional):
```
"usuarioSunat"
```
- Response 200:
```json
{ "message": "Comprobante enviado a SUNAT exitosamente" }
```
- Response 404: Comprobante no encontrado

---

## PDF

### GET /pdf/Comprobante/{id}
Genera PDF del comprobante (factura/boleta).

- Auth: Si
- Response 200: `application/pdf` (archivo descargable)
- Response 404: Comprobante no encontrado

### GET /pdf/Venta/{idVenta}
Genera PDF de una venta (boleta/factura de tienda).

- Auth: Si
- Response 200: `application/pdf`
- Response 404: Venta no encontrada

### GET /pdf/Estancia/{idEstancia}
Genera PDF de la cuenta de una estancia (checkout detallado).

- Auth: Si
- Response 200: `application/pdf`
- Response 404: Estancia no encontrada

### GET /pdf/CierreCaja
Genera PDF del reporte de cierre de caja del dia actual.

- Auth: Si
- Response 200: `application/pdf`

---

## Reportes

### GET /reporte/cierre-caja
Obtiene el cierre de caja diario con detalle de ingresos y egresos.

- Auth: Si
- Response 200:
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
- Si no hay movimientos: `[]`

### GET /reporte/estado-habitaciones
Obtiene el estado actual de todas las habitaciones.

- Auth: Si
- Response 200:
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
- Siempre devuelve todas las habitaciones (8 en seed data)

### GET /reporte/ocupacion-diaria
Obtiene el reporte de ocupacion diaria.

- Auth: Si
- Response 200:
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
- Si no hay datos: `[]`

### GET /reporte/top-productos
Obtiene el top de productos mas vendidos.

- Auth: Si
- Response 200:
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
- Si no hay ventas: `[]`

---

## Catalogos (CRUD estandar)

Los catalogos comparten patron CRUD basico. Las rutas usan el nombre del controlador (PascalCase), pero el ruteo es case-insensitive.

### CatEstadoHabitacion

**GET /cat-estado-habitacion** (listar) - Auth: Si
> Obtiene todos los estados de habitacion disponibles.

```json
[
  { "idEstado": 1, "nombre": "Disponible", "descripcion": "Lista para ser ocupada" },
  { "idEstado": 2, "nombre": "Ocupada", "descripcion": "Con huéspedes actualmente" },
  { "idEstado": 3, "nombre": "Limpieza", "descripcion": "En proceso de limpieza" },
  { "idEstado": 4, "nombre": "Mantenimiento", "descripcion": "Fuera de servicio" },
  { "idEstado": 5, "nombre": "En Reserva", "descripcion": "Habitación reservada para hoy, esperando check-in" }
]
```

**GET /cat-estado-habitacion/{id}** - Auth: Si
> Obtiene un estado por ID.
- Response 200: Objeto estado
- Response 404: `ProblemDetails`

**POST /cat-estado-habitacion** (crear) - Auth: Si
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
- Response 200: Objeto creado

**PUT /cat-estado-habitacion/{id}** - Auth: Si
- Response 204: Sin contenido

**DELETE /cat-estado-habitacion/{id}** - Auth: Si
- Response 204: Sin contenido

### CatRolUsuario

**GET /cat-rol-usuario** (listar) - Auth: Si
> Obtiene todos los roles de usuario.

```json
[
  { "idRol": 1, "nombre": "Administrador" },
  { "idRol": 2, "nombre": "Recepcionista" },
  { "idRol": 3, "nombre": "Limpieza" }
]
```

**GET /cat-rol-usuario/{id}** - Auth: Si
- Response 200: Objeto rol
- Response 404: `ProblemDetails`

**POST /cat-rol-usuario** - Auth: Si
```json
{ "nombre": "Cajero" }
```

**PUT /cat-rol-usuario/{id}** - Auth: Si
- Response 204: Sin contenido

**DELETE /cat-rol-usuario/{id}** - Auth: Si
- Response 204: Sin contenido

### CatMetodoPago

**GET /cat-metodo-pago** (listar) - Auth: Si
> Obtiene todos los metodos de pago disponibles.

```json
[
  { "codigo": "001", "descripcion": "Depósito en cuenta" },
  { "codigo": "005", "descripcion": "Efectivo" },
  { "codigo": "006", "descripcion": "Tarjeta de Crédito / Débito" },
  { "codigo": "008", "descripcion": "Transferencia bancaria (Yape/Plin)" },
  { "codigo": "999", "descripcion": "Otros" }
]
```

**GET /cat-metodo-pago/{codigo}** - Auth: Si
- Response 200: Objeto metodo pago
- Response 404: `ProblemDetails`

**POST /cat-metodo-pago** - Auth: Si
```json
{ "codigo": "007", "descripcion": "Cheque" }
```

**PUT /cat-metodo-pago/{codigo}** - Auth: Si
- Response 204: Sin contenido

**DELETE /cat-metodo-pago/{codigo}** - Auth: Si
- Response 204: Sin contenido

### CatTipoDocumento

**GET /cat-tipo-documento** (listar) - Auth: Si
> Obtiene todos los tipos de documento de identidad.

```json
[
  { "codigo": "0", "descripcion": "Otros" },
  { "codigo": "1", "descripcion": "DNI" },
  { "codigo": "6", "descripcion": "RUC" },
  { "codigo": "7", "descripcion": "Pasaporte" }
]
```

**GET /cat-tipo-documento/{codigo}** - Auth: Si
- Response 200: Objeto tipo documento
- Response 404: `ProblemDetails`

**POST /cat-tipo-documento** - Auth: Si
```json
{ "codigo": "4", "descripcion": "Carnet de Extranjeria" }
```

### CatTipoComprobante

**GET /cat-tipo-comprobante** (listar) - Auth: Si
> Obtiene los tipos de comprobante fiscal.

```json
[
  { "codigo": "01", "descripcion": "Factura" },
  { "codigo": "03", "descripcion": "Boleta de Venta" }
]
```

**GET /cat-tipo-comprobante/{codigo}** - Auth: Si
- Response 200: Objeto
- Response 404: `ProblemDetails`

### CatAfectacionIgv

**GET /cat-afectacion-igv** (listar) - Auth: Si
> Obtiene los tipos de afectacion IGV para productos.

```json
[
  { "codigo": "10", "descripcion": "Gravado - Operación Onerosa" },
  { "codigo": "20", "descripcion": "Exonerado" },
  { "codigo": "30", "descripcion": "Inafecto" },
  { "codigo": "40", "descripcion": "Exportación" }
]
```

**GET /cat-afectacion-igv/{codigo}** - Auth: Si
- Response 200: Objeto
- Response 404: `ProblemDetails`

### CatEstadoSunat

**GET /cat-estado-sunat** (listar) - Auth: Si
> Obtiene los estados posibles de envio SUNAT.

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

**GET /cat-estado-sunat/{codigo}** - Auth: Si
- Response 200: Objeto
- Response 404: `ProblemDetails`

### TiposHabitacion

**GET /tipos-habitacion** (listar) - Auth: Si
> Obtiene los tipos de habitacion disponibles.

```json
[
  { "idTipo": 1, "nombre": "Matrimonial", "capacidad": 2, "descripcion": "Habitación estándar para dos personas", "precioBase": 50.0 },
  { "idTipo": 2, "nombre": "Doble", "capacidad": 3, "descripcion": "Habitación con dos camas individuales", "precioBase": 70.0 },
  { "idTipo": 3, "nombre": "Suite", "capacidad": 4, "descripcion": "Suite con sala de estar independiente", "precioBase": 120.0 }
]
```

**GET /tipos-habitacion/{id}** - Auth: Si
- Response 200: Objeto tipo habitacion
- Response 404: `ProblemDetails`

**POST /tipos-habitacion** - Auth: Si
```json
{ "nombre": "Presidencial", "capacidad": 6, "descripcion": "Suite presidencial de lujo", "precioBase": 300.0 }
```

**PUT /tipos-habitacion/{id}** - Auth: Si
- Response 204: Sin contenido

**DELETE /tipos-habitacion/{id}** - Auth: Si
- Response 204: Sin contenido

### CategoriaProducto

**GET /categoria-producto** (listar) - Auth: Si
> Obtiene las categorias de producto.

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
Obtiene la configuracion general del hotel.

- Auth: Si
- Response 200:
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
Verifica si la base de datos esta inicializada con datos basicos.

- Auth: No
- Response 200:
```json
{ "requiereInicializacion": false }
```

### POST /setup/crear-admin
Crea el usuario administrador por defecto (admin / Admin123!).

- Auth: No
- Response 200:
```json
{ "message": "Administrador creado exitosamente" }
```

### POST /setup/crear-usuarios-defecto
Crea los usuarios predefinidos: admin, recepcion, limpieza.

- Auth: No
- Response 200:
```json
{ "message": "Usuarios por defecto creados/verificados exitosamente" }
```

---

## Backup

### POST /backup/full
Realiza un backup completo de la base de datos.

- Auth: Si
- Response 200:
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
Realiza un backup diferencial (solo cambios desde el ultimo full).

- Auth: Si
- Response 200: Mismo shape que full backup

### POST /backup/log
Realiza un backup del log de transacciones.

- Auth: Si
- Response 200: Mismo shape que full backup

### GET /backup/history
Obtiene el historial de backups realizados.

- Auth: Si
- Response 200:
```json
[
  {
    "fileName": "Full_20260602_164500.bak",
    "filePath": "/backups/Full_20260602_164500.bak",
    "sizeBytes": 1048576,
    "tipo": "Full",
    "fechaCreacion": "2026-06-02T16:45:00"
  }
]
```
- Si no hay backups: `[]`

### GET /backup/download/{fileName}
Descarga un archivo de backup.

- Auth: Si
- Query param opcional: `?originalPath=/backups/Full_20260602_164500.bak`
- Response 200: `application/octet-stream`
- Response 404: Archivo no encontrado

---

## Health Check

### GET /health
Verifica que el servicio este operativo. **Nota:** Este endpoint esta fuera del prefijo `/api/v1`.

- Auth: No
- Response 200: `Healthy`
- Response 503: `Unhealthy` (si la BD no responde)

---

## Notas

### Convencion de rutas
- Las URLs en esta documentacion usan kebab-case por legibilidad, pero el ruteo es **case-insensitive**.
- Ejemplo: `/api/v1/cat-estado-habitacion` ≡ `/api/v1/CatEstadoHabitacion`
- Los nombres de propiedades en JSON usan PascalCase (convencion C#).

### TipoDocumento
Acepta tanto codigos ("1", "6", "7", "0") como nombres ("DNI", "RUC", "Pasaporte", "Otros"). El API normaliza automaticamente via `TipoDocumentoMapper`.

### MetodoPago
| Codigo | Descripcion |
|--------|-------------|
| 001 | Deposito en cuenta |
| 005 | Efectivo |
| 006 | Tarjeta de Credito/Debito |
| 008 | Yape/Plin / Transferencia |
| 999 | Otros |

### Usuarios predefinidos
| Usuario | Password | Rol |
|---------|----------|-----|
| admin | Admin123! | Administrador |
| recepcion | Recepcion123! | Recepcionista |
| limpieza | Limpieza123! | Limpieza |

### Errores
Todas las responses de error usan formato **ProblemDetails** (RFC 7807):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "La habitacion no esta disponible",
  "instance": "/api/v1/Estancia/checkin"
}
```

### Codigos de respuesta comunes
| Codigo | Significado |
|--------|-------------|
| 200 | OK (exito con body) |
| 201 | Created (recurso creado) |
| 204 | No Content (exito sin body) |
| 400 | Bad Request (error de validacion) |
| 401 | Unauthorized (no autenticado) |
| 403 | Forbidden (sin permisos) |
| 404 | Not Found (recurso no existe) |
| 409 | Conflict (duplicado/inconsistencia) |
| 502 | Bad Gateway (error externo, ej: RENIEC) |
