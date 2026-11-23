# Envelhecer Bem

Diário de hábitos e bem-estar para adultos 45+: sono, passos, água, humor, medicamentos e consultas, com um painel de acompanhamento.

> **Projeto de estudo e portfólio.** Todos os dados são **fictícios**. Não é um dispositivo médico e não substitui acompanhamento profissional de saúde.

## Funcionalidades

- **Login** com JWT (usuário de demonstração).
- **Registro diário:** sono, passos, copos de água, humor (1 a 5) e observação. Um registro por dia; salvar de novo substitui o do mesmo dia.
- **Painel:** sequência de dias, médias do período, gráfico de passos e lista dos últimos registros.
- **Lembretes** de medicamentos e consultas (criar e remover).

## Stack

- **Frontend:** Angular 22 (standalone components, signals, formulários reativos, guards, interceptor JWT), Vitest, servido por nginx em produção.
- **Backend:** .NET 8 Web API, JWT Bearer, EF Core com SQL Server, xUnit com `WebApplicationFactory`.
- **Infraestrutura:** Docker Compose (SQL Server 2022, API e front).

## Estrutura

```
envelhecer-bem/
  api/                 # EnvelhecerBem.sln (src/EnvelhecerBem.Api, tests/EnvelhecerBem.Api.Tests)
    Dockerfile
  web/                 # Aplicação Angular
    Dockerfile
    nginx.conf
  docker-compose.yml   # db + api + web
  .env.example         # variáveis necessárias (copie para .env)
```

## Como rodar com Docker (recomendado)

Pré-requisito: Docker Desktop (ou Docker Engine com o plugin Compose).

```powershell
cd envelhecer-bem
copy .env.example .env
# edite o .env e preencha JWT_KEY com pelo menos 32 caracteres
docker compose up -d --build
```

Depois, acesse:

| Serviço | Endereço |
|---|---|
| Front | http://localhost:4200 |
| API | http://localhost:5087 |
| SQL Server | `localhost,14333` (usuário `sa`) |

Na primeira execução, a API aplica as migrations e cria 90 dias de registros fictícios e três lembretes.

Para parar: `docker compose down`. Para apagar também o banco: `docker compose down -v`.

Login de demonstração: `maria@exemplo.com` / `Demo@123`.

## Como rodar em desenvolvimento

Use este fluxo para alterar código com recarga automática. O banco continua no Docker.

**1. Banco de dados:**

```powershell
docker compose up -d db
```

**2. API** (porta 5087, perfil `http`):

```powershell
cd api
dotnet test
dotnet run --project src/EnvelhecerBem.Api --launch-profile http
```

A string de conexão de desenvolvimento fica em `appsettings.Development.json` e aponta para `localhost,14333`.

**3. Web** (porta 4200):

```powershell
cd web
npm install
npm start
```

## Testes

```powershell
cd api
dotnet test
```

Os testes de integração usam SQLite em memória com o mesmo modelo do EF Core, então não dependem do Docker.

Para o front:

```powershell
cd web
npx ng test --watch=false
```

## Configuração

| Variável | Onde | Para que serve |
|---|---|---|
| `JWT_KEY` | `.env` (compose) | Chave de assinatura dos tokens. Obrigatória, com pelo menos 32 caracteres. |
| `MSSQL_SA_PASSWORD` | `.env` (compose) | Senha do SQL Server. O padrão é só para desenvolvimento. |
| `ConnectionStrings__Default` | ambiente da API | String de conexão. No compose, já aponta para o serviço `db`. |
| `Jwt__Key` | ambiente da API | Mesma chave do `JWT_KEY`, lida pela API. |

Nunca commite o `.env` nem chaves reais. O `.gitignore` já ignora o arquivo.

## Identidade visual

Paleta da Vortex Software, extraída de [vortexsoftware.tech](https://www.vortexsoftware.tech/): primária `#c03`, rosa `#ef5274`, magenta `#ae1269`, com o gradiente horizontal e os cards neumórficos do estilo de referência. As cores estão em variáveis CSS em `web/src/styles.scss`.

## Próximos passos

- Endereço da API configurável por ambiente, para a VPS
- Deploy em VPS com HTTPS
- Pipeline de CI no GitHub Actions (testes e build das imagens)
- Edição de registros e lembretes
