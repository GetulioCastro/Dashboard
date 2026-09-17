# T-03 — Acesso SQL Server: investigação SOMENTE LEITURA e preparação da camada de dados

> **Plano de referência:** `docs/plans/PLAN-001-dashboard-indicadores.md`
> **Escopo:** investigação e preparação técnica do acesso do DashboardSB ao SQL Server em modo exclusivamente leitura. **Nenhuma implementação** foi realizada além desta documentação.
> **Data:** 2026-09-17
> **Status:** **Tecnicamente concluído** — conectividade, autenticação, leitura e permissões `dashboard_readonly` (escopo auditado) confirmadas. Divergência de identificação do servidor registrada como **observação** (não bloqueia). Resta validação humana.

---

## 1. Estado atual (projeto)

| Item | Situação encontrada |
|---|---|
| Solução | `Dashboard.slnx` — contém apenas `Dashboard.Web/Dashboard.Web.csproj` |
| Projetos | **`Dashboard.Core` e `Dashboard.Data` NÃO existem** (restrição do AGENTS.md: não criar neste estágio; virá com proposta arquitetural futura / T-04) |
| Target framework | `net10.0`, `Nullable` e `ImplicitUsings` habilitados |
| Pacotes NuGet | **Nenhum** — `Dashboard.Web.csproj` sem `PackageReference` |
| Configurações | `appsettings.json` e `appsettings.Development.json` — apenas `Logging`/`AllowedHosts`/`DetailedErrors`; **sem `ConnectionStrings`** |
| `Program.cs` | Bootstrap mínimo: `AddRazorPages`, pipeline padrão, `MapRazorPages()` — sem serviços de dados |
| Páginas | Scaffold Razor Pages padrão (Index, Privacy, Error) |
| Build | `dotnet build Dashboard.slnx` **com êxito — 0 avisos, 0 erros** (SDK .NET 10.0.401) |
| `.gitignore` | Cobre `.env*`, `node_modules`, `.vscode`, `.opencode`, etc. **Não cobre** padrões de segredos de configuração (ex.: `appsettings.*.local.json`) |
| Git | Branch `main`; `.gitignore` já aparece como **modificado não commitado** (alteração pré-existente, fora desta atividade); `docs/prototype/` e `docs/architecture/contrato-dados-dashboard.md` não rastreados |

**Conclusão:** o projeto está no estado de fundação mínima (Fase 1), sem nenhuma camada de dados. A infraestrutura de acesso ao SQL Server ainda precisa ser definida (é o objeto deste relatório), mas **nenhuma criação de projeto/classes será feita agora** — apenas a proposta será registrada.

---

## 2. Estado do SQL Server

### 2.1. Identificação do ambiente (a partir da documentação)

O `dicionario-de-dados.md` (§origem das descobertas) registra o ambiente legado utilizado na engenharia reversa (T-01):

- Instância: `DESENVHMSISAC0215\MSSQLSERVER2022` *(dicionário)* — servidor físico `DESENVHMSISAC02`
- Banco: `CASAMATER`
- Produto: SQL Server 2022 (Developer)
- Acesso usado naquele momento: consultas de catálogo (`sys.tables`, `sys.columns`, `sys.partitions`), `OBJECT_DEFINITION` de procedures e amostragem `GROUP BY` — todas somente leitura

> **Resolução da divergência (complemento 2026-09-17):** a validação confirmou o uso de `DESENVHMSISAC02\MSSQLSERVER2022` como **instância utilizada no teste atual** (conforme orientação). A relação entre `DESENVHMSISAC02` (nome usado pelo cliente) e `DESENVHMSISAC0215` (identificação retornada pelo SQL Server) está registrada como **observação factual** — ver seção 10 — e **não bloqueia** o T-03.

### 2.2. Testes de conexão realizados nesta atividade

| Comando proposto | Resultado |
|---|---|
| `sqlcmd -S "DESENVHMSISAC02\MSSQLSERVER2022" -d "CASAMATER" -U "dashboard_readonly" -Q "SELECT DB_NAME()..."` | Falha de **logon** — o `sqlcmd` ficou aguardando senha no prompt interativo (não é possível responder por aqui) e o login foi rejeitado com senha vazia |
| `sqlcmd ... -U "dbTeste" ...` | Idem — falha de logon |

