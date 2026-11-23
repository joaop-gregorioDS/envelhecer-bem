using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvelhecerBem.Api.Registros;

[ApiController]
[Authorize]
[Route("api/painel")]
public sealed class PainelController(RegistroStore store) : ControllerBase
{
    [HttpGet("resumo")]
    public async Task<ActionResult<ResumoPainel>> Resumo([FromQuery] int dias = 7, CancellationToken ct = default)
    {
        dias = Math.Clamp(dias, 1, 90);
        return Ok(await store.ResumoAsync(dias, DateOnly.FromDateTime(DateTime.UtcNow), ct));
    }
}
