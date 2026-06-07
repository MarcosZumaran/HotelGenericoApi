# Procedimiento de Restauración de Backup
## Sistema Interno de Gestión de Hotel La Rica Noche (SIGHLRN)

### Requisitos previos
- SQL Server Management Studio (SSMS) o Azure Data Studio instalado
- Archivo de backup (.bak) generado desde el panel de administración
- Credenciales de administrador de la base de datos (usuario `sa`)

### Paso a paso

#### 1. Localizar el archivo de backup
- Los backups se descargan desde el panel **Backups** del sistema (menú Administrador → Backups)
- El archivo tiene extensión `.bak` y un nombre como `HotelDB_Full_20260607_143000.bak`
- Guardá el archivo en una carpeta de fácil acceso, por ejemplo `C:\backups\` en Windows

#### 2. Abrir SQL Server Management Studio
- Conectate al servidor usando las credenciales de administrador
- Servidor: `localhost` (o la IP del servidor donde está instalado el sistema)
- Autenticación: "Autenticación de SQL Server"
- Usuario: `sa`
- Contraseña: la que configuró el administrador del sistema

#### 3. Restaurar la base de datos
Ejecutá el siguiente comando SQL en una ventana de consulta nueva:

```sql
RESTORE DATABASE [HotelDB]
FROM DISK = 'C:\ruta\completa\al\archivo.bak'
WITH REPLACE,
     MOVE 'HotelDB' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\HotelDB.mdf',
     MOVE 'HotelDB_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\HotelDB_log.ldf';
```

**Importante:** Reemplazá `C:\ruta\completa\al\archivo.bak` por la ubicación real del archivo .bak
y ajustá las rutas de los archivos `.mdf` y `.ldf` según tu instalación de SQL Server.

#### 4. Verificar la restauración
- En SSMS, expandí la carpeta **Bases de datos**
- Deberías ver `HotelDB` listada
- Hacé clic derecho → **Nueva consulta** y ejecutá:
  ```sql
  SELECT COUNT(*) AS TotalHabitaciones FROM habitacion;
  ```
- Si ves un número, la base de datos está operativa

#### 5. Reiniciar la aplicación
- Detené la aplicación (cerrá la terminal donde se ejecuta)
- Volvé a iniciarla con:
  ```bash
  dotnet run --project HotelGenericoApi
  ```
- Si usás el acceso directo del escritorio, simplemente hacé doble clic nuevamente

#### 6. Verificar el sistema
- Abrí el navegador y accedé a `http://localhost:5173`
- Iniciá sesión con el usuario administrador
- Verificá que los datos (habitaciones, clientes, estancias) sean los esperados

### Solución de problemas

| Problema | Causa posible | Solución |
|----------|--------------|----------|
| "Database in use" | La base de datos está siendo usada por otra conexión | Ejecutá antes: `ALTER DATABASE [HotelDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;` |
| "Access denied" | El usuario de SQL Server no tiene permisos | Usá el usuario `sa` o pedile al administrador que te otorgue permisos `db_owner` |
| "File not found" | La ruta del archivo .bak es incorrecta | Verificá que la ruta completa sea correcta y que el archivo exista |

### Contacto
Si encontrás algún problema durante la restauración, contactá al administrador del sistema.
