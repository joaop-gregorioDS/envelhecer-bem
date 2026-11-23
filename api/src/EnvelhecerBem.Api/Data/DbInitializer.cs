using EnvelhecerBem.Api.Lembretes;
using EnvelhecerBem.Api.Registros;
using Microsoft.EntityFrameworkCore;

namespace EnvelhecerBem.Api.Data;

/// <summary>
/// Prepara o banco na inicialização: aplica migrations (SQL Server) ou cria o schema (outros provedores,
/// como SQLite nos testes) e insere dados fictícios quando as tabelas estão vazias.
/// </summary>
public static class DbInitializer
{
    private const int TentativasConexao = 10;

    public static async Task InicializarAsync(AppDbContext db, CancellationToken ct = default)
    {
        await AguardarBancoAsync(db, ct);

        if (db.Database.IsSqlServer())
            await db.Database.MigrateAsync(ct);
        else
            await db.Database.EnsureCreatedAsync(ct);

        if (!await db.Registros.AnyAsync(ct))
            db.Registros.AddRange(GerarRegistrosFicticios(90));

        if (!await db.Lembretes.AnyAsync(ct))
        {
            db.Lembretes.AddRange(
                new Lembrete(Guid.NewGuid(), "medicamento", "Losartana 50 mg",
                    DateTime.UtcNow.Date.AddHours(8), "Tomar com água (dado fictício)"),
                new Lembrete(Guid.NewGuid(), "consulta", "Consulta de rotina - Dra. Exemplo",
                    DateTime.UtcNow.Date.AddDays(10).AddHours(14), null),
                new Lembrete(Guid.NewGuid(), "medicamento", "Vitamina D 2000 UI",
                    DateTime.UtcNow.Date.AddHours(9), null));
        }

        await db.SaveChangesAsync(ct);
    }

    // Em containers, o SQL Server pode levar alguns segundos para aceitar conexões
    private static async Task AguardarBancoAsync(AppDbContext db, CancellationToken ct)
    {
        for (var tentativa = 1; ; tentativa++)
        {
            try
            {
                await db.Database.CanConnectAsync(ct);
                return;
            }
            catch when (tentativa < TentativasConexao)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }
        }
    }

    private static IEnumerable<RegistroDiario> GerarRegistrosFicticios(int dias)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var rnd = new Random(42); // determinístico, para o portfólio ser reproduzível

        for (var i = 0; i < dias; i++)
        {
            yield return new RegistroDiario(
                Data: hoje.AddDays(-i),
                HorasSono: Math.Round(5.5 + rnd.NextDouble() * 3, 1),
                Passos: rnd.Next(2000, 12000),
                CoposDeAgua: rnd.Next(3, 10),
                Humor: rnd.Next(1, 6),
                Observacao: i % 9 == 0 ? "Dado fictício de demonstração" : null);
        }
    }
}
