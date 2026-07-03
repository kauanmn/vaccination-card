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
