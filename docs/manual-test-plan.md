# Roteiro de Testes Manuais — Vaccination Card API

Roteiro para validar manualmente todas as funcionalidades relevantes da API.

## 0. Preparação

### Subir a aplicação

```bash
cd src/Api
dotnet run --launch-profile https
```

- Base URL (HTTPS): `https://localhost:7077`
- Base URL (HTTP): `http://localhost:5160`
- Swagger UI (só em Development): `https://localhost:7077/swagger` → usa `openapi/v1.json`

> A app usa `UseHttpsRedirection`. Prefira o perfil **https** e use `curl -k` para ignorar o certificado dev. O banco SQLite (`vaccinationcard.db`) é criado com `EnsureCreated()` no start.

> ⚠️ **Vacinas periódicas:** a coluna `TotalDoses` passou a ser anulável (`null` = periódica, sem limite de doses). Como não há migrations, **apague o `vaccinationcard.db` (+ `-shm`/`-wal`) antes de subir** para o schema novo ser recriado. O seed já traz duas periódicas: **Influenza (Gripe)** e **dT (Difteria e Tétano)** (`totalDoses = null`).

### Credenciais

| Ator     | Usuário            | Senha                                                                           |
| -------- | ------------------ | ------------------------------------------------------------------------------- |
| Admin    | `admin`            | `admin123` (definida via hash em `appsettings.json` — confirme se foi alterada) |
| Paciente | gerado no cadastro | gerado no cadastro (retornado **uma única vez** no POST)                        |

### Formato de resposta (envelope)

Sucesso:

```json
{ "success": true, "data": { ... } }
```

Erro:

```json
{
  "success": false,
  "error": {
    "code": "...",
    "message": "...",
    "details": [{ "field": "...", "message": "..." }]
  }
}
```

### Convenção deste roteiro

- `$ADMIN` = token JWT do admin
- `$PAT` = token JWT do paciente
- Guarde IDs retornados (`patientId`, `vaccineId`, `vaccinationId`) para os passos seguintes.

---

## 1. Autenticação (`/auth/login`)

| #   | Cenário             | Passos                                                          | Esperado                                                               |
| --- | ------------------- | --------------------------------------------------------------- | ---------------------------------------------------------------------- |
| 1.1 | Login admin OK      | POST `/auth/login` `{"username":"admin","password":"admin123"}` | 200; `data.token`, `data.role="Admin"`, `data.expiresAt`               |
| 1.2 | Login senha errada  | password inválida                                               | 401 `code=UNAUTHORIZED`                                                |
| 1.3 | Usuário inexistente | username qualquer                                               | 401 `code=UNAUTHORIZED`                                                |
| 1.4 | Campos vazios       | `{"username":"","password":""}`                                 | 400 `code=VALIDATION_ERROR` com `details` (Usuário/Senha obrigatórios) |
| 1.5 | Login paciente OK   | credenciais geradas no passo 3.1                                | 200; `role="Patient"`                                                  |

```bash
curl -k -s -X POST https://localhost:7077/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

Extrair token e exportar:

```bash
ADMIN="<token do 1.1>"
```

---

## 2. Vacinas (`/vaccines`)

| #    | Cenário                    | Auth          | Passos                                                  | Esperado                                                    |
| ---- | -------------------------- | ------------- | ------------------------------------------------------- | ----------------------------------------------------------- |
| 2.1  | Listar vacinas (anônimo)   | nenhuma       | GET `/vaccines`                                         | 200; `data` paginado (`items`, `page`, `pageSize`, `total`) |
| 2.2  | Criar vacina               | Admin         | POST `/vaccines` `{"name":"Hepatite B","totalDoses":3}` | 201; `data.id`, header `Location`                           |
| 2.3  | Criar vacina sem token     | nenhuma       | POST `/vaccines`                                        | 401 `UNAUTHORIZED`                                          |
| 2.4  | Criar vacina como paciente | Patient       | POST `/vaccines`                                        | 403 `FORBIDDEN`                                             |
| 2.5  | Nome vazio                 | Admin         | `{"name":"","totalDoses":2}`                            | 400 `VALIDATION_ERROR`                                      |
| 2.6  | Doses <= 0                 | Admin         | `{"name":"BCG","totalDoses":0}`                         | 400 `VALIDATION_ERROR` (deve ser maior que zero)            |
| 2.7  | Buscar por id              | Patient/Admin | GET `/vaccines/{id}`                                    | 200; dados da vacina                                        |
| 2.8  | Buscar id inexistente      | autenticado   | GET `/vaccines/{guid-aleatorio}`                        | 404 `NOT_FOUND`                                             |
| 2.9  | Buscar por id sem token    | nenhuma       | GET `/vaccines/{id}`                                    | 401 `UNAUTHORIZED`                                          |
| 2.10 | Paginação                  | nenhuma       | GET `/vaccines?page=1&pageSize=1`                       | 200; respeita `pageSize`                                    |
| 2.11 | Criar vacina periódica     | Admin         | `{"name":"Dengue","totalDoses":null}`                   | 201; `data.totalDoses = null`                               |
| 2.12 | Periódica: campo omitido   | Admin         | `{"name":"Raiva"}` (sem `totalDoses`)                   | 201; `data.totalDoses = null` (tratada como periódica)      |
| 2.13 | Listar traz periódicas     | nenhuma       | GET `/vaccines`                                         | 200; Influenza e dT com `totalDoses: null`                  |

```bash
# 2.2 criar vacina (guarde o id → VACCINE)
curl -k -s -X POST https://localhost:7077/vaccines \
  -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d '{"name":"Hepatite B","totalDoses":3}'
