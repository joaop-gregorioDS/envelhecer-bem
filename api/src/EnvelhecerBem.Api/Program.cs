using System.Text;
using EnvelhecerBem.Api.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- Opções JWT (chave real só via variável de ambiente / secrets na VPS) ----
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Seção 'Jwt' não configurada.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
    throw new InvalidOperationException("Jwt:Key ausente ou com menos de 32 caracteres. Defina via variável de ambiente Jwt__Key.");

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<TokenService>();
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

builder.Services.AddDbContext<EnvelhecerBem.Api.Data.AppDbContext>(o => o.UseSqlServer(connectionString));
builder.Services.AddScoped<EnvelhecerBem.Api.Registros.RegistroStore>();
builder.Services.AddScoped<EnvelhecerBem.Api.Lembretes.LembreteStore>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS para o front Angular em desenvolvimento
const string FrontCors = "FrontEnd";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontCors, policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"])
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EnvelhecerBem.Api.Data.AppDbContext>();
    await EnvelhecerBem.Api.Data.DbInitializer.InicializarAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(FrontCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Necessário para os testes de integração referenciarem a aplicação
public partial class Program { }
