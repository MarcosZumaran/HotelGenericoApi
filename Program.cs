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

// Crear carpeta de imágenes si no existe
var imagenesPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "imagenes", "productos");
Directory.CreateDirectory(imagenesPath);

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