**Observação:** a falha de logon indica que o **servidor é alcançável** (a mensagem `Falha de logon do usuário ...` partiu do driver/SQL Server), mas **a autenticação não pôde ser completada** por ausência de credencial válida. **Não foram inventadas credenciais.**

### 2.3. Validações de diagnóstico pendentes (aguardando acesso)

> **Atualização (complemento 2026-09-17):** com a validação executada manualmente no terminal interativo, **conectividade, autenticação, acesso a `CASAMATER`, leitura nas 10 tabelas de interesse e `SELECT TOP 5` em `dbo.ENTRADA` foram confirmados**. Permanecem pendentes apenas: **auditoria das permissões efetivas** do `dashboard_readonly` (sem testar escrita), **confirmação da existência de `SP_ATUALIZA_DASHBOARD`/`SP_PacienteDia`** e a **análise da divergência de identificação do servidor** (seção 10).

Para as pendências, executar **somente** o seguinte, nesta ordem:

1. `SELECT @@VERSION; SELECT DB_NAME(); SELECT SUSER_SNAME();`
2. Catálogo somente leitura:
   - schemas: `SELECT name FROM sys.schemas;`
   - tabelas alvo: `SELECT s.name, t.name FROM sys.tables t JOIN sys.schemas s ON t.schema_id=s.schema_id WHERE t.name IN ('ENTRADA','FATURA','GLOSA','BI_PACIENTEDIA','CONTLEITO','CADMEDICO','ESPECIALIDADE','CADCONVENIO','RECEBER','LOCAL');`
   - colunas/tipos/PK/FK/índices: `sys.columns`, `sys.indexes`, `sys.foreign_keys`
   - procedures: `SELECT name, OBJECT_DEFINITION(OBJECT_ID(name)) FROM sys.procedures WHERE name IN ('SP_ATUALIZA_DASHBOARD','SP_PacienteDia');`
3. Amostras leves com `TOP` (ex.: `SELECT TOP 5 * FROM dbo.ENTRADA;`) — **nunca `SELECT *` sem `TOP`** em tabelas grandes.

> A **existência** de `SP_ATUALIZA_DASHBOARD`/`SP_PacienteDia` é indicativa apenas do ambiente legado (documentada no dicionário e na ADR-007). **Não se presume que o Dashboard as utilize** (ADR-008: banco do SisacHTML5 é a fonte; procedures legadas não são contrato).

---

## 3. Conectividade

- **Validado (complemento 2026-09-17, executado via terminal interativo):**
  - Instância `DESENVHMSISAC02\MSSQLSERVER2022` acessível (teste de rede TCP/1433 → `172.18.100.253`; logon aceito);
  - Banco `CASAMATER` acessível com o usuário `dashboard_readonly` (autenticação **OK**);
  - Acesso às 10 tabelas de interesse e `SELECT TOP 5` em `dbo.ENTRADA` **OK**;
  - SDK `sqlcmd` 16.0.1000.6 disponível na máquina.
- **Permissões (Teste 1):** `dashboard_readonly` atende ao requisito de acesso **somente leitura para o escopo auditado** — `db_datareader=1`, escrita `=0` (`HAS_PERMS_BY_NAME`/`IS_ROLEMEMBER`). Ver seções 4 e 10.4.
- **Procedures:** `SP_ATUALIZA_DASHBOARD` e `SP_PacienteDia` **existem** (`dbo`) — sem uso pelo Dashboard.
- **Divergência de instância:** registrada como observação factual (seção 10.4) — não bloqueia.

---

## 4. Permissões

