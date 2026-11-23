using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnvelhecerBem.Api.Auth;
using EnvelhecerBem.Api.Registros;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EnvelhecerBem.Api.Tests;

public sealed class AuthEndpointsTests(ApiFactory factory)
    : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_com_credenciais_validas_retorna_token()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("maria@exemplo.com", "Demo@123"));

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
    }

    [Fact]
    public async Task Login_com_senha_errada_retorna_401()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("maria@exemplo.com", "errada"));

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Registros_sem_token_retorna_401()
    {
        var resp = await _client.GetAsync("/api/registros");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Registros_com_token_valido_retorna_200()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("maria@exemplo.com", "Demo@123"));
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.Token;

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/registros?dias=7");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var registros = await resp.Content.ReadFromJsonAsync<List<RegistroDiario>>();
        Assert.NotEmpty(registros!);
    }
}
