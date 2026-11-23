using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnvelhecerBem.Api.Auth;
using EnvelhecerBem.Api.Lembretes;
using EnvelhecerBem.Api.Registros;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EnvelhecerBem.Api.Tests;

public sealed class RegistrosEPainelTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("maria@exemplo.com", "Demo@123"));
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.Token;

        var cliente = factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    [Fact]
    public async Task Criar_registro_de_hoje_e_listar()
    {
        var cliente = await ClienteAutenticadoAsync();
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        var post = await cliente.PostAsJsonAsync("/api/registros", new
        {
            data = hoje, horasSono = 7.5, passos = 9000, coposDeAgua = 8, humor = 4, observacao = "teste"
        });

        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        var criado = await post.Content.ReadFromJsonAsync<RegistroDiario>();
        Assert.Equal(hoje, criado!.Data);
    }

    [Fact]
    public async Task Criar_registro_com_humor_invalido_retorna_400()
    {
        var cliente = await ClienteAutenticadoAsync();

        var post = await cliente.PostAsJsonAsync("/api/registros", new
        {
            data = DateOnly.FromDateTime(DateTime.UtcNow), horasSono = 7, passos = 1000, coposDeAgua = 2, humor = 9
        });

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
    }

    [Fact]
    public async Task Criar_registro_no_futuro_retorna_400()
    {
        var cliente = await ClienteAutenticadoAsync();

        var post = await cliente.PostAsJsonAsync("/api/registros", new
        {
            data = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
            horasSono = 7, passos = 1000, coposDeAgua = 2, humor = 3
        });

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
    }

    [Fact]
    public async Task Resumo_do_painel_retorna_medias_e_sequencia()
    {
        var cliente = await ClienteAutenticadoAsync();

        var resp = await cliente.GetAsync("/api/painel/resumo?dias=7");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var resumo = await resp.Content.ReadFromJsonAsync<ResumoPainel>();
        Assert.Equal(7, resumo!.Dias);
        Assert.True(resumo.MediaPassos > 0);
        Assert.True(resumo.SequenciaAtual >= 1);
    }

    [Fact]
    public async Task Lembrete_criar_listar_e_remover()
    {
        var cliente = await ClienteAutenticadoAsync();

        var post = await cliente.PostAsJsonAsync("/api/lembretes", new
        {
            tipo = "consulta", titulo = "Teste de consulta", dataHoraUtc = DateTime.UtcNow.AddDays(3), observacao = (string?)null
        });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        var criado = await post.Content.ReadFromJsonAsync<Lembrete>();

        var lista = await cliente.GetFromJsonAsync<List<Lembrete>>("/api/lembretes");
        Assert.Contains(lista!, l => l.Id == criado!.Id);

        var del = await cliente.DeleteAsync($"/api/lembretes/{criado!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
    }

    [Fact]
    public async Task Lembrete_com_tipo_invalido_retorna_400()
    {
        var cliente = await ClienteAutenticadoAsync();

        var post = await cliente.PostAsJsonAsync("/api/lembretes", new
        {
            tipo = "outro", titulo = "x", dataHoraUtc = DateTime.UtcNow
        });

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
    }
}
