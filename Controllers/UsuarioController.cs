using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HotelGenericoApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _service;
    private readonly IConfiguration _configuration;

    public UsuarioController(IUsuarioService service, IConfiguration configuration)
    {
        _service = service;
        _configuration = configuration;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<DTOs.Response.UsuarioResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetPagedAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DTOs.Response.UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(DTOs.Response.UsuarioResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(UsuarioCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.IdUsuario }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UsuarioUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(DTOs.Response.UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _service.LoginAsync(dto, ipAddress, userAgent);
        if (result is null) return Unauthorized();

        var (token, refreshToken, usuario) = result.Value;

        var (secure, sameSite) = GetCookieOptions();
        var expiration = ObtenerExpiracionToken(token);

        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Expires = expiration,
            Path = "/"
        });

        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        });

        return Ok(usuario);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DTOs.Response.UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "No hay refresh token" });

        var result = await _service.RefreshTokenAsync(refreshToken);
        if (result is null)
        {
            Response.Cookies.Delete("auth_token", GetDeleteOptions());
            Response.Cookies.Delete("refresh_token", GetDeleteOptions());
            return Unauthorized(new { message = "Refresh token inválido o expirado" });
        }

        var (token, nuevoRefresh, usuario) = result.Value;

        var (secure, sameSite) = GetCookieOptions();
        var expiration = ObtenerExpiracionToken(token);

        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Expires = expiration,
            Path = "/"
        });

        Response.Cookies.Append("refresh_token", nuevoRefresh, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        });

        return Ok(usuario);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(DTOs.Response.UsuarioResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var usuario = await _service.GetByIdAsync(userId);
        return usuario is not null ? Ok(usuario) : Unauthorized();
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is not null && int.TryParse(userIdClaim, out var userId))
        {
            await _service.RevokeRefreshTokensAsync(userId);
        }
        else
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var stored = await _service.RefreshTokenAsync(refreshToken);
                if (stored.HasValue)
                {
                    await _service.RevokeRefreshTokensAsync(stored.Value.usuario.IdUsuario);
                }
            }
        }

        Response.Cookies.Delete("auth_token", GetDeleteOptions());
        Response.Cookies.Delete("refresh_token", GetDeleteOptions());

        return Ok(new { message = "Sesión cerrada" });
    }

    private (bool secure, SameSiteMode sameSite) GetCookieOptions()
    {
        var cookieSection = _configuration.GetSection("CookieAuth");
        var secure = cookieSection.GetValue<bool>("Secure");
        var sameSite = cookieSection.GetValue<string>("SameSite") switch
        {
            "Strict" => SameSiteMode.Strict,
            "Lax" => SameSiteMode.Lax,
            "None" => SameSiteMode.None,
            _ => SameSiteMode.Lax
        };
        return (secure, sameSite);
    }

    private static DateTime ObtenerExpiracionToken(string token)
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        return jwtToken.ValidTo;
    }

    private static CookieOptions GetDeleteOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Path = "/"
        };
    }
}
