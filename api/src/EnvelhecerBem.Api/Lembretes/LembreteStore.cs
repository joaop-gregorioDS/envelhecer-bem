using EnvelhecerBem.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EnvelhecerBem.Api.Lembretes;

public sealed class LembreteStore(AppDbContext db)
{
    public Task<List<Lembrete>> ListarAsync(CancellationToken ct = default) =>
        db.Lembretes.AsNoTracking().OrderBy(l => l.DataHoraUtc).ToListAsync(ct);

    public async Task<Lembrete> AdicionarAsync(NovoLembreteRequest req, CancellationToken ct = default)
    {
        var lembrete = new Lembrete(Guid.NewGuid(), req.Tipo, req.Titulo.Trim(), req.DataHoraUtc,
            string.IsNullOrWhiteSpace(req.Observacao) ? null : req.Observacao.Trim());

        db.Lembretes.Add(lembrete);
        await db.SaveChangesAsync(ct);
        return lembrete;
    }

    public async Task<bool> RemoverAsync(Guid id, CancellationToken ct = default) =>
        await db.Lembretes.Where(l => l.Id == id).ExecuteDeleteAsync(ct) > 0;
}
