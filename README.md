# vaccination-card

Sistema de gestão de cartão de vacinação: API REST em .NET + frontend em React.

## Estrutura

| Pasta        | Conteúdo                                                        |
| ------------ | --------------------------------------------------------------- |
| `src/`       | API em .NET (Clean Architecture: Api, Application, Domain, Infrastructure) |
| `frontend/`  | SPA em React + Vite + Tailwind ([documentação](frontend/README.md)) |
| `tests/`     | Testes unitários (xUnit)                                        |
| `docs/`      | Documentação, incluindo o [roteiro de testes manuais](docs/manual-test-plan.md) |

## Como rodar

### Docker (recomendado — um comando)

Sobe API + SPA no mesmo container, servidos em `http://localhost:8080` (Swagger em `/swagger`):

```bash
docker compose up --build
```

Ou sem compose:

```bash
docker build -t vaccination-card .
docker run -p 8080:8080 vaccination-card
```

### Local (sem Docker)

```bash
# API (https://localhost:7077, Swagger em /swagger)
cd src/Api
dotnet run --launch-profile https

# Frontend (http://localhost:5173) — em outro terminal
cd frontend
npm install
npm run dev
```

Login inicial: usuário `admin`, senha `admin123` (hash configurável em `appsettings.json`). Pacientes recebem credenciais geradas no cadastro.

> A API é servida sob o prefixo `/api` (ex.: `/api/auth/login`). Em produção (Docker), o mesmo container serve o SPA na raiz e faz fallback das rotas do React para `index.html`.

## Deploy (live demo)

O `Dockerfile` gera uma imagem única (API + SPA). Configs prontas para dois provedores gratuitos:

- **Render** — `render.yaml` (Blueprint). "New > Blueprint" apontando para o repo. Injeta `PORT` automaticamente; gera `Auth__Jwt__Key`. Sem disco persistente no plano free (banco recriado a cada deploy).
- **Fly.io** — `fly.toml`. `fly launch --no-deploy`, crie o volume (`fly volumes create vacccard_data --size 1 --region gru`), defina o segredo (`fly secrets set Auth__Jwt__Key=...`) e `fly deploy`. Volume em `/app/data` persiste o SQLite.

Variáveis de ambiente úteis (padrão .NET, `__` = `:`):

| Variável | Efeito |
| --- | --- |
| `Auth__Jwt__Key` | Chave de assinatura do JWT (use >= 32 bytes em produção) |
| `ConnectionStrings__DefaultConnection` | Caminho do banco SQLite (padrão `Data Source=/app/data/vaccinationcard.db`) |
| `PORT` | Porta HTTP (Render define; local usa 8080) |
