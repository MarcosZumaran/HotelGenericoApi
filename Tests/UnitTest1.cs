using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Moq;
using HotelGenericoApi.Data;
using HotelGenericoApi.Models;
using HotelGenericoApi.Services.Implementations;
using HotelGenericoApi.Services.Interfaces;
using Xunit;

namespace HotelGenericoApi.Tests;

public class EstanciaServiceTests
{
    private HotelDbContext CreateContext() => TestDbContextFactory.Create();

    private static ILogger<T> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>().Object;
    }

    private CheckinService CreateCheckinService(HotelDbContext db)
    {
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
        var hubMock = new Mock<IHubContext<HotelGenericoApi.Hubs.HabitacionHub>>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        var amenidadMock = new Mock<IAmenidadService>();
        var reservaCorpMock = new Mock<IReservaCorporativaService>();
        var paramHotelMock = new Mock<IParametroHotelService>();
        paramHotelMock.Setup(p => p.GetDepositoGarantiaParamsAsync()).ReturnsAsync(new DTOs.Response.DepositoGarantiaParamsDto());
        paramHotelMock.Setup(p => p.GetEarlyCheckinParamsAsync()).ReturnsAsync(new DTOs.Response.EarlyCheckinParamsDto());
        return new CheckinService(db, CreateMockLogger<CheckinService>(), hubMock.Object, amenidadMock.Object, reservaCorpMock.Object, paramHotelMock.Object);
    }

    private CheckoutService CreateCheckoutService(HotelDbContext db)
    {
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
        var hubMock = new Mock<IHubContext<HotelGenericoApi.Hubs.HabitacionHub>>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        var configCacheMock = new Mock<IConfiguracionCacheService>();
        var paramHotelMock = new Mock<IParametroHotelService>();
        paramHotelMock.Setup(p => p.GetCheckoutParamsAsync()).ReturnsAsync(new DTOs.Response.CheckoutParamsDto());
        return new CheckoutService(db, CreateMockLogger<CheckoutService>(), hubMock.Object, configCacheMock.Object, paramHotelMock.Object);
    }

    private ConsumoEstanciaService CreateConsumoService(HotelDbContext db)
    {
        return new ConsumoEstanciaService(db, CreateMockLogger<ConsumoEstanciaService>());
    }

    [Fact]
    public async Task Checkin_ConDto_CreaEstanciaCorrectamente()
    {
        var db = CreateContext();
        var service = CreateCheckinService(db);

        var dto = new DTOs.Request.CheckinCreateDto
        {
            IdHabitacion = 1,
            FechaCheckoutPrevista = DateTime.UtcNow.AddDays(2),
            TipoDocumento = "1",
            Documento = "61077298",
            Nombres = "Test",
            Apellidos = "Cliente",
            IdClienteExistente = 1
        };

        var result = await service.CheckinAsync(dto, 1);

        Assert.NotNull(result);
        Assert.Equal(1, result.IdHabitacion);
        Assert.Equal(2, result.IdEstadoEstancia);
    }

    [Fact]
    public async Task Checkout_EstanciaActiva_FinalizaCorrectamente()
    {
        var db = CreateContext();
        var checkinService = CreateCheckinService(db);
        var checkoutService = CreateCheckoutService(db);

        var dto = new DTOs.Request.CheckinCreateDto
        {
            IdHabitacion = 1,
            FechaCheckoutPrevista = DateTime.UtcNow.AddDays(2),
            TipoDocumento = "1",
            Documento = "61077298",
            Nombres = "Test",
            Apellidos = "Cliente",
            IdClienteExistente = 1
        };

        var creada = await checkinService.CheckinAsync(dto, 1);

        var result = await checkoutService.RealizarCheckoutAsync(creada.IdEstancia, 1);
        Assert.NotNull(result);
        Assert.True(result.TotalFinal >= 0);
    }

    [Fact]
    public async Task AddConsumo_EstanciaExistente_AgregaCorrectamente()
    {
        var db = CreateContext();
        var checkinService = CreateCheckinService(db);
        var consumoService = CreateConsumoService(db);

        var dto = new DTOs.Request.CheckinCreateDto
        {
            IdHabitacion = 1,
            FechaCheckoutPrevista = DateTime.UtcNow.AddDays(2),
            TipoDocumento = "1",
            Documento = "61077298",
            Nombres = "Test",
            Apellidos = "Cliente",
            IdClienteExistente = 1
        };

        var creada = await checkinService.CheckinAsync(dto, 1);

        var item = new ItemEstancia
        {
            IdProducto = 1,
            Cantidad = 2,
            PrecioUnitario = 10.5m
        };

        var result = await consumoService.AddConsumoAsync(creada.IdEstancia, item);
        Assert.True(result);
    }

    [Fact]
    public async Task Transicion_Disponible_Ocupada_EsValida()
    {
        var db = TestDbContextFactory.Create();
        var validador = new ValidadorEstadoService(db);
        Assert.True(await validador.EsTransicionValidaAsync(1, 2));
    }

    [Fact]
    public async Task Transicion_Mantenimiento_Ocupada_NoPermitida()
    {
        var db = TestDbContextFactory.Create();
        var validador = new ValidadorEstadoService(db);
        Assert.False(await validador.EsTransicionValidaAsync(4, 5));
    }
}