> **Atualização final (2026-09-17):** **Teste 1 confirmado** — `IsDbDataReader=1`, `IsDbDataWriter/IsDbOwner/IsSysAdmin=0`, `CanSelect=1`, `CanInsert/CanUpdate/CanDelete/CanAlter/CanControl=0`. **Conclusão: a conta `dashboard_readonly` atende ao requisito de acesso somente leitura para o escopo de permissões auditado.**
>
> **Observação registrada (Teste 2, histórico):** a função `fn_my_permissions(NULL, 'DATABASE')` retornou uma lista extensa de nomes de permissões, incluindo nomes de escrita/DDL. Como essa função devolve o rol de permissões *possíveis/nomeadas* no escopo do banco (nem sempre efetivamente concedidas), o Teste 1 (`HAS_PERMS_BY_NAME` + `IS_ROLEMEMBER`, permissões efetivas) é o resultado autoritativo para o escopo auditado. Recomenda-se validação final pelo DBA como confirmação de segurança, **sem testar `INSERT`/`UPDATE`/`DELETE`**.

Perfil verificado do usuário técnico:

- Membro de `db_datareader` no banco de destino (leitura de tabelas/views) — **confirmado**.
- **Sem** `db_datawriter`, `db_owner` ou `sysadmin` — **confirmado**.
- Permissões efetivas de escrita (`INSERT`/`UPDATE`/`DELETE`/`ALTER`/`CONTROL`) = **0** no escopo `DATABASE` — **confirmado**.
- `VIEW DEFINITION` apenas se a leitura de definição de procedures for necessária em diagnóstico (não é requisito de runtime).

### Consulta correta de auditoria de permissões (metadados, somente leitura)

> **Correção da consulta anterior:** a consulta original baseada em `fn_my_permissions(NULL, 'DATABASE')` era **imprecisa** — ela lista apenas permissões no nível do banco e não evidencia o efeito das associações de role patrimoniadas em objetos, podendo subestimar permissões de escrita. A forma correta de auditar **sem testar escrita** é combinar **roles** e **permissões efetivas por objeto**:

```sql
-- Roles e permissões do usuário (0/1) — incluir TODAS as roles relevantes (a primeira auditoria
-- omitiu db_ddladmin, db_securityadmin, db_backupoperator, db_accessadmin, db_sysadmin etc.)
SELECT
    SUSER_SNAME() AS Login,
    DB_NAME()    AS Banco,
    IS_ROLEMEMBER('db_datareader')      AS IsDbDataReader,
    IS_ROLEMEMBER('db_datawriter')      AS IsDbDataWriter,
    IS_ROLEMEMBER('db_ddladmin')        AS IsDbDdlAdmin,
    IS_ROLEMEMBER('db_securityadmin')   AS IsDbSecurityAdmin,
    IS_ROLEMEMBER('db_backupoperator')  AS IsDbBackupOperator,
    IS_ROLEMEMBER('db_accessadmin')     AS IsDbAccessAdmin,
    IS_ROLEMEMBER('db_owner')           AS IsDbOwner,
    IS_SRVROLEMEMBER('sysadmin')        AS IsSysAdmin,
    IS_SRVROLEMEMBER('serveradmin')     AS IsServerAdmin,
    IS_SRVROLEMEMBER('securityadmin')   AS IsSecSrvAdmin,
    IS_SRVROLEMEMBER('dbcreator')       AS IsDbCreator;

-- Todas as roles (banco e servidor) das quais o usuário é membro
SELECT 'DB' AS Nivel, r.name AS Role
FROM sys.database_role_members rm
JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
WHERE rm.member_principal_id = DATABASE_PRINCIPAL_ID(SUSER_SNAME())
UNION ALL
SELECT 'SRV' AS Nivel, r.name
FROM sys.server_role_members rm
JOIN sys.server_principals r ON r.principal_id = rm.role_principal_id
WHERE rm.member_principal_id = SUSER_ID(SUSER_SNAME())
ORDER BY Nivel, Role;

-- Permissões efetivas por objeto (somente leitura — fn_my_permissions retorna apenas permission_name)
SELECT
    OBJECT_SCHEMA_NAME(p.major_id) AS [Schema],
    OBJECT_NAME(p.major_id)        AS [Objeto],
    p.permission_name
FROM sys.fn_my_permissions(NULL, 'OBJECT') p
ORDER BY [Schema], [Objeto], p.permission_name;

-- Permissões de DDL no nível do banco (somente leitura)
SELECT permission_name
FROM sys.fn_my_permissions(NULL, 'DATABASE')
ORDER BY permission_name;
```

