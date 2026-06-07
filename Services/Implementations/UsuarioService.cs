using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Mappings;
using HotelGenericoApi.Services.Interfaces;
using HotelGenericoApi.Extensions;
using HotelGenericoApi.Models;
using HotelGenericoApi.Models.Exceptions;

namespace HotelGenericoApi.Services.Implementations;

public class UsuarioService : IUsuarioService
{
    private readonly HotelDbContext _db;
    private readonly UsuarioMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(HotelDbContext db, UsuarioMapper mapper, IConfiguration configuration, ILogger<UsuarioService> logger)
    {
        _db = db;
        _mapper = mapper;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<UsuarioResponseDto>> GetAllAsync()
    {
        var usuarios = await _db.Usuarios
            .Include(u => u.IdRolNavigation)
            .AsNoTracking()
            .ToListAsync();

        return usuarios.Select(u => new UsuarioResponseDto(
            u.IdUsuario,
            u.Username,
            u.IdRol,
            u.Rol?.Nombre ?? "",
            u.EstaActivo,
            u.FechaCreacion
        ));
    }

    public async Task<PagedResult<UsuarioResponseDto>> GetPagedAsync(int page, int pageSize)
    {
        var query = _db.Usuarios
            .Include(u => u.IdRolNavigation)
            .AsNoTracking()
            .Select(u => new UsuarioResponseDto(
                u.IdUsuario,
                u.Username,
                u.IdRol,
                u.Rol!.Nombre ?? "",
                u.EstaActivo,
                u.FechaCreacion
            ));
        return await query.ToPagedResultAsync(page, pageSize);
    }

    public async Task<UsuarioResponseDto?> GetByIdAsync(int id)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.IdRolNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUsuario == id);

        if (usuario is null) return null;

        return new UsuarioResponseDto(
            usuario.IdUsuario,
            usuario.Username,
            usuario.IdRol,
            usuario.Rol?.Nombre ?? "",
            usuario.EstaActivo,
            usuario.FechaCreacion
        );
    }

    public async Task<UsuarioResponseDto> CreateAsync(UsuarioCreateDto dto)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            if (await _db.Usuarios.AnyAsync(u => u.Username == dto.Username))
                throw new BusinessRuleViolationException(BusinessErrorCode.UserDuplicate, "El nombre de usuario ya existe.");

            var entity = _mapper.FromCreate(dto);
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            entity.FechaCreacion = DateTime.UtcNow;
            entity.EstaActivo = true;

            _db.Usuarios.Add(entity);
            await _db.SaveChangesAsync();

            await _db.Entry(entity).Reference(u => u.IdRolNavigation).LoadAsync();

            await transaction.CommitAsync();

            return new UsuarioResponseDto(
                entity.IdUsuario,
                entity.Username,
                entity.IdRol,
                entity.Rol?.Nombre ?? "",
                entity.EstaActivo,
                entity.FechaCreacion
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, UsuarioUpdateDto dto)
    {
        var entity = await _db.Usuarios.FindAsync(id);
        if (entity is null) return false;

        _mapper.UpdateFromDto(dto, entity);

        if (!string.IsNullOrEmpty(dto.Password))
        {
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return false;

        bool tieneVentas = await _db.Ventas.AnyAsync(v => v.IdUsuario == id);
        bool tieneReservas = await _db.Reservas.AnyAsync(r => r.IdUsuario == id);

        if (tieneVentas || tieneReservas)
            throw new BusinessRuleViolationException(BusinessErrorCode.UserHasActiveDependencies,
                "No se puede desactivar al usuario porque tiene ventas o reservas asociadas.");

        usuario.EstaActivo = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<(string token, UsuarioResponseDto usuario)?> LoginAsync(LoginDto dto, string? ipAddress = null, string? userAgent = null)
    {
        _logger.LogInformation("[Backend-Debug] Login attempt: Username={Username}, Ip={Ip}", dto.Username, ipAddress);

        var usuario = await _db.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.Username == dto.Username && u.EstaActivo == true);

        bool succeeded;

        if (usuario is null)
        {
            _logger.LogWarning("[Backend-Debug] Login failed: user '{Username}' not found or inactive", dto.Username);
            succeeded = false;
            await RegistrarLoginAttemptAsync(ipAddress, dto.Username, succeeded, userAgent);
            return null;
        }

        _logger.LogInformation("[Backend-Debug] User found: Id={Id}, Rol={Rol}, Active={Active}",
            usuario.IdUsuario, usuario.Rol?.Nombre, usuario.EstaActivo);

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
        {
            _logger.LogWarning("[Backend-Debug] Login failed: invalid password for user '{Username}'", dto.Username);
            succeeded = false;
            await RegistrarLoginAttemptAsync(ipAddress, dto.Username, succeeded, userAgent);
            return null;
        }

        _logger.LogInformation("[Backend-Debug] Login success: user '{Username}', generating token", dto.Username);
        succeeded = true;
        await RegistrarLoginAttemptAsync(ipAddress, dto.Username, succeeded, userAgent);

        var token = GenerarToken(usuario);
        _logger.LogInformation("[Backend-Debug] Token generated for '{Username}', expires in {Min} min",
            dto.Username, ObtenerExpiracionMinutos(usuario.Rol?.Nombre));

        var usuarioDto = new UsuarioResponseDto(
            usuario.IdUsuario,
            usuario.Username,
            usuario.IdRol,
            usuario.Rol?.Nombre ?? "",
            usuario.EstaActivo,
            usuario.FechaCreacion
        );

        return (token, usuarioDto);
    }

    private static int ObtenerExpiracionMinutos(string? nombreRol)
    {
        return nombreRol switch
        {
            "Administrador" => 10,
            "Recepcion" => 720,
            "Limpieza" => 1440,
            _ => 30
        };
    }

    private async Task RegistrarLoginAttemptAsync(string? ipAddress, string? username, bool succeeded, string? userAgent)
    {
        try
        {
            var attempt = new LoginAttempt
            {
                IpAddress = ipAddress ?? "",
                Username = username,
                AttemptedAt = DateTime.UtcNow,
                Succeeded = succeeded,
                UserAgent = userAgent?.Length > 500 ? userAgent[..500] : userAgent
            };
            _db.LoginAttempts.Add(attempt);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar LoginAttempt");
        }
    }

    private string GenerarToken(Usuario usuario)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        int expirationMinutes = ObtenerExpiracionMinutos(usuario.Rol?.Nombre);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
        new Claim(ClaimTypes.Name, usuario.Username),
        new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "")
    };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}