```

---

## 3. Pacientes (`/patients`)

| #    | Cenário                      | Auth    | Passos                              | Esperado                                                                       |
| ---- | ---------------------------- | ------- | ----------------------------------- | ------------------------------------------------------------------------------ |
| 3.1  | Criar paciente               | Admin   | POST `/patients` `{"name":"Kauan"}` | 201; `data` com `id`, `name`, `username` (slug), `password` (gerada, 16 chars) |
| 3.2  | Criar paciente sem token     | nenhuma | POST `/patients`                    | 401                                                                            |
| 3.3  | Criar paciente como paciente | Patient | POST `/patients`                    | 403 `FORBIDDEN`                                                                |
| 3.4  | Nome vazio                   | Admin   | `{"name":""}`                       | 400 `VALIDATION_ERROR`                                                         |
| 3.5  | Username único               | Admin   | criar 2 pacientes "Kauan"           | segundo recebe `username` com sufixo numérico (ex.: `kauan2`)                  |
| 3.6  | Listar pacientes             | Admin   | GET `/patients`                     | 200; paginado                                                                  |
| 3.7  | Listar como paciente         | Patient | GET `/patients`                     | 403 `FORBIDDEN`                                                                |
| 3.8  | Listar sem token             | nenhuma | GET `/patients`                     | 401                                                                            |
| 3.9  | Ver próprio paciente         | Patient | GET `/patients/{ownId}`             | 200; `data` com `vaccinations`                                                 |
| 3.10 | Ver paciente de outro        | Patient | GET `/patients/{outroId}`           | 403 `FORBIDDEN`                                                                |
| 3.11 | Admin vê qualquer paciente   | Admin   | GET `/patients/{qualquerId}`        | 200                                                                            |
| 3.12 | Id inexistente (admin)       | Admin   | GET `/patients/{guid-aleatorio}`    | 404 `NOT_FOUND`                                                                |

```bash
# 3.1 criar paciente (guarde id, username, password)
curl -k -s -X POST https://localhost:7077/patients \
  -H "Authorization: Bearer $ADMIN" -H "Content-Type: application/json" \
  -d '{"name":"Kauan"}'

# login como paciente (guarde token → PAT)
curl -k -s -X POST https://localhost:7077/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"<username gerado>","password":"<password gerado>"}'
```

---

## 4. Registro de Vacinação (`POST /patients/{patientId}/vaccinations`)

Regras de negócio (entidade `Patient.AddVaccination`):

- `dose` entre 1 e `totalDoses` da vacina — **vacina periódica (`totalDoses = null`) não tem teto**
- doses **sequenciais** por vacina (1, depois 2, depois 3…) — vale também para periódicas
- **sem duplicar** dose já registrada
- `applicationDate` obrigatória e **não futura**

| #    | Cenário                     | Auth    | Passos                                                   | Esperado                                                    |
| ---- | --------------------------- | ------- | -------------------------------------------------------- | ----------------------------------------------------------- |
| 4.1  | Registrar 1ª dose (próprio) | Patient | body `{vaccineId, dose:1, applicationDate:"2026-01-10"}` | 201; `data` do paciente com `vaccinations` incluindo a nova |
| 4.2  | Admin registra p/ paciente  | Admin   | mesmo endpoint com `patientId` do paciente               | 201                                                         |
| 4.3  | Registrar em outro paciente | Patient | `patientId` de terceiro                                  | 403 `FORBIDDEN`                                             |
| 4.4  | Sem token                   | nenhuma | —                                                        | 401                                                         |
| 4.5  | Dose fora de ordem          | Patient | `dose:3` sem ter 1 e 2                                   | 400 `INVALID_PARAMETERS` (próxima esperada é 1)             |
| 4.6  | Dose duplicada              | Patient | registrar `dose:1` duas vezes                            | 400 `INVALID_PARAMETERS` (dose já registrada)               |
| 4.7  | Dose acima do total         | Patient | `dose:4` p/ vacina de 3 doses                            | 400 `INVALID_PARAMETERS` (entre 1 e 3)                      |
| 4.8  | Dose <= 0                   | Patient | `dose:0`                                                 | 400 `VALIDATION_ERROR`                                      |
| 4.9  | Data futura                 | Patient | `applicationDate:"2030-01-01"`                           | 400 `VALIDATION_ERROR` (não pode ser futura)                |
| 4.10 | Data ausente/`0001-01-01`   | Patient | `applicationDate` default                                | 400 `VALIDATION_ERROR`                                      |
| 4.11 | `vaccineId` vazio           | Patient | `vaccineId:"000...0"`                                    | 400 `VALIDATION_ERROR`                                      |
| 4.12 | Vacina inexistente          | Patient | `vaccineId` aleatório válido                             | 404 `NOT_FOUND` (vacina não encontrada)                     |
| 4.13 | Fluxo completo              | Patient | registrar dose 1 → 2 → 3 em sequência                    | cada uma 201; ao final `vaccinations` tem 3 itens           |
| 4.14 | Periódica sem teto          | Patient | vacina periódica (Influenza): dose 1 → 2 → 3 → 4 → 5      | todas 201; nenhuma barrada por "dose acima do total"        |
| 4.15 | Periódica mantém ordem      | Patient | vacina periódica: `dose:3` sem ter 1 e 2                 | 400 `INVALID_PARAMETERS` (próxima esperada é 1)             |
| 4.16 | Periódica sem duplicar      | Patient | vacina periódica: registrar `dose:1` duas vezes         | 400 `INVALID_PARAMETERS` (dose já registrada)               |
| 4.17 | Periódica dose <= 0         | Patient | vacina periódica: `dose:0`                              | 400 `VALIDATION_ERROR`                                      |

```bash
curl -k -s -X POST https://localhost:7077/patients/$PATIENT_ID/vaccinations \
  -H "Authorization: Bearer $PAT" -H "Content-Type: application/json" \
  -d '{"vaccineId":"'$VACCINE'","dose":1,"applicationDate":"2026-01-10"}'