> Se uma auditoria futura revelar qualquer permissão de escrita (`INSERT`/`UPDATE`/`DELETE`/DDL/`CONTROL`), **não corrigir** — registrar que a conta não atende ao requisito e recomendar ajuste pelo responsável pelo banco.

---

## 5. Tecnologia escolhida (recomendação)

**Recomendação: `Microsoft.Data.SqlClient` (ADO.NET) + Dapper, isolados na camada `Dashboard.Data`.**

| Critério | `Microsoft.Data.SqlClient` puro | **Dapper** | Entity Framework Core |
|---|---|---|---|
| Controle sobre SQL | Total (SQL manual) | Total (SQL manual) | Parcial (LINQ/modelo) |
| Adequação a dashboard read-only com SQL específico | Ótima | **Ótima** (micro-ORM, consultas por streaming/leitura) | Baixa (modelagem + tracking desnecessários) |
| Performance | Alta | **Alta** (overhead mínimo sobre ADO.NET) | Média/baixa (tradução LINQ, tracking) |
| Simplicidade | Média (muito boilerplate de `SqlConnection`/`SqlDataReader`) | **Alta** (`Query<T>` mapeia direto para DTOs) | Média (config de DbContext) |
| Dependências | Nenhuma extra | Pacote `Dapper` + `Microsoft.Data.SqlClient` | Pacote pesado + providers + migrations |
| Aderência à arquitetura (ADR-001/005) | Sim | **Sim** (dependência só em `Dashboard.Data`; `Dashboard.Core` continua sem dependência) | Sim, mas incentivaria model mapping não desejado (schema ainda em definição) |
| Riscos | Verbosidade e risco de inconsistência manual | Minimo; SQL permanece explícito e parametrizado | **Risco**: migrations/`EnsureCreated` ameaçam a regra "nunca alterar o banco"; tracking pode gerar escrita acidental — proibido (ADR-003) |

**Justificativa da escolha (Dapper + SqlClient):**
1. O projeto é **100% leitura** com consultas SQL específicas e sensíveis a performance (tabelas de milhões de linhas — ENTRADA/FATURA/GLOSA) — Dapper executa SQL explícito com overhead mínimo.
2. **Regra de negócio não vive no SQL** (ADR-007): o SQL em `Dashboard.Data` é apenas filtro/join/agregação técnica; Dapper não incentiva lógica no mapeamento.
3. **Core permanece puro** (ADR-005): as interfaces (`IRepository`) ficam em `Dashboard.Core`; a dependência `Dapper`/`SqlClient` fica apenas em `Dashboard.Data`.
4. **Evita migrations/`EnsureCreated`** do EF Core — alinhado ao requisito de **nunca** alterar o banco.
5. Sem necessidade de change tracking, state management ou modelagem antecipada de todo o schema.

**Nota:** nenhum pacote foi instalado nesta etapa. **T-05 (2026-09-17):** `Microsoft.Data.SqlClient` 7.0.3 + `Dapper` 2.1.86 instalados em `Dashboard.Data` (somente leitura); `Dashboard.Core` segue sem PackageReference (ADR-005).

---

## 6. Segurança — onde armazenar a connection string

**Proposta (decidida antes da implementação):**

```csharp
builder.Configuration.GetConnectionString("SisacDatabase");
```

- **`appsettings.json` (versionado):** apenas valores **não sensíveis** — ex.: sem `Password`/`User ID`. Não colocar credenciais reais.
- **Desenvolvimento:** **User Secrets** (`dotnet user-secrets set "ConnectionStrings:SisacDatabase" "Server=...;Database=CASAMATER;...;TrustServerCertificate=true"`) — arquivo fora do repositório. Alternativa aceitável: variável de ambiente `ConnectionStrings__SisacDatabase`.
- **Homologação/Produção:** **variáveis de ambiente** ou Secret Manager/cofre da plataforma — nunca no código nem em arquivos versionados.
- **`.gitignore`:** recomendado adicionar padrão para arquivos locais de segredo (ex.: `appsettings.*.local.json`) — **aplicado na T-05 (2026-09-17)**.

**Formato recomendado da connection string (leitura):**

```text
Server=...;Database=CASAMATER;User Id=...;Password=...;TrustServerCertificate=true;Application Name=DashboardSB;
```

