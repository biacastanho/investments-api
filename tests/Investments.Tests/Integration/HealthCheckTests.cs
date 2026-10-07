using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace Investments.Tests.Integration;

/// <summary>Cobre o endpoint /health usado na verificação após o deploy.</summary>
public class HealthCheckTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public HealthCheckTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Health_deve_responder_sem_autenticacao_com_status_healthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        json.RootElement.GetProperty("checks").GetProperty("database").GetString().Should().Be("Healthy");
        json.RootElement.GetProperty("version").GetString().Should().NotBeNullOrWhiteSpace();
    }
}
