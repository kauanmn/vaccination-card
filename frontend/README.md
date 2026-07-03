# Frontend — Cartão de Vacinação

SPA em React que consome a API REST do cartão de vacinação. Foco em simplicidade, usabilidade e robustez: poucas dependências, tratamento consistente de erros da API e interface em português.

## Stack

| Tecnologia       | Papel                                             |
| ---------------- | ------------------------------------------------- |
| Vite + React 19  | Build e UI (TypeScript estrito)                   |
| Tailwind CSS v4  | Estilo, via plugin `@tailwindcss/vite`            |
| react-router-dom | Rotas e guards por papel (Admin/Paciente)         |

Sem biblioteca de estado global: a única informação compartilhada é a sessão (JWT), mantida em um `AuthContext` + `localStorage`.

## Como rodar

Pré-requisitos: Node 20+ e a API rodando (`cd src/Api && dotnet run --launch-profile https`).

```bash
cd frontend
npm install
npm run dev        # http://localhost:5173
```

Build de produção: `npm run build` (typecheck + bundle em `dist/`).

### Proxy de desenvolvimento

O frontend chama a API em `/api/*`. O servidor do Vite repassa essas chamadas para `https://localhost:7077` com `secure: false` (ver `vite.config.ts`). Isso evita dois problemas comuns em dev:

- não é preciso confiar no certificado HTTPS de desenvolvimento do ASP.NET;
- não há preflight CORS — o navegador só fala com o Vite.

Para apontar para outra URL (ex.: produção), defina `VITE_API_URL` no build.

## Papéis e fluxos

| Papel        | O que vê                                                                                   |
| ------------ | ------------------------------------------------------------------------------------------ |
| **Admin**    | Lista/cadastro/exclusão de pacientes, credenciais geradas (exibidas uma única vez), cartão de qualquer paciente, cadastro de vacinas |
| **Paciente** | O próprio cartão (registrar/remover doses) e o catálogo de vacinas                          |

O id do paciente logado vem do claim de subject do JWT (decodificado no cliente apenas para navegação — a autorização real é feita pela API).

## Cartão de vacinação

Página central (`/patients/:id`). Cada vacina do catálogo vira um card com:

- chips verdes = doses aplicadas (com data; ✕ remove com confirmação);
- chip tracejado = próxima dose (clique abre o registro já pré-preenchido);
- chips cinza = doses futuras de vacinas com esquema fechado;
- barra de progresso e selo “✓ Completa” para esquemas finitos; selo “Periódica” para vacinas sem teto de doses (`totalDoses = null`).

A regra de “próxima dose” espelha o domínio (`maior dose aplicada + 1`), mas o formulário permite digitar qualquer dose — a validação final é sempre da API, e as mensagens de erro dela (dose fora de ordem, duplicada, data futura…) são exibidas no formulário.

## Estrutura

```
src/
  api/          # funções por recurso (auth, patients, vaccines)
  auth/         # AuthContext: sessão, login/logout, expiração automática
  components/   # Layout, Modal, ConfirmDialog, Toast, Pagination, guards
  lib/          # client HTTP (envelope success/error), tipos, JWT, datas
  pages/        # Login, Pacientes, Vacinas, Cartão, 404
```

Decisões relevantes:

- **Envelope da API**: `lib/api.ts` desembrulha `{ success, data }` e converte `{ success: false, error }` em `ApiError` (código, mensagem e detalhes por campo), consumido pelos formulários.
- **401 → logout**: qualquer 401 com sessão ativa limpa a sessão e redireciona para o login; há também um timer para o instante exato de expiração do token.
- **Datas sem fuso**: `applicationDate` é uma data pura (`yyyy-MM-dd`); formatação manual evita o deslocamento de dia que `new Date(...)` causaria em UTC-3.
- **Senha exibida uma única vez**: o modal de paciente criado destaca o aviso e oferece botões de copiar.