Com criptografia/config adicional conforme ambiente. Se a instância usar soquetes nomeados dinâmicos, incluir `Encrypt`/`TrustServerCertificate` conforme política de segurança do time.

---

## 7. Arquitetura — posicionamento do acesso a dados

Respeitando ADR-001 (três camadas) e ADR-005 (Core sem dependências):

```text
Dashboard.Web  (apresentação — Razor Pages)
      │
      ▼ (chama serviços/controllers, sem SQL)
Dashboard.Core  (interfaces + DTOs + regras — SEM SqlConnection/Dapper/EF)
      │
      ▼ (implementação das interfaces)
Dashboard.Data  (Dapper + Microsoft.Data.SqlClient — QUERY SELECT / SOMENTE LEITURA)
      │
      ▼
SQL Server (CASAMATER / SisacHTML5)
```

**Regras de implementação propostas (para T-04/T-05):**
- `Dashboard.Core`: define interfaces (ex.: `IIndicatorRepository` com assinaturas de leitura) e DTOs; **não referencia** `Microsoft.Data.SqlClient`, `Dapper` ou EF.
- `Dashboard.Data`: implementa as interfaces; abre `SqlConnection` com a string de leitura; executa **somente** `SELECT` parametrizados via Dapper; **nenhuma** operação de escrita, DDL ou procedure com side effect.
- `Dashboard.Web`: registra a implementação no DI (`Program.cs`) e consome as interfaces — sem conhecimento de SQL.
- **Garantia de somente leitura:**
  - credencial SQL limitada a `db_datareader` (sem permissões de escrita/DML/DDL);
  - código restrito a `SELECT`;
  - nenhum `INSERT/UPDATE/DELETE/MERGE/DROP/ALTER/CREATE/TRUNCATE`;
  - (opcional) `ApplicationIntent=ReadOnly` se o banco estiver em Availability Group.

---

## 8. Riscos e pontos que exigem decisão humana

| # | Risco / questão | Situação / ação |
|---|---|---|
| 1 | **Permissões efetivas** de `dashboard_readonly` | **RESOLVIDO (2026-09-17):** Teste 1 confirma somente leitura no escopo auditado (`db_datareader=1`; escrita=0). Validação final do DBA recomendada como confirmação (sem testar escrita). |
| 2 | **Divergência de instância** `DESENVHMSISAC02` × `DESENVHMSISAC0215` | **Registrada como OBSERVAÇÃO (não bloqueia):** nome usado pelo cliente `DESENVHMSISAC02\MSSQLSERVER2022`; SQL Server retorna `DESENVHMSISAC0215\MSSQLSERVER2022` (ServerName/MachineName `DESENVHMSISAC0215`; NetBIOS físico `DESENVHMSISAC02`; IsClustered=0). Sem evidência de alias/redirecionamento — deixar documentado. |
| 3 | **Banco de destino** — `CASAMATER` confirmado no teste; confirmar se é o banco do novo SisacHTML5 ou se compartilha dados | Confirmar com o responsável |
| 4 | **Usuário com permissão de escrita** — auditoria (Teste 1) sem escrita no escopo avaliado | Confirmado OK; validação final DBA recomendada |
| 5 | **Procedures legadas** — `SP_ATUALIZA_DASHBOARD` e `SP_PacienteDia` **existem** (`dbo`, confirmado). Não são contrato (ADR-008); não executar | Mantido apenas como registro |
| 6 | **Volume de dados / performance** — tabelas com milhões de linhas | Definir `TOP`/filtros/índices em T-05; tratamento de lentidão (RN-43) |
| 7 | **`Dashboard.Core`/`Dashboard.Data` ainda não criados** (AGENTS.md) | Aprovar gate de fundação (T-04) antes de qualquer criação |

---

## 9. Próximos passos (próximo task)

