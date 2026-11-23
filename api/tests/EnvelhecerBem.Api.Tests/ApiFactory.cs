using EnvelhecerBem.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnvelhecerBem.Api.Tests;

/// <summary>Sobe a API com SQLite em memória no lugar do SQL Server.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    // A conexão precisa ficar aberta enquanto a fábrica viver, senão o banco em memória some
    private readonly SqliteConnection _conexao = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            _conexao.Open();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conexao));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _conexao.Dispose();
        base.Dispose(disposing);
    }
}
