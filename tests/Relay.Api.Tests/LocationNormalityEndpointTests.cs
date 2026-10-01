using Microsoft.AspNetCore.Http;
using Relay.Api.Features.LocationNormality;

namespace Relay.Api.Tests;

public class LocationNormalityEndpointTests
{
    [Fact]
    public async Task GetAsync_RejectsUnsupportedEventType()
    {
        using var database = new LocationNormalityServiceTests.TestDatabase();
        var result = await LocationNormalityEndpoint.GetAsync(
            1,
            "messages",
            database.Service,
            CancellationToken.None);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    [Fact]
    public async Task GetAsync_ReturnsNotFoundForUnknownAccount()
    {
        using var database = new LocationNormalityServiceTests.TestDatabase();
        var result = await LocationNormalityEndpoint.GetAsync(
            404,
            "calls",
            database.Service,
            CancellationToken.None);

        Assert.Equal(
            StatusCodes.Status404NotFound,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    [Fact]
    public async Task GetAsync_ReturnsUnprocessableForAccountWithoutActivity()
    {
        using var database = new LocationNormalityServiceTests.TestDatabase();
        database.AddAccount("UTC");
        await database.SaveAsync();

        var result = await LocationNormalityEndpoint.GetAsync(
            1,
            "calls",
            database.Service,
            CancellationToken.None);

        Assert.Equal(
            StatusCodes.Status422UnprocessableEntity,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }
}
