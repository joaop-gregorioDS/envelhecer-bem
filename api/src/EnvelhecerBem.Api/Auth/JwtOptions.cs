namespace EnvelhecerBem.Api.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "envelhecer-bem";
    public string Audience { get; init; } = "envelhecer-bem-web";

    /// <summary>Chave simétrica (mínimo 32 caracteres). Em produção, vir de variável de ambiente.</summary>
    public string Key { get; init; } = string.Empty;

    public int ExpiresMinutes { get; init; } = 60;
}
