using System.Net;
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
}