```

---

## 5. Remoção de Vacinação (`DELETE /patients/{patientId}/vaccinations/{vaccinationId}`)

| #   | Cenário                   | Auth    | Esperado                                        |
| --- | ------------------------- | ------- | ----------------------------------------------- |
| 5.1 | Remover própria vacinação | Patient | 204 No Content                                  |
| 5.2 | Admin remove              | Admin   | 204                                             |
| 5.3 | Remover de outro paciente | Patient | 403 `FORBIDDEN`                                 |
| 5.4 | Sem token                 | nenhuma | 401                                             |
| 5.5 | Confirmar remoção         | Patient | GET `/patients/{id}` não lista mais a vacinação |

> Nota: o `PatientOwnershipFilter` valida apenas a posse do `patientId`. Registrar novamente a mesma dose após remover deve voltar a ser possível (regra de sequência recalcula pelo que restou).

---

## 6. Autorização & Segurança (transversal)

| #   | Cenário                                                                               | Esperado                                                   |
| --- | ------------------------------------------------------------------------------------- | ---------------------------------------------------------- |
| 6.1 | Token ausente em rota protegida                                                       | 401 `UNAUTHORIZED`                                         |
| 6.2 | Token malformado (`Bearer abc`)                                                       | 401                                                        |
| 6.3 | Token expirado                                                                        | 401 (expiração = 60 min, `ExpiryMinutes` em `appsettings`) |
| 6.4 | Paciente acessa rota admin-only (`GET /patients`, `POST /patients`, `POST /vaccines`) | 403 `FORBIDDEN`                                            |
| 6.5 | Paciente acessa recurso de outro paciente                                             | 403 `FORBIDDEN`                                            |
| 6.6 | Admin acessa qualquer recurso                                                         | 200                                                        |
| 6.7 | Rota inexistente                                                                      | 404                                                        |
| 6.8 | CORS: origem permitida (`http://localhost:5173`)                                      | headers CORS presentes na resposta                         |

Teste de expiração rápido: reduza `Auth:Jwt:ExpiryMinutes` para `1` em `appsettings.development.json`, faça login, espere ~90s, use o token → 401.

---

## 7. Checklist de cobertura

- [ ] Login: admin, paciente, credenciais inválidas, validação
- [ ] Vacinas: listar (anon), criar (admin), buscar id, 404, autorização, validação, paginação
- [ ] Vacinas periódicas: criar com `totalDoses` null/omitido, listar traz seed periódico
- [ ] Vacinação periódica: doses ilimitadas, ordem sequencial mantida, sem duplicar, dose <= 0
- [ ] Pacientes: criar (senha gerada + slug único), listar (admin), ver próprio/alheio, autorização
- [ ] Vacinação: registrar (regras de dose/ordem/duplicidade/data), 404 vacina, ownership
- [ ] Remover vacinação: própria, admin, ownership, confirmação
- [ ] Segurança: 401/403 em todas as combinações, expiração, CORS
- [ ] Envelope de resposta (`success`/`data`/`error`) consistente em todos os casos
