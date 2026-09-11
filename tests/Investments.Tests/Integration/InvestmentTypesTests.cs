using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Investments.Application.DTOs;

namespace Investments.Tests.Integration;

/// <summary>Cobre a manutenção dos tipos de investimento em /investment-types.</summary>
public class InvestmentTypesTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly ApiFactory _factory;
    public InvestmentTypesTests(ApiFactory factory) => _factory = factory;

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"types-{Guid.NewGuid():N}@teste.com";

        await client.PostAsJsonAsync("/users", new RegisterUserRequest("Tipos", email, "Senha@123"));
        var auth = await client.PostAsJsonAsync("/auth", new AuthRequest(email, "Senha@123"));
        var body = (await auth.Content.ReadFromJsonAsync<AuthResponse>(Json))!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);
        return client;
    }

    [Fact]
    public async Task Sem_token_deve_retornar_401()
    {
        var client = _factory.CreateClient();

        (await client.GetAsync("/investment-types")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.PostAsJsonAsync("/investment-types", new InvestmentTypeRequest("X")))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.DeleteAsync("/investment-types/1")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Deve_listar_os_cinco_tipos_do_seed()
    {
        var client = await CreateAuthenticatedClientAsync();

        var types = await client.GetFromJsonAsync<List<InvestmentTypeResponse>>("/investment-types", Json);

        types.Should().NotBeNull();
        types!.Select(t => t.Name).Should().Contain(new[] { "Acoes", "RendaFixa", "Fundos", "Tesouro", "Cripto" });
    }

    [Fact]
    public async Task Fluxo_completo_de_CRUD_de_tipos()
    {
        var client = await CreateAuthenticatedClientAsync();
        var nome = $"Fiis-{Guid.NewGuid():N}"[..20];

        // Create
        var create = await client.PostAsJsonAsync("/investment-types", new InvestmentTypeRequest(nome));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await create.Content.ReadFromJsonAsync<InvestmentTypeResponse>(Json))!;
        created.Id.Should().BeGreaterThan(0);

        // O Location precisa apontar para uma rota que existe.
        (await client.GetAsync(create.Headers.Location)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Nome duplicado
        (await client.PostAsJsonAsync("/investment-types", new InvestmentTypeRequest(nome)))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Update
        var update = await client.PutAsJsonAsync($"/investment-types/{created.Id}",
            new InvestmentTypeRequest(nome + "-Renomeado"));
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await update.Content.ReadFromJsonAsync<InvestmentTypeResponse>(Json))!;
        updated.Name.Should().Be(nome + "-Renomeado");
        updated.UpdatedAt.Should().NotBeNull();

        // O tipo recém-criado já pode ser usado em um investimento
        var inv = await client.PostAsJsonAsync("/investments",
            new InvestmentRequest(updated.Name, 100m, DateTime.UtcNow.AddDays(-1), "novo tipo"));
        inv.StatusCode.Should().Be(HttpStatusCode.Created);
        var investimento = (await inv.Content.ReadFromJsonAsync<InvestmentResponse>(Json))!;
        investimento.TypeId.Should().Be(created.Id);

        // Enquanto o investimento existir, o tipo está em uso e não pode ser removido.
        (await client.DeleteAsync($"/investment-types/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await client.DeleteAsync($"/investments/{investimento.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Delete
        (await client.DeleteAsync($"/investment-types/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.DeleteAsync($"/investment-types/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Nao_deve_remover_tipo_em_uso()
    {
        var client = await CreateAuthenticatedClientAsync();
        var nome = $"EmUso-{Guid.NewGuid():N}"[..20];

        var created = (await (await client.PostAsJsonAsync("/investment-types", new InvestmentTypeRequest(nome)))
            .Content.ReadFromJsonAsync<InvestmentTypeResponse>(Json))!;

        var inv = await client.PostAsJsonAsync("/investments",
            new InvestmentRequest(nome, 500m, DateTime.UtcNow.AddDays(-1), null));
        inv.StatusCode.Should().Be(HttpStatusCode.Created);

        var delete = await client.DeleteAsync($"/investment-types/{created.Id}");

        delete.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Nome_vazio_deve_retornar_400()
    {
        var client = await CreateAuthenticatedClientAsync();

        (await client.PostAsJsonAsync("/investment-types", new InvestmentTypeRequest("")))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Tipo_inexistente_no_investimento_deve_retornar_400_com_os_disponiveis()
    {
        var client = await CreateAuthenticatedClientAsync();

        var resp = await client.PostAsJsonAsync("/investments",
            new InvestmentRequest("NaoExiste", 10m, DateTime.UtcNow.AddDays(-1), null));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("NaoExiste").And.Contain("Acoes");
    }

    [Fact]
    public async Task Get_de_tipo_inexistente_deve_retornar_404()
    {
        var client = await CreateAuthenticatedClientAsync();

        (await client.GetAsync("/investment-types/999999")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
