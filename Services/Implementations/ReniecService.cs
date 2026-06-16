using System.Text.Json;
using Microsoft.Extensions.Logging;
using HotelGenericoApi.DTOs.Response;
using HotelGenericoApi.Services.Interfaces;
using HotelGenericoApi.Models.Exceptions;

namespace HotelGenericoApi.Services.Implementations;

public class ReniecService : IReniecService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReniecService> _logger;
    private readonly string _apiKey;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ReniecService(HttpClient httpClient, ILogger<ReniecService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["VerificaPE:ApiKey"]!;
    }

    public async Task<string?> ConsultarDniAsync(string dni)
    {
        _logger.LogInformation("Consultando RENIEC para DNI {Dni}", dni);

        var url = $"https://api.verificape.com/v2/dni/{dni}";
        _logger.LogDebug("URL RENIEC: {Url}", url);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("RENIEC: DNI {Dni} no encontrado", dni);
                return null;
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Consulta RENIEC exitosa para DNI {Dni}", dni);
            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al consultar RENIEC para DNI {Dni}", dni);
            throw new ExternalServiceException("Error al comunicarse con RENIEC", ex);
        }
    }

    public async Task<ReniecRucResponseDto?> ConsultarRucAsync(string ruc)
    {
        _logger.LogInformation("Consultando SUNAT para RUC {Ruc}", ruc);

        var url = $"https://api.verificape.com/v2/ruc/{ruc}";
        _logger.LogDebug("URL SUNAT: {Url}", url);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("SUNAT: RUC {Ruc} no encontrado", ruc);
                return null;
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Consulta SUNAT exitosa para RUC {Ruc}", ruc);

            var wrapper = JsonSerializer.Deserialize<VerificaRucResponse>(content, _jsonOptions);
            return wrapper?.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al consultar SUNAT para RUC {Ruc}", ruc);
            throw new ExternalServiceException("Error al comunicarse con SUNAT", ex);
        }
    }

    private sealed record VerificaRucResponse(bool Success, ReniecRucResponseDto? Data);
}
