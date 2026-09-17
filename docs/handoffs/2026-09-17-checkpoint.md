# Checkpoint — 17/09/2026

> **Tipo:** handoff de encerramento de expediente — registro do estado exato do projeto para retomada.
> **Criado em:** 2026-09-17
> **Repositório:** `DashboardSB/Dashboard` (branch `main`)
> **Instrução:** ler este documento integralmente antes de qualquer ação na próxima sessão.

---

## Estado do projeto

DashboardSB em **.NET 10 / Razor Pages / Bootstrap 5**.

- Backend: ASP.NET Core 10.0 (Razor Pages)
- Apresentação: Razor Pages renderizadas no servidor + Bootstrap 5.3.3 **local** (sem CDN)
- Acesso a dados: SQL Server, **somente leitura** (ADR-003, alvo revisado por ADR-008)
- Nenhuma alteração de banco/schema em nenhum momento.

---

## Arquitetura vigente

- `Dashboard.Web` = apresentação/composição
- `Dashboard.Core` = contratos, modelos e regras, **sem infraestrutura**
- `Dashboard.Data` = acesso SQL Server
- `Web -> Core`
- `Web -> Data`
- `Data -> Core`
- `Core` **não depende** de infraestrutura (ADR-005).

**Decisões estruturais em vigor:**
- Sem Application Layer, sem CQRS, sem Mediator (ADR-001).
- Regras de negócio residem em `Dashboard.Core` (ADR-007).
- Banco do novo SisacHTML5 é a **única fonte operacional** (ADR-008).
- **Não usar EF Core.**
- Stack de acesso a dados: `Microsoft.Data.SqlClient` + `Dapper` **somente** em `Dashboard.Data`.

---

## Estado do PLAN-001

```
T-01 — Dicionário de dados: CONCLUÍDO
T-02 — Indicadores/PRD: CONCLUÍDO
T-03 — Contrato de dados/acesso somente leitura: CONCLUÍDO COM RESSALVAS
T-04 — Fundação estrutural: CONCLUÍDO/VALIDADO
T-05 — Acesso a dados somente leitura: CONCLUÍDO/VALIDADO
T-06 — Shell do dashboard: CONCLUÍDO/VALIDADO
T-07 — Contratos Core: PRÓXIMA TAREFA, ainda NÃO iniciada
```

---

## T-05 — validação

O teste real de conexão foi executado com sucesso:

- **1 teste**, **0 falhas**;
- conexão SQL validada (`factory_abre_conexao_com_banco_sisac_html5`, executando `SELECT 1`);
- **credencial fornecida por variável de ambiente** (`ConnectionStrings__SisacDatabase`);
- **nenhuma credencial é gravada em arquivo ou commitada** — a senha não aparece neste documento nem em nenhum arquivo do repositório.

Teste localizado em `Dashboard.Data.Tests/Connections/SqlConnectionFactoryTests.cs`.
Infraestrutura: `Dashboard.Data/Connections/ISqlConnectionFactory.cs` + `SqlConnectionFactory.cs`; DI em `Dashboard.Web/Program.cs` (`GetConnectionString("SisacDatabase")`).

---

## T-06 — resultado

- `Dashboard.Web/Pages/Shared/_Layout.cshtml` ajustado (Bootstrap 5, navegação padrão Home/Privacy preservada);
- `Dashboard.Web/Pages/Index.cshtml` ajustado (título, seção e seis placeholders);
- `Dashboard.Web/Pages/Index.cshtml.cs` **não precisou de alteração**;
- **Bootstrap 5.3.3 local** (`wwwroot/lib/bootstrap`) — **sem CDN**;
- layout **responsivo** (1 coluna no celular, 2 no tablet, 3 no desktop);
- **seis placeholders**:
  1. Atendimentos
  2. Consultas
  3. Exames
  4. Faturamento
  5. Produção Médica
  6. Despesas
- **sem números fictícios** (texto "Aguardando dados");
- **sem SQL**;
- **sem Repository**;
- **sem DTO**;
- **sem Service**;
- **sem gráficos reais**;
- **sem filtros**;
- **sem autenticação**;
- **build validado** (`dotnet build Dashboard.slnx` → 0 avisos, 0 erros);
- aplicação subiu e **`GET /` retornou HTTP 200**.

---

## Próximo passo

**T-07 — Contratos Core.**

> **IMPORTANTE:** amanhã, **antes de executar T-07**, devemos ler o conteúdo **ATUAL** do T-07 no `PLAN-001-dashboard-indicadores.md` e **somente então** montar o prompt cirúrgico para o OpenCode.

**NÃO antecipar decisões de T-07 neste checkpoint.** Nenhuma decisão de contrato/DTO foi tomada.

---

## Regras de retomada

- Não reabrir T-03, T-04, T-05 ou T-06 sem motivo concreto.
- Não alterar banco de dados.
- Dashboard continua **somente leitura**.
- **Não usar EF Core.**
- **Não colocar infraestrutura no `Dashboard.Core`.**
- Não avançar tarefas automaticamente.
- Cada tarefa deve passar pelo **gate humano** antes da próxima.
- O OpenCode deve executar **somente o escopo explicitamente autorizado**.

---

## Estado do Git no encerramento

- Branch `main`; commits anteriores: `fa84c93` ("Commit do checkpoint.").
- Este checkpoint consolida no Git o estado de 17/09/2026 (fundação + shell + documentação).
- `git push` **não** foi executado — apenas commit local.
