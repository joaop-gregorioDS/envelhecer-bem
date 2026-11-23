using Microsoft.AspNetCore.Mvc;

namespace EnvelhecerBem.Api.Auth;

public sealed record LoginRequest(string Email, string Senha);
public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc, string Nome);

[ApiController]
[Route("api/auth")]
public sealed class AuthController(TokenService tokens) : ControllerBase
{
    // Usuário fictício apenas para demonstração. Substituir por tabela de usuários + hash de senha.
    private static readonly (string Id, string Email, string Senha, string Nome) DemoUser =
        ("1", "maria@exemplo.com", "Demo@123", "Maria (demo)");

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (!string.Equals(request.Email, DemoUser.Email, StringComparison.OrdinalIgnoreCase)
            || request.Senha != DemoUser.Senha)
        {
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
        }

        var (token, expires) = tokens.CreateToken(DemoUser.Id, DemoUser.Email, DemoUser.Nome);
        return Ok(new LoginResponse(token, expires, DemoUser.Nome));
    }
}
