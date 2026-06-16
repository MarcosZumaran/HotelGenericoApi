using Microsoft.EntityFrameworkCore;
using HotelGenericoApi.Data;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Interfaces;

namespace HotelGenericoApi.Services.Implementations;

public class ParametroHotelService : IParametroHotelService
{
    private readonly HotelDbContext _db;

    private static readonly Dictionary<string, string> LimpiezaDefaults = new()
    {
        ["limpieza_salida_tiempo"] = "40",
        ["limpieza_estancia_tiempo"] = "20",
        ["limpieza_frecuencia_horas"] = "24",
        ["limpieza_horario_inicio"] = "08:00",
        ["limpieza_horario_fin"] = "14:00",
    };

    private static readonly Dictionary<string, string> CheckoutDefaults = new()
    {
        ["checkout_hora_limite"] = "12:00",
        ["checkout_cargo_por_hora"] = "50.00",
        ["checkout_gracia_minutos"] = "30",
    };

    private static readonly Dictionary<string, string> LimpiezaDtoKeyToDbKey = new()
    {
        ["LimpiezaSalidaTiempo"] = "limpieza_salida_tiempo",
        ["LimpiezaEstanciaTiempo"] = "limpieza_estancia_tiempo",
        ["LimpiezaFrecuenciaHoras"] = "limpieza_frecuencia_horas",
        ["LimpiezaHorarioInicio"] = "limpieza_horario_inicio",
        ["LimpiezaHorarioFin"] = "limpieza_horario_fin",
    };

    private static readonly Dictionary<string, string> CheckoutDtoKeyToDbKey = new()
    {
        ["CheckoutHoraLimite"] = "checkout_hora_limite",
        ["CheckoutCargoPorHora"] = "checkout_cargo_por_hora",
        ["CheckoutGraciaMinutos"] = "checkout_gracia_minutos",
    };

    private static readonly Dictionary<string, string> PagosDefaults = new()
    {
        ["pagos_metodos_habilitados"] = "Efectivo,Tarjeta,Depósito",
        ["pagos_tasa_igv_hotel"] = "10.50",
        ["pagos_tasa_igv_productos"] = "18.00",
    };

    private static readonly Dictionary<string, string> NotificacionesDefaults = new()
    {
        ["notif_email"] = "",
        ["notif_checkin"] = "true",
        ["notif_checkout"] = "true",
        ["notif_incidentes"] = "false",
        ["notif_stock_bajo"] = "true",
    };

    private static readonly Dictionary<string, string> DepositoGarantiaDefaults = new()
    {
        ["deposito_garantia_habilitado"] = "false",
        ["deposito_garantia_monto"] = "50",
        ["deposito_garantia_porcentaje"] = "30",
    };

    private static readonly Dictionary<string, string> EarlyCheckinDefaults = new()
    {
        ["early_checkin_hora_limite"] = "10:00",
        ["early_checkin_cargo"] = "20.00",
    };

    private static readonly Dictionary<string, string> PagosDtoKeyToDbKey = new()
    {
        ["MetodosPagoHabilitados"] = "pagos_metodos_habilitados",
        ["TasaIgvHotel"] = "pagos_tasa_igv_hotel",
        ["TasaIgvProductos"] = "pagos_tasa_igv_productos",
    };

    private static readonly Dictionary<string, string> NotificacionesDtoKeyToDbKey = new()
    {
        ["EmailNotificaciones"] = "notif_email",
        ["NotificarCheckin"] = "notif_checkin",
        ["NotificarCheckout"] = "notif_checkout",
        ["NotificarIncidentes"] = "notif_incidentes",
        ["NotificarStockBajo"] = "notif_stock_bajo",
    };

    private static readonly Dictionary<string, string> DepositoGarantiaDtoKeyToDbKey = new()
    {
        ["DepositoHabilitado"] = "deposito_garantia_habilitado",
        ["DepositoMonto"] = "deposito_garantia_monto",
        ["DepositoPorcentaje"] = "deposito_garantia_porcentaje",
    };

