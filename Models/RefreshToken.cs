using System;

namespace HotelGenericoApi.Models;

public partial class RefreshToken
{
    public int IdRefreshToken { get; set; }

    public int IdUsuario { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsActive => RevokedAt == null && !IsExpired;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
