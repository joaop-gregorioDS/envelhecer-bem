using EnvelhecerBem.Api.Lembretes;
using EnvelhecerBem.Api.Registros;
using Microsoft.EntityFrameworkCore;

namespace EnvelhecerBem.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RegistroDiario> Registros => Set<RegistroDiario>();
    public DbSet<Lembrete> Lembretes => Set<Lembrete>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegistroDiario>(e =>
        {
            e.ToTable("RegistrosDiarios");
            e.HasKey(r => r.Data);
            e.Property(r => r.Observacao).HasMaxLength(500);
        });

        modelBuilder.Entity<Lembrete>(e =>
        {
            e.ToTable("Lembretes");
            e.HasKey(l => l.Id);
            e.Property(l => l.Tipo).HasMaxLength(20).IsRequired();
            e.Property(l => l.Titulo).HasMaxLength(120).IsRequired();
            e.Property(l => l.Observacao).HasMaxLength(500);
        });
    }
}