    private static readonly Dictionary<string, string> EarlyCheckinDtoKeyToDbKey = new()
    {
        ["EarlyCheckinHoraLimite"] = "early_checkin_hora_limite",
        ["EarlyCheckinCargo"] = "early_checkin_cargo",
    };

    public ParametroHotelService(HotelDbContext db)
    {
        _db = db;
    }

    public async Task<LimpiezaParamsDto> GetLimpiezaParamsAsync()
    {
        var claves = LimpiezaDefaults.Keys.ToList();
        var existentes = await _db.ParametrosHotel
            .Where(p => claves.Contains(p.Clave))
            .ToListAsync();

        var map = existentes.ToDictionary(p => p.Clave, p => p.Valor);

        return new LimpiezaParamsDto
        {
            LimpiezaSalidaTiempo = map.GetValueOrDefault("limpieza_salida_tiempo", LimpiezaDefaults["limpieza_salida_tiempo"]),
            LimpiezaEstanciaTiempo = map.GetValueOrDefault("limpieza_estancia_tiempo", LimpiezaDefaults["limpieza_estancia_tiempo"]),
            LimpiezaFrecuenciaHoras = map.GetValueOrDefault("limpieza_frecuencia_horas", LimpiezaDefaults["limpieza_frecuencia_horas"]),
            LimpiezaHorarioInicio = map.GetValueOrDefault("limpieza_horario_inicio", LimpiezaDefaults["limpieza_horario_inicio"]),
            LimpiezaHorarioFin = map.GetValueOrDefault("limpieza_horario_fin", LimpiezaDefaults["limpieza_horario_fin"]),
        };
    }

