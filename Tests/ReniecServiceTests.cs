using System.Net;
using System.Text;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using HotelGenericoApi.Services.Implementations;
using HotelGenericoApi.Models.Exceptions;
using Xunit;

namespace HotelGenericoApi.Tests;

public class ReniecServiceTests
{
    private static IConfiguration CreateMockConfiguration()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["VerificaPE:ApiKey"]).Returns("fake-api-key");
        return configMock.Object;
    }

    [Fact]
    public async Task ConsultarDniAsync_HttpRequestException_ThrowsExternalServiceException()
    {
        var mockHttp = new Mock<HttpMessageHandler>();
        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Error de red"));
        var client = new HttpClient(mockHttp.Object) { BaseAddress = new Uri("http://localhost") };
        var logger = new Mock<ILogger<ReniecService>>();
        var service = new ReniecService(client, logger.Object, CreateMockConfiguration());

        await Assert.ThrowsAsync<ExternalServiceException>(() => service.ConsultarDniAsync("12345678"));
    }

    [Fact]
    public async Task ConsultarDniAsync_NotFound_ReturnsNull()
    {
        var mockHttp = new Mock<HttpMessageHandler>();
        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new HttpClient(mockHttp.Object) { BaseAddress = new Uri("http://localhost") };
        var logger = new Mock<ILogger<ReniecService>>();
        var service = new ReniecService(client, logger.Object, CreateMockConfiguration());

        var result = await service.ConsultarDniAsync("99999999");

        Assert.Null(result);
    }

    [Fact]
    public async Task ConsultarDniAsync_Success_ReturnsContent()
    {
        var mockHttp = new Mock<HttpMessageHandler>();
        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"dni\":\"12345678\"}") });
        var client = new HttpClient(mockHttp.Object) { BaseAddress = new Uri("http://localhost") };
        var logger = new Mock<ILogger<ReniecService>>();
        var service = new ReniecService(client, logger.Object, CreateMockConfiguration());

        var result = await service.ConsultarDniAsync("12345678");

        Assert.NotNull(result);
        Assert.Contains("12345678", result);
    }

    [Fact]
    public async Task ConsultarRucAsync_HttpRequestException_ThrowsExternalServiceException()
    {
        var mockHttp = new Mock<HttpMessageHandler>();
        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Error de red"));
        var client = new HttpClient(mockHttp.Object) { BaseAddress = new Uri("http://localhost") };
        var logger = new Mock<ILogger<ReniecService>>();
        var service = new ReniecService(client, logger.Object, CreateMockConfiguration());

        await Assert.ThrowsAsync<ExternalServiceException>(() => service.ConsultarRucAsync("20512002090"));
    }

    [Fact]
    public async Task ConsultarRucAsync_NotFound_ReturnsNull()
    {
        var mockHttp = new Mock<HttpMessageHandler>();
        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new HttpClient(mockHttp.Object) { BaseAddress = new Uri("http://localhost") };
        var logger = new Mock<ILogger<ReniecService>>();
        var service = new ReniecService(client, logger.Object, CreateMockConfiguration());

        var result = await service.ConsultarRucAsync("20512002090");

        Assert.Null(result);
    }

    [Fact]
    public async Task ConsultarRucAsync_Success_ReturnsDto()
    {
        var json = @"
        {
            ""success"": true,
            ""data"": {
                ""ruc"": ""20512002090"",
                ""businessName"": ""MIFARMA S.A.C."",
                ""taxpayerType"": ""PERSONA JURIDICA"",
                ""status"": ""ACTIVO"",
                ""condition"": ""HABIDO"",
                ""address"": ""CAL. VICTOR ALZAMORA NRO 147 URB. SANTA CATALINA"",
                ""source"": ""SUNAT"",
                ""updatedAt"": ""2026-03-18T05:26:54.000Z""
            }
        }";

        var mockHttp = new Mock<HttpMessageHandler>();
        mockHttp.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        var client = new HttpClient(mockHttp.Object) { BaseAddress = new Uri("http://localhost") };
        var logger = new Mock<ILogger<ReniecService>>();
        var service = new ReniecService(client, logger.Object, CreateMockConfiguration());

        var result = await service.ConsultarRucAsync("20512002090");

        Assert.NotNull(result);
        Assert.Equal("20512002090", result.Ruc);
        Assert.Equal("MIFARMA S.A.C.", result.BusinessName);
        Assert.Equal("PERSONA JURIDICA", result.TaxpayerType);
        Assert.Equal("ACTIVO", result.Status);
        Assert.Equal("HABIDO", result.Condition);
        Assert.Equal("CAL. VICTOR ALZAMORA NRO 147 URB. SANTA CATALINA", result.Address);
        Assert.Equal("SUNAT", result.Source);
        Assert.NotNull(result.UpdatedAt);
    }
}