1. **Validação humana do T-03** e, como confirmação opcional de segurança, validação final do DBA das permissões (Teste 1 já confirma somente leitura no escopo auditado; **sem testar escrita**).
2. **Registrar em documentação do ambiente** a observação da divergência de identificação: conectar via `DESENVHMSISAC02\MSSQLSERVER2022` (nome do cliente); SQL Server responde como `DESENVHMSISAC0215` — **sem conclusão de alias/redirecionamento**.
3. **Aprovar a tecnologia** (Dapper + `Microsoft.Data.SqlClient`) e o modelo de connection string (User Secrets/dev; env/homologação-produção).
4. **Atualizar `.gitignore`** com padrão de arquivos locais de segredo (proposta).
5. Na sequência (após gate humano): **T-04** — criar `Dashboard.Core`/`Dashboard.Data`, referências e DI; **T-05** — acesso a dados operacional. **NÃO executar nesta etapa.**
6. Registrar conclusão T-03 no plano (status/checklist) após a validação humana.

---

## 10. Complemento de validação — acesso confirmado (2026-09-17)

> Preserva o histórico das seções 1–9. Esta seção registra o resultado da validação executada no ambiente interativo pelo operador. **T-03 tecnicamente concluído** após os testes 1–4.

### 10.1. Valores confirmados

| Item | Valor confirmado |
|---|---|
| Instância utilizada no teste | `DESENVHMSISAC02\MSSQLSERVER2022` |
| Banco | `CASAMATER` |
| Usuário | `dashboard_readonly` |
| Método de autenticação | SQL Server (logon com usuário técnico) — **OK** |
| Sucesso da autenticação | **Sim** |
| Acesso às 10 tabelas de interesse | **OK** (ENTRADA, FATURA, GLOSA, BI_PACIENTEDIA, CONTLEITO, CADMEDICO, ESPECIALIDADE, CADCONVENIO, RECEBER, LOCAL) |
| `SELECT` de leitura | **OK** — `SELECT TOP 5` em `dbo.ENTRADA` executado com sucesso |
| Alterações no banco | **Nenhuma** (somente `SELECT` e leitura de catálogo/metadados) |

### 10.2. Pendências — situação final

- **Permissões efetivas** de `dashboard_readonly` — **CONCLUÍDO (Teste 1)**: somente leitura no escopo auditado. Observação (Teste 2): `fn_my_permissions(NULL,'DATABASE')` listou nomes de permissões de escrita/DDL; como o Teste 1 (permissões efetivas via `HAS_PERMS_BY_NAME`/`IS_ROLEMEMBER`) é autoritativo, a validação final do DBA fica apenas como confirmação de segurança.
- **Procedures legadas — confirmadas**: `dbo.SP_PacienteDia` e `dbo.SP_ATUALIZA_DASHBOARD` **existem** no banco (teste 3). **Não são utilizadas/executadas** pelo Dashboard (ADR-008).
- **Divergência de identificação do servidor — RESOLVIDA como OBSERVAÇÃO (não bloqueia)**: registrada factualmente em 10.4.

### 10.3. Notas de segurança

- A senha de `dashboard_readonly` **não foi solicitada, registrada ou exibida** em nenhum arquivo, relatório, terminal compartilhado ou log.
- Nenhuma credencial foi gravada no código ou em `appsettings.*.json`.
- Nenhum objeto/dado do banco foi criado, alterado ou removido. Nenhuma permissão foi alterada.

### 10.4. Resultados dos testes finais (executados no terminal interativo pelo operador)

**Teste 1 — Roles e capabilities do usuário (`IS_ROLEMEMBER` + `HAS_PERMS_BY_NAME` no escopo `DATABASE`):**

| Indicador | Valor |
|---|---|
| `IsDbDataReader` | **1** |
| `IsDbDataWriter` | 0 |
| `IsDbOwner` | 0 |
| `IsSysAdmin` | 0 |
| `CanSelect` | **1** |
| `CanInsert` | 0 |
| `CanUpdate` | 0 |
| `CanDelete` | 0 |
| `CanAlter` | 0 |
| `CanControl` | 0 |

