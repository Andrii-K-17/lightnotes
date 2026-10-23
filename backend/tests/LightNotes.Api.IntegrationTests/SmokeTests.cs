using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LightNotes.Api.IntegrationTests;

public class SmokeTests
{
    [Fact]
    public async Task Liveness_endpoint_responds()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=unused");
            builder.UseSetting("Jwt:SigningKey", new string('k', 32));
        });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
