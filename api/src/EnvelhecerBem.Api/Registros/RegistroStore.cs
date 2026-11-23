using EnvelhecerBem.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EnvelhecerBem.Api.Registros;

public sealed class RegistroStore(AppDbContext db)
{
    public async Task<IReadOnlyList<RegistroDiario>> ListarAsync(int dias, DateOnly hoje, CancellationToken ct = default)
    {
        var limite = hoje.AddDays(-(dias - 1));
        return await db.Registros.AsNoTracking()
            .Where(r => r.Data >= limite && r.Data <= hoje)
            .OrderByDescending(r => r.Data)
            .ToListAsync(ct);
    }

    /// <summary>Cria ou substitui o registro do dia.</summary>
    public async Task<RegistroDiario> SalvarAsync(RegistroDiario registro, CancellationToken ct = default)
    {
        var existente = await db.Registros.FindAsync(new object[] { registro.Data }, ct);
        if (existente is null)
            db.Registros.Add(registro);
        else
            db.Entry(existente).CurrentValues.SetValues(registro);

        await db.SaveChangesAsync(ct);
        return registro;
    }

    public async Task<ResumoPainel> ResumoAsync(int dias, DateOnly hoje, CancellationToken ct = default)
    {
        var periodo = await ListarAsync(dias, hoje, ct);
        if (periodo.Count == 0)
            return new ResumoPainel(0, 0, 0, 0, 0, 0);

        // Sequência: dias seguidos com registro, terminando hoje
        var datas = (await db.Registros.AsNoTracking()
                .Where(r => r.Data <= hoje)
                .Select(r => r.Data)
                .ToListAsync(ct))
            .ToHashSet();

        var sequencia = 0;
        for (var dia = hoje; datas.Contains(dia); dia = dia.AddDays(-1))
            sequencia++;

        return new ResumoPainel(
            Dias: periodo.Count,
            MediaSonoHoras: Math.Round(periodo.Average(r => r.HorasSono), 1),
            MediaPassos: (int)Math.Round(periodo.Average(r => r.Passos)),
            MediaCoposDeAgua: Math.Round(periodo.Average(r => r.CoposDeAgua), 1),
            MediaHumor: Math.Round(periodo.Average(r => r.Humor), 1),
            SequenciaAtual: sequencia);
    }
}

public sealed record ResumoPainel(
    int Dias,
    double MediaSonoHoras,
    int MediaPassos,
    double MediaCoposDeAgua,
    double MediaHumor,
    int SequenciaAtual);
