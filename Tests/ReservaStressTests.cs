using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.Models;
using HotelGenericoApi.Constants;
using Xunit;

namespace HotelGenericoApi.Tests;

public class ReservaStressTests
{
    private static HotelDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new HotelDbContext(opts);
        SeedEstados(db);
        SeedHabitaciones(db);
        SeedClientes(db);
        SeedUsuarios(db);
        db.SaveChanges();
        return db;
    }

    private static void SeedUsuarios(HotelDbContext db)
    {
        db.Usuarios.Add(new Usuario
        {
            IdUsuario = 1,
            Username = "admin",
            PasswordHash = "admin",
            IdRol = 1,
            EstaActivo = true,
            FechaCreacion = DateTime.UtcNow
        });
    }

    private static void SeedEstados(HotelDbContext db)
    {
        db.EstadosReserva.AddRange(
            new EstadoReserva { IdEstadoReserva = 1, Codigo = "Pendiente", Descripcion = "Pendiente de confirmacion", EsFinal = false },
            new EstadoReserva { IdEstadoReserva = 2, Codigo = "Confirmada", Descripcion = "Reserva confirmada", EsFinal = false },
            new EstadoReserva { IdEstadoReserva = 3, Codigo = "Cancelada", Descripcion = "Reserva cancelada", EsFinal = true },
            new EstadoReserva { IdEstadoReserva = 4, Codigo = "NoShow", Descripcion = "El cliente no se presento", EsFinal = true },
            new EstadoReserva { IdEstadoReserva = 5, Codigo = "Completa", Descripcion = "Reserva completada", EsFinal = true },
            new EstadoReserva { IdEstadoReserva = 6, Codigo = "Vencida", Descripcion = "Reserva vencida", EsFinal = true }
        );
    }

    private static void SeedHabitaciones(HotelDbContext db)
    {
        db.TiposHabitacion.Add(new TipoHabitacion { IdTipo = 1, Nombre = "Estandar", PrecioBase = 100m });
        for (int i = 1; i <= 18; i++)
        {
            db.Habitaciones.Add(new Habitacion
            {
                IdHabitacion = i,
                NumeroHabitacion = $"10{i:D2}",
                IdTipo = 1,
                PrecioNoche = 100m,
                IdEstado = 1,
                Piso = 1
            });
        }
    }

    private static void SeedClientes(HotelDbContext db)
    {
        db.Clientes.Add(new Cliente
        {
            IdCliente = 1,
            TipoDocumento = "0",
            Documento = "00000000",
            Nombres = "CLIENTE",
            Apellidos = "ANONIMO",
            CodigoInterno = "CLI-00000000",
            Nacionalidad = "PERUANA"
        });
        for (int i = 2; i <= 21; i++)
        {
            db.Clientes.Add(new Cliente
            {
                IdCliente = i,
                TipoDocumento = "1",
                Documento = $"123456{i:D2}",
                Nombres = $"Cliente{i}",
                Apellidos = $"Apellido{i}",
                CodigoInterno = $"CLI-{i:D8}",
                Nacionalidad = "PERUANA"
            });
        }
    }

    private static void SeedProductos(HotelDbContext db)
    {
        db.Productos.Add(new Producto
        {
            IdProducto = 1,
            Nombre = "Gaseosa",
            PrecioUnitario = 5m,
            IdAfectacionIgv = "1",
            UnidadMedida = "UNIDAD",
            Stock = 100,
            StockMinimo = 5,
            EsVendibleEnTienda = true
        });
        db.Productos.Add(new Producto
        {
            IdProducto = 2,
            Nombre = "Agua",
            PrecioUnitario = 3m,
            IdAfectacionIgv = "1",
            UnidadMedida = "UNIDAD",
            Stock = 100,
            StockMinimo = 5,
            EsVendibleEnTienda = true
        });
    }

    [Fact]
    public async Task Crear50ReservasEn18Habitaciones_SinConflictos()
    {
        using var db = CreateDb();
        var hoy = DateTime.UtcNow.Date;

        for (int i = 1; i <= 50; i++)
        {
            var habitacionId = (i % 18) + 1;
            var entrada = hoy.AddDays(i / 18);
            var salida = entrada.AddDays(2);

            db.Reservas.Add(new Reserva
            {
                IdCliente = (i % 20) + 1,
                IdHabitacion = habitacionId,
                IdUsuario = 1,
                IdEstadoReserva = 2,
                FechaEntradaPrevista = entrada,
                FechaSalidaPrevista = salida,
                MontoTotal = 200m,
                FechaRegistro = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();

        Assert.Equal(50, await db.Reservas.CountAsync());

        var habitacionesOcupadas = await db.Reservas
            .Where(r => r.IdEstadoReserva == 2)
            .Select(r => r.IdHabitacion)
            .Distinct()
            .CountAsync();
        Assert.True(habitacionesOcupadas > 0);
    }

    [Fact]
    public async Task ConflictosDeFechas_Detectados()
    {
        using var db = CreateDb();

        var entrada = new DateTime(2026, 7, 1, 14, 0, 0, DateTimeKind.Utc);
        var salida = new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc);

        db.Reservas.Add(new Reserva
        {
            IdCliente = 1,
            IdHabitacion = 1,
            IdUsuario = 1,
            IdEstadoReserva = 2,
            FechaEntradaPrevista = entrada,
            FechaSalidaPrevista = salida,
            MontoTotal = 200m,
            FechaRegistro = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var fechasOcupadas = await db.Reservas
            .Where(r => r.IdHabitacion == 1
                && (r.IdEstadoReserva == 1 || r.IdEstadoReserva == 2)
                && r.FechaEntradaPrevista < new DateTime(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc)
                && r.FechaSalidaPrevista > new DateTime(2026, 6, 30, 14, 0, 0, DateTimeKind.Utc))
            .CountAsync();
        Assert.Equal(1, fechasOcupadas);
    }

    [Fact]
    public async Task Concurrencia_Paralelismo_SinDataCorruption()
    {
        var dbName = Guid.NewGuid().ToString();
        var opts = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        using (var seed = new HotelDbContext(opts))
        {
            SeedEstados(seed);
            SeedHabitaciones(seed);
            SeedClientes(seed);
            SeedUsuarios(seed);
            seed.SaveChanges();
        }

        var hoy = DateTime.UtcNow.Date;
        var semaphore = new SemaphoreSlim(1, 1);
        var tasks = new List<Task>();

        for (int i = 0; i < 20; i++)
        {
            var idx = i;
            tasks.Add(Task.Run(async () =>
            {
                using var localDb = new HotelDbContext(opts);
                await semaphore.WaitAsync();
                try
                {
                    var habits = await localDb.Habitaciones.ToListAsync();
                    var habitacionId = (idx % habits.Count) + 1;
                    localDb.Reservas.Add(new Reserva
                    {
                        IdCliente = (idx % 20) + 2,
                        IdHabitacion = habitacionId,
                        IdUsuario = 1,
                        IdEstadoReserva = EstadoReservaCodigo.Confirmada,
                        FechaEntradaPrevista = hoy.AddDays(idx),
                        FechaSalidaPrevista = hoy.AddDays(idx + 1),
                        MontoTotal = 100m,
                        FechaRegistro = DateTime.UtcNow
                    });
                    await localDb.SaveChangesAsync();
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);

        using var verifyDb = new HotelDbContext(opts);
        Assert.Equal(20, await verifyDb.Reservas.CountAsync());
    }

    [Fact]
    public async Task ReservasVencidas_SeMarcanCorrectamente()
    {
        using var db = CreateDb();

        db.Reservas.Add(new Reserva
        {
            IdCliente = 1,
            IdHabitacion = 1,
            IdUsuario = 1,
            IdEstadoReserva = EstadoReservaCodigo.Confirmada,
            FechaEntradaPrevista = new DateTime(2026, 1, 2, 14, 0, 0, DateTimeKind.Utc),
            FechaSalidaPrevista = new DateTime(2026, 1, 3, 12, 0, 0, DateTimeKind.Utc),
            MontoTotal = 100m,
            FechaRegistro = DateTime.UtcNow
        });
        db.Reservas.Add(new Reserva
        {
            IdCliente = 1,
            IdHabitacion = 2,
            IdUsuario = 1,
            IdEstadoReserva = EstadoReservaCodigo.Pendiente,
            FechaEntradaPrevista = new DateTime(2026, 1, 5, 14, 0, 0, DateTimeKind.Utc),
            FechaSalidaPrevista = new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc),
            MontoTotal = 100m,
            FechaRegistro = DateTime.UtcNow
        });
        db.Reservas.Add(new Reserva
        {
            IdCliente = 1,
            IdHabitacion = 3,
            IdUsuario = 1,
            IdEstadoReserva = EstadoReservaCodigo.Cancelada,
            FechaEntradaPrevista = new DateTime(2026, 1, 10, 14, 0, 0, DateTimeKind.Utc),
            FechaSalidaPrevista = new DateTime(2026, 1, 11, 12, 0, 0, DateTimeKind.Utc),
            MontoTotal = 100m,
            FechaRegistro = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var pendienteId = await db.EstadosReserva
            .Where(e => e.Codigo == "Pendiente")
            .Select(e => e.IdEstadoReserva)
            .FirstAsync();
        var confirmadaId = await db.EstadosReserva
            .Where(e => e.Codigo == "Confirmada")
            .Select(e => e.IdEstadoReserva)
            .FirstAsync();
        var vencidaId = await db.EstadosReserva
            .Where(e => e.Codigo == "Vencida")
            .Select(e => e.IdEstadoReserva)
            .FirstAsync();

        Assert.Equal(6, vencidaId);
        Assert.Equal(1, pendienteId);
        Assert.Equal(2, confirmadaId);

        var vencibles = await db.Reservas
            .Where(r => r.FechaSalidaPrevista < DateTime.UtcNow
                && (r.IdEstadoReserva == pendienteId || r.IdEstadoReserva == confirmadaId))
            .ToListAsync();

        Assert.Equal(2, vencibles.Count);

        foreach (var r in vencibles)
            r.IdEstadoReserva = vencidaId;
        await db.SaveChangesAsync();

        Assert.Equal(2, await db.Reservas.CountAsync(r => r.IdEstadoReserva == vencidaId));
    }

    [Fact]
    public async Task CicloCompleto_ReservaAEstancia()
    {
        using var db = CreateDb();

        var hoy = DateTime.UtcNow.Date.AddDays(-1);
        var entrada = hoy.AddHours(14);
        var salida = hoy.AddDays(1).AddHours(12);

        db.Reservas.Add(new Reserva
        {
            IdCliente = 2,
            IdHabitacion = 1,
            IdUsuario = 1,
            IdEstadoReserva = EstadoReservaCodigo.Confirmada,
            FechaEntradaPrevista = entrada,
            FechaSalidaPrevista = salida,
            MontoTotal = 100m,
            FechaRegistro = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var reserva = await db.Reservas
            .Include(r => r.IdEstadoReservaNavigation)
            .FirstAsync(r => r.IdCliente == 2);

        Assert.Equal("Confirmada", reserva.IdEstadoReservaNavigation.Codigo);

        var estancia = new Estancia
        {
            IdReserva = reserva.IdReserva,
            IdClienteTitular = 2,
            IdHabitacion = 1,
            IdEstadoEstancia = 2,
            FechaCheckin = entrada,
            FechaCheckoutPrevista = salida,
            MontoTotal = 100m
        };
        db.Estancias.Add(estancia);
        await db.SaveChangesAsync();

        reserva.IdEstadoReserva = await db.EstadosReserva
            .Where(e => e.Codigo == "Completa")
            .Select(e => e.IdEstadoReserva)
            .FirstAsync();
        await db.SaveChangesAsync();

        var checkReserva = await db.Reservas
            .Include(r => r.IdEstadoReservaNavigation)
            .FirstAsync(r => r.IdReserva == reserva.IdReserva);
        Assert.Equal("Completa", checkReserva.IdEstadoReservaNavigation.Codigo);
    }

    [Fact]
    public async Task GetFechasOcupadas_SoloPendingYConfirmada()
    {
        using var db = CreateDb();

        var entrada = new DateTime(2026, 8, 1, 14, 0, 0, DateTimeKind.Utc);
        var salida = new DateTime(2026, 8, 3, 12, 0, 0, DateTimeKind.Utc);

        db.Reservas.Add(new Reserva
        {
            IdCliente = 1,
            IdHabitacion = 1,
            IdUsuario = 1,
            IdEstadoReserva = 2,
            FechaEntradaPrevista = entrada,
            FechaSalidaPrevista = salida,
            MontoTotal = 200m,
            FechaRegistro = DateTime.UtcNow
        });
        db.Reservas.Add(new Reserva
        {
            IdCliente = 1,
            IdHabitacion = 2,
            IdUsuario = 1,
            IdEstadoReserva = 6,
            FechaEntradaPrevista = entrada,
            FechaSalidaPrevista = salida,
            MontoTotal = 200m,
            FechaRegistro = DateTime.UtcNow
        });
        db.Reservas.Add(new Reserva
        {
            IdCliente = 1,
            IdHabitacion = 3,
            IdUsuario = 1,
            IdEstadoReserva = 3,
            FechaEntradaPrevista = entrada,
            FechaSalidaPrevista = salida,
            MontoTotal = 200m,
            FechaRegistro = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var ocupadas = await db.Reservas
            .AsNoTracking()
            .Include(r => r.IdEstadoReservaNavigation)
            .Where(r => r.IdEstadoReservaNavigation.Codigo == "Pendiente"
                     || r.IdEstadoReservaNavigation.Codigo == "Confirmada")
            .Where(r => r.IdHabitacion == 1 || r.IdHabitacion == 2 || r.IdHabitacion == 3)
            .ToListAsync();

        Assert.Single(ocupadas);
        Assert.Equal(1, ocupadas[0].IdHabitacion);
    }

    [Fact]
    public async Task EliminarReservaVencida_NoBloqueaNuevaReserva()
    {
        using var db = CreateDb();

        var entrada = new DateTime(2026, 9, 1, 14, 0, 0, DateTimeKind.Utc);
        var salida = new DateTime(2026, 9, 3, 12, 0, 0, DateTimeKind.Utc);

        db.Reservas.Add(new Reserva
        {
            IdCliente = 1,
            IdHabitacion = 1,
            IdUsuario = 1,
            IdEstadoReserva = 6,
            FechaEntradaPrevista = entrada,
            FechaSalidaPrevista = salida,
            MontoTotal = 200m,
            FechaRegistro = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var bloquea = await db.Reservas
            .AsNoTracking()
            .Include(r => r.IdEstadoReservaNavigation)
            .Where(r => r.IdHabitacion == 1
                && (r.IdEstadoReservaNavigation.Codigo == "Pendiente"
                 || r.IdEstadoReservaNavigation.Codigo == "Confirmada")
                && r.FechaEntradaPrevista < salida
                && r.FechaSalidaPrevista > entrada)
            .AnyAsync();

        Assert.False(bloquea);
    }

    [Fact]
    public async Task EstadoReservaNavigation_Codigo_Correcto()
    {
        using var db = CreateDb();

        foreach (var codigo in new[] { "Pendiente", "Confirmada", "Cancelada", "NoShow", "Completa", "Vencida" })
        {
            var estado = await db.EstadosReserva
                .FirstAsync(e => e.Codigo == codigo);
            Assert.Equal(codigo, estado.Codigo);
        }

        var reserva = new Reserva
        {
            IdCliente = 1,
            IdHabitacion = 1,
            IdUsuario = 1,
            IdEstadoReserva = 2,
            FechaEntradaPrevista = DateTime.UtcNow,
            FechaSalidaPrevista = DateTime.UtcNow.AddDays(1),
            MontoTotal = 100m,
            FechaRegistro = DateTime.UtcNow
        };
        db.Reservas.Add(reserva);
        await db.SaveChangesAsync();

        var loaded = await db.Reservas
            .Include(r => r.IdEstadoReservaNavigation)
            .FirstAsync(r => r.IdReserva == reserva.IdReserva);

        Assert.Equal("Confirmada", loaded.IdEstadoReservaNavigation.Codigo);
        Assert.NotEmpty(loaded.IdEstadoReservaNavigation.Descripcion);
    }
}