    public async Task UpdateLimpiezaParamsAsync(LimpiezaParamsUpdateDto dto)
    {
        var props = typeof(LimpiezaParamsUpdateDto).GetProperties()
            .Where(p => p.GetValue(dto) != null)
            .ToList();

        foreach (var prop in props)
        {
            var dbKey = LimpiezaDtoKeyToDbKey[prop.Name];
            var valor = (string)prop.GetValue(dto)!;

            var existente = await _db.ParametrosHotel
                .FirstOrDefaultAsync(p => p.Clave == dbKey);

            if (existente != null)
            {
                existente.Valor = valor;
                existente.FechaActualizacion = DateTime.UtcNow;
            }
            else
            {
                _db.ParametrosHotel.Add(new ParametroHotel
                {
                    Clave = dbKey,
                    Valor = valor,
                    Descripcion = "Parámetro de limpieza",
                    FechaActualizacion = DateTime.UtcNow,
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<CheckoutParamsDto> GetCheckoutParamsAsync()
    {
        var claves = CheckoutDefaults.Keys.ToList();
        var existentes = await _db.ParametrosHotel
            .Where(p => claves.Contains(p.Clave))
            .ToListAsync();

        var map = existentes.ToDictionary(p => p.Clave, p => p.Valor);

        return new CheckoutParamsDto
        {
            CheckoutHoraLimite = map.GetValueOrDefault("checkout_hora_limite", CheckoutDefaults["checkout_hora_limite"]),
            CheckoutCargoPorHora = map.GetValueOrDefault("checkout_cargo_por_hora", CheckoutDefaults["checkout_cargo_por_hora"]),
            CheckoutGraciaMinutos = map.GetValueOrDefault("checkout_gracia_minutos", CheckoutDefaults["checkout_gracia_minutos"]),
        };
    }

    public async Task UpdateCheckoutParamsAsync(CheckoutParamsUpdateDto dto)
    {
        var props = typeof(CheckoutParamsUpdateDto).GetProperties()
            .Where(p => p.GetValue(dto) != null)
            .ToList();

        foreach (var prop in props)
        {
            var dbKey = CheckoutDtoKeyToDbKey[prop.Name];
            var valor = (string)prop.GetValue(dto)!;

            var existente = await _db.ParametrosHotel
                .FirstOrDefaultAsync(p => p.Clave == dbKey);

            if (existente != null)
            {
                existente.Valor = valor;
                existente.FechaActualizacion = DateTime.UtcNow;
            }
            else
            {
                _db.ParametrosHotel.Add(new ParametroHotel
                {
                    Clave = dbKey,
                    Valor = valor,
                    Descripcion = "Parámetro de checkout",
                    FechaActualizacion = DateTime.UtcNow,
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<PagosParamsDto> GetPagosParamsAsync()
    {
        var claves = PagosDefaults.Keys.ToList();
        var existentes = await _db.ParametrosHotel
            .Where(p => claves.Contains(p.Clave))
            .ToListAsync();

        var map = existentes.ToDictionary(p => p.Clave, p => p.Valor);

        return new PagosParamsDto
        {
            MetodosPagoHabilitados = map.GetValueOrDefault("pagos_metodos_habilitados", PagosDefaults["pagos_metodos_habilitados"]),
            TasaIgvHotel = map.GetValueOrDefault("pagos_tasa_igv_hotel", PagosDefaults["pagos_tasa_igv_hotel"]),
            TasaIgvProductos = map.GetValueOrDefault("pagos_tasa_igv_productos", PagosDefaults["pagos_tasa_igv_productos"]),
        };
    }

    public async Task UpdatePagosParamsAsync(PagosParamsUpdateDto dto)
    {
        var props = typeof(PagosParamsUpdateDto).GetProperties()
            .Where(p => p.GetValue(dto) != null)
            .ToList();

        foreach (var prop in props)
        {
            var dbKey = PagosDtoKeyToDbKey[prop.Name];
            var valor = (string)prop.GetValue(dto)!;

            var existente = await _db.ParametrosHotel
                .FirstOrDefaultAsync(p => p.Clave == dbKey);

            if (existente != null)
            {
                existente.Valor = valor;
                existente.FechaActualizacion = DateTime.UtcNow;
            }
            else
            {
                _db.ParametrosHotel.Add(new ParametroHotel
                {
                    Clave = dbKey,
                    Valor = valor,
                    Descripcion = "Parámetro de pagos",
                    FechaActualizacion = DateTime.UtcNow,
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<NotificacionesParamsDto> GetNotificacionesParamsAsync()
    {
        var claves = NotificacionesDefaults.Keys.ToList();
        var existentes = await _db.ParametrosHotel
            .Where(p => claves.Contains(p.Clave))
            .ToListAsync();

        var map = existentes.ToDictionary(p => p.Clave, p => p.Valor);

        return new NotificacionesParamsDto
        {
            EmailNotificaciones = map.GetValueOrDefault("notif_email", NotificacionesDefaults["notif_email"]),
            NotificarCheckin = map.GetValueOrDefault("notif_checkin", NotificacionesDefaults["notif_checkin"]),
            NotificarCheckout = map.GetValueOrDefault("notif_checkout", NotificacionesDefaults["notif_checkout"]),
            NotificarIncidentes = map.GetValueOrDefault("notif_incidentes", NotificacionesDefaults["notif_incidentes"]),
            NotificarStockBajo = map.GetValueOrDefault("notif_stock_bajo", NotificacionesDefaults["notif_stock_bajo"]),
        };
    }

    public async Task UpdateNotificacionesParamsAsync(NotificacionesParamsUpdateDto dto)
    {
        var props = typeof(NotificacionesParamsUpdateDto).GetProperties()
            .Where(p => p.GetValue(dto) != null)
            .ToList();

        foreach (var prop in props)
        {
            var dbKey = NotificacionesDtoKeyToDbKey[prop.Name];
            var valor = (string)prop.GetValue(dto)!;

            var existente = await _db.ParametrosHotel
                .FirstOrDefaultAsync(p => p.Clave == dbKey);

            if (existente != null)
            {
                existente.Valor = valor;
                existente.FechaActualizacion = DateTime.UtcNow;
            }
            else
            {
                _db.ParametrosHotel.Add(new ParametroHotel
                {
                    Clave = dbKey,
                    Valor = valor,
                    Descripcion = "Parámetro de notificaciones",
                    FechaActualizacion = DateTime.UtcNow,
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<DepositoGarantiaParamsDto> GetDepositoGarantiaParamsAsync()
    {
        var claves = DepositoGarantiaDefaults.Keys.ToList();
        var existentes = await _db.ParametrosHotel
            .Where(p => claves.Contains(p.Clave))
            .ToListAsync();

        var map = existentes.ToDictionary(p => p.Clave, p => p.Valor);

        return new DepositoGarantiaParamsDto
        {
            DepositoHabilitado = map.GetValueOrDefault("deposito_garantia_habilitado", DepositoGarantiaDefaults["deposito_garantia_habilitado"]),
            DepositoMonto = map.GetValueOrDefault("deposito_garantia_monto", DepositoGarantiaDefaults["deposito_garantia_monto"]),
            DepositoPorcentaje = map.GetValueOrDefault("deposito_garantia_porcentaje", DepositoGarantiaDefaults["deposito_garantia_porcentaje"]),
        };
    }

    public async Task UpdateDepositoGarantiaParamsAsync(DepositoGarantiaParamsUpdateDto dto)
    {
        var props = typeof(DepositoGarantiaParamsUpdateDto).GetProperties()
            .Where(p => p.GetValue(dto) != null)
            .ToList();

        foreach (var prop in props)
        {
            var dbKey = DepositoGarantiaDtoKeyToDbKey[prop.Name];
            var valor = (string)prop.GetValue(dto)!;

            var existente = await _db.ParametrosHotel
                .FirstOrDefaultAsync(p => p.Clave == dbKey);

            if (existente != null)
            {
                existente.Valor = valor;
                existente.FechaActualizacion = DateTime.UtcNow;
            }
            else
            {
                _db.ParametrosHotel.Add(new ParametroHotel
                {
                    Clave = dbKey,
                    Valor = valor,
                    Descripcion = "Parámetro de depósito de garantía",
                    FechaActualizacion = DateTime.UtcNow,
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<EarlyCheckinParamsDto> GetEarlyCheckinParamsAsync()
    {
        var claves = EarlyCheckinDefaults.Keys.ToList();
        var existentes = await _db.ParametrosHotel
            .Where(p => claves.Contains(p.Clave))
            .ToListAsync();

        var map = existentes.ToDictionary(p => p.Clave, p => p.Valor);

        return new EarlyCheckinParamsDto
        {
            EarlyCheckinHoraLimite = map.GetValueOrDefault("early_checkin_hora_limite", EarlyCheckinDefaults["early_checkin_hora_limite"]),
            EarlyCheckinCargo = map.GetValueOrDefault("early_checkin_cargo", EarlyCheckinDefaults["early_checkin_cargo"]),
        };
    }

    public async Task UpdateEarlyCheckinParamsAsync(EarlyCheckinParamsUpdateDto dto)
    {
        var props = typeof(EarlyCheckinParamsUpdateDto).GetProperties()
            .Where(p => p.GetValue(dto) != null)
            .ToList();

        foreach (var prop in props)
        {
            var dbKey = EarlyCheckinDtoKeyToDbKey[prop.Name];
            var valor = (string)prop.GetValue(dto)!;

            var existente = await _db.ParametrosHotel
                .FirstOrDefaultAsync(p => p.Clave == dbKey);

            if (existente != null)
            {
                existente.Valor = valor;
                existente.FechaActualizacion = DateTime.UtcNow;
            }
            else
            {
                _db.ParametrosHotel.Add(new ParametroHotel
                {
                    Clave = dbKey,
                    Valor = valor,
                    Descripcion = "Parámetro de early check-in",
                    FechaActualizacion = DateTime.UtcNow,
                });
            }
        }

        await _db.SaveChangesAsync();
    }
}
