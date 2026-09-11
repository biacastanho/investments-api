using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Investments.Application.DTOs;

namespace Investments.Tests.Integration;

/// <summary>Cobre o fluxo completo: cadastro -> autenticação -> endpoints protegidos.</summary>
public class InvestmentsFlowTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ApiFactory _factory;
    public InvestmentsFlowTests(ApiFactory factory) => _factory = factory;

    private async Task<(HttpClient Client, UserResponse User)> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@teste.com";

        var register = await client.PostAsJsonAsync("/users", new RegisterUserRequest("Usuário Teste", email, "Senha@123"));
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var auth = await client.PostAsJsonAsync("/auth", new AuthRequest(email, "Senha@123"));
        auth.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await auth.Content.ReadFromJsonAsync<AuthResponse>(Json))!;
        body.Token.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);
        return (client, body.User);
    }

    [Fact]
    public async Task Cadastro_duplicado_deve_retornar_409()
    {
        var client = _factory.CreateClient();
        var req = new RegisterUserRequest("Dup", $"dup-{Guid.NewGuid():N}@teste.com", "Senha@123");

        (await client.PostAsJsonAsync("/users", req)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await client.PostAsJsonAsync("/users", req)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Auth_com_senha_errada_deve_retornar_401()
    {
        var client = _factory.CreateClient();
        var email = $"x-{Guid.NewGuid():N}@teste.com";
        await client.PostAsJsonAsync("/users", new RegisterUserRequest("X", email, "Senha@123"));

        var auth = await client.PostAsJsonAsync("/auth", new AuthRequest(email, "errada"));

        auth.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Endpoints_protegidos_sem_token_devem_retornar_401()
    {
        var client = _factory.CreateClient();

        (await client.GetAsync("/investments")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.PostAsJsonAsync("/investments", new { })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.DeleteAsync($"/investments/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Fluxo_completo_de_CRUD_de_investimentos()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        // Lista vazia
        var empty = await client.GetFromJsonAsync<List<InvestmentResponse>>("/investments", Json);
        empty.Should().BeEmpty();

        // Create
        var create = await client.PostAsJsonAsync("/investments",
            new InvestmentRequest("Acoes", 1000m, DateTime.UtcNow.AddDays(-1), "PETR4"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await create.Content.ReadFromJsonAsync<InvestmentResponse>(Json))!;
        created.Amount.Should().Be(1000m);

        // List
        var list = await client.GetFromJsonAsync<List<InvestmentResponse>>("/investments", Json);
        list.Should().ContainSingle(i => i.Id == created.Id);

        // Update
        var update = await client.PutAsJsonAsync($"/investments/{created.Id}",
            new InvestmentRequest("RendaFixa", 2500m, DateTime.UtcNow.AddDays(-2), "CDB"));
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await update.Content.ReadFromJsonAsync<InvestmentResponse>(Json))!;
        updated.Type.Should().Be("RendaFixa");
        updated.Amount.Should().Be(2500m);
        updated.UpdatedAt.Should().NotBeNull();

        // Delete
        (await client.DeleteAsync($"/investments/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.DeleteAsync($"/investments/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Usuario_nao_deve_acessar_investimento_de_outro_usuario()
    {
        var (clientA, _) = await CreateAuthenticatedClientAsync();
        var (clientB, _) = await CreateAuthenticatedClientAsync();

        var create = await clientA.PostAsJsonAsync("/investments",
            new InvestmentRequest("Fundos", 300m, DateTime.UtcNow, null), Json);
        var inv = (await create.Content.ReadFromJsonAsync<InvestmentResponse>(Json))!;

        var listB = await clientB.GetFromJsonAsync<List<InvestmentResponse>>("/investments", Json);
        listB.Should().NotContain(i => i.Id == inv.Id);

        var putB = await clientB.PutAsJsonAsync($"/investments/{inv.Id}",
            new InvestmentRequest("Fundos", 1m, DateTime.UtcNow, null), Json);
        putB.StatusCode.Should().Be(HttpStatusCode.NotFound);

        (await clientB.DeleteAsync($"/investments/{inv.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_sem_data_de_investimento_deve_retornar_400()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var resp = await client.PostAsync("/investments",
            new StringContent("""{"type":"Acoes","amount":100}""", Encoding.UTF8, "application/json"));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_por_id_deve_retornar_investimento_do_dono_e_404_para_outro_usuario()
    {
        var (clientA, _) = await CreateAuthenticatedClientAsync();
        var (clientB, _) = await CreateAuthenticatedClientAsync();

        var create = await clientA.PostAsJsonAsync("/investments",
            new InvestmentRequest("Tesouro", 800m, DateTime.UtcNow.AddDays(-3), "Selic"), Json);
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var inv = (await create.Content.ReadFromJsonAsync<InvestmentResponse>(Json))!;

        // O Location do POST precisa apontar para uma rota que realmente existe.
        var byLocation = await clientA.GetAsync(create.Headers.Location);
        byLocation.StatusCode.Should().Be(HttpStatusCode.OK);

        (await clientA.GetAsync($"/investments/{inv.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await clientB.GetAsync($"/investments/{inv.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_com_valor_invalido_deve_retornar_400()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var resp = await client.PostAsJsonAsync("/investments",
            new InvestmentRequest("Acoes", 0m, DateTime.UtcNow, null), Json);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
