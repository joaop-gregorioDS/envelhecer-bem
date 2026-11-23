namespace EnvelhecerBem.Api.Registros;

/// <summary>Registro diário de hábitos de bem-estar (dados fictícios).</summary>
public sealed record RegistroDiario(
    DateOnly Data,
    double HorasSono,
    int Passos,
    int CoposDeAgua,
    int Humor,
    string? Observacao);
