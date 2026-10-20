using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LightNotes.Api.IntegrationTests;

public class SmokeTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Root_returns_200()
    {
        var response = await factory
            .CreateClient()
            .GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
