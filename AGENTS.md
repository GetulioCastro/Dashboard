# DashboardSB — Instruções para agentes de IA

<!-- leanwork-context:start -->

## Resumo

Dashboard web do Sisac Brasil, módulo do novo **SisacHTML5**. Acessa o banco do SisacHTML5 — única fonte operacional — somente para leitura; a Web atualmente opera com dados demonstrativos. **T-08 ainda não validado.**

## Stack

- **Projetos:** `Dashboard.Web` (apresentação Razor Pages), `Dashboard.Core` (regras/contratos), `Dashboard.Data` (acesso a dados), `Dashboard.Data.Tests` (testes)
- **Backend:** C# / .NET 10 · ASP.NET Core (Razor Pages)
- **Frontend:** Razor Pages (renderização server-side)
- **Banco:** SQL Server
- **Acesso a dados:** Dapper + SqlClient (Microsoft.Data.SqlClient)
- **Testes:** xUnit
- **Infra:** local (dotnet CLI)

## Comandos

```bash
# Build
dotnet build Dashboard.slnx

# Testes
dotnet test Dashboard.slnx

# Rodar local
dotnet run --project Dashboard.Web

# Workflow Leanwork SDD (recuperar estado antes de qualquer ação)
/leanwork-next          # descobre a fase atual e sugere UMA próxima ação
/leanwork-trace         # audita a cadeia ADR → RN → CA → UI → T → R
/leanwork-execute T-XX  # executa uma tarefa do plano
/leanwork-review T-XX   # revisa a entrega contra plano + PRD + arquitetura
/leanwork-start         # inicia pipeline de nova demanda
```

## Convenções

- Arquitetura do pipeline SDD com IDs `ADR-XX`, `RN-XX`, `CA-XX`, `UI-XX`, `T-XX`, `R-XX`
- Testes de cenário nomeados `CA_XX_descricao_do_cenario` (quando existirem)
- Arquivos do pipeline em `docs/`: `architecture/`, `prds/`, `prototype/`, `plans/`, `reviews/`, `handoffs/`, `sdd/`
- Arquitetura em três camadas unidirecionais (Web → Core → Data), sem CQRS/Mediator (ADR-001)
- Regras de negócio no `Dashboard.Core`, sem dependências externas (ADR-005, ADR-007)
- Acesso a dados somente no `Dashboard.Data` (Dapper + SqlClient); Web e Core não consultam o banco (ADR-001)
- Validação humana explícita antes de cada entrega

## Restrições

- SQL Server do SisacHTML5 — única fonte operacional — acessado somente leitura, sem escrita e sem migrations (ADR-008, ADR-003)
- Sem persistência de indicadores (sem cache, snapshot ou histórico no MVP)
- Não consultar o sistema legado VCL/Delphi nem seu banco em runtime (ADR-008)
- Não alterar `Dashboard.Web` ou comportamento existente sem necessidade explícita
- Identidade, credenciais, sessão, autorização e contexto do usuário pertencem ao SisacHTML5; o Dashboard não tem autenticação própria e não transporta/armazena segredos do Sisac (ADR-009)

### Banco

- Dashboard **somente leitura** — sem criar tabelas, sem alterar schema, sem `INSERT`/`UPDATE`/`DELETE`, sem criar procedures, sem migrations (ADR-003, ADR-008)
- Nunca conceder permissões automaticamente; nunca usar credencial administrativa
- Nunca armazenar credencial, connection string ou senha no repositório — a credencial de leitura chega por variável de ambiente `ConnectionStrings__SisacDatabase`

### Investigação

- Preferir evidência existente; investigar só o necessário
- Não reabrir gate fechado sem motivo objetivo — a lista de encerradas vive em `docs/sdd/STATUS-002-estado-workflow.md` §7
- Não investigar caminhos com prefixo `old`/`OLD`
- Não modificar código durante investigação sem gate explícito
- Não inventar resultado de teste ausente; lacuna é o achado

### Git

- Ao fim de uma unidade real de trabalho: `git status`, revisar `git diff`, confirmar ausência de segredos e arquivos locais, commitar e dar push
- `bin/` e `obj/` são artefatos de build — não versionar
- O Git é fonte confiável do estado do projeto

## Documentação

- **Arquitetura:** `docs/architecture/proposta-arquitetural.md`
- **ADRs:** `docs/architecture/adrs/` — decisões arquiteturais numeradas
- **Prototype / SPEC-UI:** `docs/prototype/`
- **PRDs:** `docs/prds/` — requisitos por feature (RN-XX, CA-XX); inclui `PRD-002-atendimentos-especificacao-funcional.md`
- **Planos:** `docs/plans/` — tarefas de execução (T-XX); plano atual `PLAN-001-dashboard-indicadores.md`
- **Reviews:** `docs/reviews/` — relatórios de review (R-XX); review atual `REVIEW-T-08-2026-09-18.md`
- **Handoffs:** `docs/handoffs/` — encerramento de investigações e estado de sessão
- **SDD:** `docs/sdd/` — `STATUS-001-checkpoint-kickoff.md` é histórico (kick off de 2026-09-16), superado e preservado

## Estado do workflow

- **Fonte de verdade do estado atual:** `docs/sdd/STATUS-002-estado-workflow.md`
- `STATUS-001-checkpoint-kickoff.md` é histórico (kick off de 2026-09-16) — superado, preservado, não usar para decidir o próximo passo
- Ao encerrar a sessão, **atualize o STATUS-002** em vez de criar handoff novo
- Handoff só para evento que exija registro próprio: mudança de fase, incidente ou encerramento formal
- Se houver divergência entre documento histórico e estado atual, **registre a divergência** — não reconcilie por inferência nem reescreva o histórico

## Como trabalhar neste projeto

Este projeto usa o pipeline SDD Leanwork. Antes de implementar qualquer feature:

1. Verifique se existe um plano em `docs/plans/PLAN-XXX-*.md`
2. Identifique a próxima tarefa pendente sem bloqueio: `Status: Pendente` e todas as tarefas de `Depende de:` com `Status: Concluído`
3. Leia a tarefa inteira, incluindo os campos `Implementa:`, `Valida:` e `Decisões base:`
4. Abra os artefatos referenciados
5. Respeite os pontos de validação humana marcados no plano
6. Nomeie os testes conforme a convenção `CA_XX_*`
7. Atualize o campo `Status:` e o histórico de execução ao concluir
8. Execute uma tarefa por vez e peça review antes de seguir

<!-- leanwork-context:end -->
