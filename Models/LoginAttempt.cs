using System;
using System.Collections.Generic;

namespace HotelGenericoApi.Models;

public partial class LoginAttempt
{
    public int IdLoginAttempt { get; set; }

    public string IpAddress { get; set; } = null!;

    public string? Username { get; set; }

    public DateTime AttemptedAt { get; set; }

    public bool Succeeded { get; set; }

    public string? UserAgent { get; set; }
}