**Teste 2 — `fn_my_permissions(NULL, 'DATABASE')` (histórico):** retornou **lista extensa de nomes de permissões**, incluindo nomes de escrita/DDL (`INSERT`, `UPDATE`, `DELETE`, `ALTER`, `CONTROL`, `CREATE TABLE`, `CREATE VIEW`, `CREATE PROCEDURE`, etc.). ⚠️ **Interpretação:** esta função devolve o rol de permissões *possíveis* no escopo do banco, não apenas as efetivamente aplicadas. O resultado **autoritativo** para o escopo auditado é o **Teste 1** (permissões efetivas via `HAS_PERMS_BY_NAME`/`IS_ROLEMEMBER`): **somente leitura confirmado**. Validação final do DBA recomendada como confirmação de segurança.

**Teste 3 — Procedures legadas (existência apenas, não executadas):**

- `dbo.SP_PacienteDia` — **existe**
- `dbo.SP_ATUALIZA_DASHBOARD` — **existe**

**Teste 4 — Identificação da instância (divergência `DESENVHMSISAC02` × `DESENVHMSISAC0215`):**

| Propriedade | Valor |
|---|---|
| `@@SERVERNAME` | `DESENVHMSISAC0215\MSSQLSERVER2022` |
| `SERVERPROPERTY('ServerName')` | `DESENVHMSISAC0215\MSSQLSERVER2022` |
| `SERVERPROPERTY('MachineName')` | `DESENVHMSISAC0215` |
| `SERVERPROPERTY('ComputerNamePhysicalNetBIOS')` | `DESENVHMSISAC02` |
| `SERVERPROPERTY('InstanceName')` | `MSSQLSERVER2022` |
| `SERVERPROPERTY('IsClustered')` | 0 (instância autônoma, sem Failover Cluster) |
| `SERVERPROPERTY('Edition')` | Developer Edition (64-bit) |

**Interpretação da divergência (registro factual, sem conclusão de alias/redirecionamento):**

- O nome **utilizado pelo cliente** na conexão foi `DESENVHMSISAC02\MSSQLSERVER2022`.
- O SQL Server **retornou como identificação** da instância: `@@SERVERNAME` = `SERVERNAME` do servidor `DESENVHMSISAC0215\MSSQLSERVER2022`, `MachineName` = `DESENVHMSISAC0215`, `ComputerNamePhysicalNetBIOS` = `DESENVHMSISAC02`.
- **Fato registrado:** o nome usado pelo cliente difere do nome pelo qual a instância SQL se identifica. Não há evidência específica que permita concluir que existe "alias" ou "redirecionamento de instância nomeada" — portanto **não se conclui nada além do fato**. `IsClustered=0` (instância autônoma).
- **Decisão de configuração (sem alterar ambiente):** para fins do Dashboard, a instância a conectar permanece **`DESENVHMSISAC02\MSSQLSERVER2022`** (validada operacionalmente). A observação deve constar na documentação do ambiente.

> **Registro:** a divergência entre `DESENVHMSISAC02` (nome cliente) e `DESENVHMSISAC0215` (identificação retornada) é **observação documental**, não bloqueio do T-03. Nenhuma configuração foi alterada.

---

## Critério de sucesso do T-03 (checklist atual)

```text
[x] O Dashboard consegue alcançar o SQL Server?          → Sim: DESENVHMSISAC02\MSSQLSERVER2022 (TCP/1433 OK, logon aceito)
[x] Qual banco será utilizado?                           → CASAMATER (confirmado no teste)
[x] Qual schema?                                         → dbo confirmado (10 tabelas de interesse; catálogo acessível)
[x] Qual usuário/permissão?                              → dashboard_readonly autentica e tem PERFIL SOMENTE LEITURA no escopo auditado (db_datareader=1; escrita=0)
[x] O acesso é realmente somente leitura?                → Confirmado no escopo auditado (Teste 1 autoritativo); validação final do DBA como confirmação opcional
[x] Qual tecnologia C# será utilizada?                   → Dapper + Microsoft.Data.SqlClient (recomendada, aguarda aprovação)
[x] Como a connection string será protegida?             → Definido: User Secrets/dev; variáveis em homologação/produção
[x] Como Dashboard.Data será estruturado?                → Definido: interfaces em Core, implementação Dapper em Data
[x] Decisões humanas dependentes?                        → DIVERGÊNCIA DE INSTÂNCIA = OBSERVAÇÃO (não bloqueia); GATE T-04 para fundação (próximo task, não executado)
```