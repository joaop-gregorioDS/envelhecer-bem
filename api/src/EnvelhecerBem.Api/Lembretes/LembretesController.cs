using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvelhecerBem.Api.Lembretes;

[ApiController]
[Authorize]
[Route("api/lembretes")]
public sealed class LembretesController(LembreteStore store) : ControllerBase
{
    private static readonly string[] TiposValidos = ["medicamento", "consulta"];

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Lembrete>>> Listar(CancellationToken ct = default) =>
        Ok(await store.ListarAsync(ct));

    [HttpPost]
    public async Task<ActionResult<Lembrete>> Criar([FromBody] NovoLembreteRequest req, CancellationToken ct = default)
    {
        if (!TiposValidos.Contains(req.Tipo))
            return ValidationProblem("Tipo deve ser 'medicamento' ou 'consulta'.");
        if (string.IsNullOrWhiteSpace(req.Titulo) || req.Titulo.Length > 120)
            return ValidationProblem("Título é obrigatório (até 120 caracteres).");

        var criado = await store.AdicionarAsync(req, ct);
        return CreatedAtAction(nameof(Listar), criado);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct = default) =>
        await store.RemoverAsync(id, ct) ? NoContent() : NotFound();
}
