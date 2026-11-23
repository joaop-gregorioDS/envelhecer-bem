namespace EnvelhecerBem.Api.Lembretes;

/// <summary>Lembrete de medicamento ou consulta (dados fictícios).</summary>
public sealed record Lembrete(
    Guid Id,
    string Tipo,        // "medicamento" | "consulta"
    string Titulo,
    DateTime DataHoraUtc,
    string? Observacao);

public sealed record NovoLembreteRequest(
    string Tipo,
    string Titulo,
    DateTime DataHoraUtc,
    string? Observacao);
