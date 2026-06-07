using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.Models;
using HotelGenericoApi.Models.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HotelGenericoApi.Services.Implementations;

public class SetupService
{
    private readonly HotelDbContext _db;
    private readonly ILogger<SetupService> _logger;

    public SetupService(HotelDbContext db, ILogger<SetupService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> EsPrimerInicioAsync()
    {
        return !await _db.Usuarios.AnyAsync();
    }

    public async Task CrearUsuarioAdminAsync(UsuarioCreateDto dto)
    {
        if (!await EsPrimerInicioAsync())
            throw new BusinessRuleViolationException(BusinessErrorCode.SetupAlreadyDone, "El sistema ya fue inicializado.");

        var rolAdmin = await _db.RolesUsuario.FirstOrDefaultAsync(r => r.Nombre == "Administrador")
            ?? throw new BusinessRuleViolationException(BusinessErrorCode.ValidationError, "Rol Administrador no encontrado en el catálogo.");

        var usuario = new Usuario
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IdRol = rolAdmin.IdRol,
            FechaCreacion = DateTime.UtcNow,
            EstaActivo = true
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
    }

    public async Task CrearUsuariosPorDefectoAsync()
    {
        // Asegurar que los roles existen
        var roles = new[] { "Administrador", "Recepcion", "Limpieza" };
        foreach (var nombreRol in roles)
        {
            if (!await _db.RolesUsuario.AnyAsync(r => r.Nombre == nombreRol))
            {
                _db.RolesUsuario.Add(new RolUsuario { Nombre = nombreRol });
            }
        }
        await _db.SaveChangesAsync();

        // Obtener IDs de roles
        var rolAdmin = await _db.RolesUsuario.FirstAsync(r => r.Nombre == "Administrador");
        var rolRecepcion = await _db.RolesUsuario.FirstAsync(r => r.Nombre == "Recepcion");
        var rolLimpieza = await _db.RolesUsuario.FirstAsync(r => r.Nombre == "Limpieza");

        // Definir usuarios por defecto con contraseñas conocidas en desarrollo
        var usuariosPorDefecto = new (string Username, int IdRol, string Password)[]
        {
        ("admin", rolAdmin.IdRol, "Admin123!"),
        ("recepcion", rolRecepcion.IdRol, "Recepcion123!"),
        ("limpieza", rolLimpieza.IdRol, "Limpieza123!")
        };

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var (username, idRol, password) in usuariosPorDefecto)
            {
                if (!await _db.Usuarios.AnyAsync(u => u.Username == username))
                {
                    _db.Usuarios.Add(new Usuario
                    {
                        Username = username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        IdRol = idRol,
                        FechaCreacion = DateTime.UtcNow,
                        EstaActivo = true,
                        DebeCambiarPassword = false
                    });

                    _logger.LogInformation("Usuario creado: {Username} (Rol: {Rol})", username,
                    usuariosPorDefecto.First(u => u.Username == username).IdRol == rolAdmin.IdRol ? "Administrador"
                    : username == "recepcion" ? "Recepcion"
                    : "Limpieza");
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


}
