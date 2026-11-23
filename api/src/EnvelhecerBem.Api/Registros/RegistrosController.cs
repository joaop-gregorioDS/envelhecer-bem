using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvelhecerBem.Api.Registros;

public sealed record NovoRegistroRequest(
    DateOnly Data,
    double HorasSono,
    int Passos,
    int CoposDeAgua,
    int Humor,
    string? Observacao);

[ApiController]
[Authorize]
[Route("api/registros")]
public sealed class RegistrosController(RegistroStore store) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RegistroDiario>>> Listar([FromQuery] int dias = 7, CancellationToken ct = default)
    {
        dias = Math.Clamp(dias, 1, 90);
        return Ok(await store.ListarAsync(dias, Hoje(), ct));
    }

    [HttpPost]
    public async Task<ActionResult<RegistroDiario>> Criar([FromBody] NovoRegistroRequest req, CancellationToken ct = default)
    {
        if (req.HorasSono is < 0 or > 24)
            return ValidationProblem("Horas de sono devem estar entre 0 e 24.");
        if (req.Passos is < 0 or > 100_000)
            return ValidationProblem("Passos devem estar entre 0 e 100.000.");
        if (req.CoposDeAgua is < 0 or > 30)
            return ValidationProblem("Copos de água devem estar entre 0 e 30.");
        if (req.Humor is < 1 or > 5)
            return ValidationProblem("Humor deve estar entre 1 e 5.");
        if (req.Data > Hoje())
            return ValidationProblem("A data não pode estar no futuro.");

        var registro = await store.SalvarAsync(new RegistroDiario(
            req.Data, req.HorasSono, req.Passos, req.CoposDeAgua, req.Humor,
            string.IsNullOrWhiteSpace(req.Observacao) ? null : req.Observacao.Trim()), ct);

        return CreatedAtAction(nameof(Listar), registro);
    }

    private static DateOnly Hoje() => DateOnly.FromDateTime(DateTime.UtcNow);
}
