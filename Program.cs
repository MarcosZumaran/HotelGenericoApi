using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using HotelGenericoApi;
using HotelGenericoApi.Data;
using HotelGenericoApi.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using HotelGenericoApi.Hubs;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// DbContext — Testing usa InMemory, otros usan SQL Server
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<HotelDbContext>(options =>
        options.UseInMemoryDatabase("IntegrationTestDb"));
}
else
{
    builder.Services.AddDbContext<HotelDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddApplicationServices(builder.Configuration);

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("reniec", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("authenticated", context =>
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(userId,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                });
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

// JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token))
            {
                context.Token = context.Request.Cookies["auth_token"];
            }
            return Task.CompletedTask;
        }
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? ["http://localhost:5173"];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new HotelGenericoApi.JsonConverters.TrimStringConverter());
    });
builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.SwaggerDoc("v1", new()
    {
        Title = "Hotel Genérico API",
        Description = "API para gestión de hotel, huéspedes y facturación SUNAT",
        Version = "v1"
    });
});

var app = builder.Build();

// HQC-040: Validar placeholders en producción
if (app.Environment.IsProduction())
{
    var criticalSettings = new Dictionary<string, string>
    {
        ["Jwt:Key"] = app.Configuration["Jwt:Key"]!,
        ["ConnectionStrings:DefaultConnection"] = app.Configuration.GetConnectionString("DefaultConnection")!
    };

    foreach (var (name, value) in criticalSettings)
    {
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith("__"))
        {
            var error = $"CRÍTICO: La configuración de producción '{name}' no ha sido establecida. " +
                        "Asegúrate de configurar las variables de entorno o el archivo appsettings.Production.json.";
            throw new InvalidOperationException(error);
        }
    }
}

app.UseMiddleware<HotelGenericoApi.Middleware.ExceptionMiddleware>();

// Seed de usuarios por defecto en desarrollo
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var setupService = scope.ServiceProvider.GetRequiredService<SetupService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await setupService.CrearUsuariosPorDefectoAsync();
        logger.LogInformation("Usuarios por defecto creados/verificados exitosamente.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "No se pudieron crear los usuarios por defecto. Puede que ya existan.");
    }

    // Asegurar que el tipo de movimiento REPOSICION exista
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<HotelGenericoApi.Data.HotelDbContext>();
        if (!await db.TiposMovimientoStock.AnyAsync(t => t.Codigo == "REPOSICION"))
        {
            db.TiposMovimientoStock.Add(new HotelGenericoApi.Models.TipoMovimientoStock
            {
                Codigo = "REPOSICION",
                Descripcion = "Reposicion de amenidad en habitacion",
            });
            await db.SaveChangesAsync();
            logger.LogInformation("Tipo de movimiento REPOSICION creado exitosamente.");
        }

        var lateProduct = await db.Productos.FirstOrDefaultAsync(p => p.Nombre == "Late check-out");
        if (lateProduct != null && (lateProduct.Stock != 0 || lateProduct.StockMinimo != 0 || lateProduct.EsVendibleEnTienda))
        {
            lateProduct.Stock = 0;
            lateProduct.StockMinimo = 0;
            lateProduct.EsVendibleEnTienda = false;
            await db.SaveChangesAsync();
            logger.LogInformation("Producto 'Late check-out' corregido: stock=0, stock_minimo=0, vendible=false.");
        }

        if (!await db.Productos.AnyAsync(p => p.Nombre == "Depósito de garantía"))
        {
            db.Productos.Add(new HotelGenericoApi.Models.Producto
            {
                Nombre = "Depósito de garantía",
                Descripcion = "Depósito de garantía por estadía",
                PrecioUnitario = 0,
                IdAfectacionIgv = "10",
                Stock = 0,
                StockMinimo = 0,
                UnidadMedida = "NIU",
                EsAmenidad = false,
                EsVendibleEnTienda = false,
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
            logger.LogInformation("Producto 'Depósito de garantía' creado.");
        }

        if (!await db.Productos.AnyAsync(p => p.Nombre == "Early check-in"))
        {
            db.Productos.Add(new HotelGenericoApi.Models.Producto
            {
                Nombre = "Early check-in",
                Descripcion = "Cargo por entrada anticipada",
                PrecioUnitario = 20,
                IdAfectacionIgv = "10",
                Stock = 0,
                StockMinimo = 0,
                UnidadMedida = "NIU",
                EsAmenidad = false,
                EsVendibleEnTienda = false,
                CreatedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
            logger.LogInformation("Producto 'Early check-in' creado.");
        }

        var depKeys = new[] { "deposito_garantia_habilitado", "deposito_garantia_monto", "deposito_garantia_porcentaje",
            "early_checkin_hora_limite", "early_checkin_cargo" };
        foreach (var dk in depKeys)
        {
            if (!await db.ParametrosHotel.AnyAsync(p => p.Clave == dk))
            {
                var defVal = dk switch
                {
                    "deposito_garantia_habilitado" => "false",
                    "deposito_garantia_monto" => "50",
                    "deposito_garantia_porcentaje" => "30",
                    "early_checkin_hora_limite" => "10:00",
                    "early_checkin_cargo" => "20.00",
                    _ => ""
                };
                db.ParametrosHotel.Add(new HotelGenericoApi.Models.ParametroHotel
                {
                    Clave = dk,
                    Valor = defVal,
                    Descripcion = dk.Contains("deposito") ? "Parámetro de depósito de garantía" : "Parámetro de early check-in",
                    FechaActualizacion = DateTime.UtcNow,
                });
            }
        }
        await db.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "No se pudo asegurar los productos/parámetros de depósito y early check-in.");
    }
}

// Asegurar columna Pago.Concepto (compatibilidad con BD existente)
if (!app.Environment.IsEnvironment("Testing"))
{
    try
    {
        using var scope2 = app.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<HotelGenericoApi.Data.HotelDbContext>();
        await db2.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Pago') AND name = 'Concepto')
                ALTER TABLE Pago ADD Concepto nvarchar(200) NULL
        ");
    }
    catch
    {
    }
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapScalarApiReference(options =>
    {
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });
}

// Servir archivos estáticos desde wwwroot
app.UseStaticFiles();

// Crear carpetas de imágenes si no existen
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "imagenes", "productos"));
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "imagenes", "incidentes"));
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "imagenes", "objetos"));

app.UseResponseCompression();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// La ruta debe coincidir con el frontend: /hotelhub
app.MapHub<HabitacionHub>("/hotelhub").AllowAnonymous();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
