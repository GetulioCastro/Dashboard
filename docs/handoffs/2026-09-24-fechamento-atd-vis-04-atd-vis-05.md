# Handoff — 2026-09-24 Fechamento do dia: ATD-VIS-04 e ATD-VIS-05

> **Data:** 2026-09-24
> **Modo:** MODO ENGENHEIRO — FECHAMENTO DE SESSÃO (sem novas alterações funcionais)
> **Escopo:** encerrar o dia registrando o estado validado dos trabalhos ATD-VIS-04 (contrato `IndicadorVisao`) e ATD-VIS-05 (fatia vertical demo de Atendimentos). Nenhuma alteração funcional adicional foi executada; nenhum commit foi realizado.

---

## 1. Objetivo da sessão

Preparar a primeira visão funcional de **Atendimentos** sobre o novo contrato de visão, em duas etapas sucessivas:

- **ATD-VIS-04** — implementar e proteger o contrato `IndicadorVisao` em `Dashboard.Core` (por SPEC-VIS-001);
- **ATD-VIS-05** — produzir a menor mudança para exibir essa visão com dados demonstrativos, **sem SQL** e **sem tocar na camada Data**.

## 2. ATD-VIS-04 — Concluído

Contrato `IndicadorVisao` implementado em `Dashboard.Core/DTOs/IndicadorVisao.cs` e protegido por testes:

- Tipos: `DimensaoVisao { Nenhuma, Convenio, Sexo }` e `GraficoForma { Linha, Coluna, Donut }`.
- `IndicadorVisao`: `Id`, `Nome`, `Unidade`, `Monetario`, `Periodo` (`BusinessReferencePeriod`), `ValorTotal`, `Forma`, `DimensaoAtiva`, `DimensoesDisponiveis`, `Series`.
- `Data` e `ValorAtual` **removidos** do contrato (SUB por `Periodo`/`ValorTotal`).

**Registros a preservar:**
- O projeto de testes nasceu com **9 testes** (`IndicadorVisaoTests.cs`); o estado originalmente registrado citava "10 testes". Divergência anotada na conclusão do ATD-VIS-04.
- Falha corrigida durante a validação: assert `DimensoesDisponiveis.Count` corrigido de `3` → `2` (fixture com `Convenio`/`Sexo`; `Nenhuma` **não** é dimensão funcional — SPEC §5.3).

## 3. ATD-VIS-05 — Concluído (validação técnica)

Fatia vertical **demo e isolada** de Atendimentos consumindo o novo contrato. A página `/Atendimentos` usa `IndicadorVisao` e cobre:

- evolução temporal (dimensão `Nenhuma`);
- formas **Linha**, **Coluna** e **Donut**;
- dimensão **Convênio** → séries Particular / Convênio / SUS (RN-26);
- dimensão **Sexo** → séries Masculino / Feminino;
- **total** e **período** de referência explícitos;
- enums serializados como **camelCase** (`JsonStringEnumConverter(CamelCase)`); `Periodo` como `{start, end}`.

Comportamento de conformidade: `dimensao=nenhuma&forma=donut` é **clampeado para Coluna** (SPEC §4 — temporal não admite Donut).

## 4. Arquivos criados

**ATD-VIS-04:**
- `Dashboard.Core/DTOs/IndicadorVisao.cs` (novo)
- `Dashboard.Core.Tests/Dashboard.Core.Tests.csproj` (novo)
- `Dashboard.Core.Tests/DTOs/IndicadorVisaoTests.cs` (novo — 9 testes)
- `Dashboard.slnx` (editado: registro do projeto `Dashboard.Core.Tests`)

**ATD-VIS-05 (todos aditivos, nenhum arquivo existente alterado):**
- `Dashboard.Web/Dados/IndicadoresVisaoDemonstracao.cs` — fonte demo determinística → `IndicadorVisao`
- `Dashboard.Web/Pages/Atendimentos.cshtml.cs` — `AtendimentosModel`
- `Dashboard.Web/Pages/Atendimentos.cshtml` — card + seletores de dimensão/forma
- `Dashboard.Web/wwwroot/js/atendimentos.js` — render Linha/Coluna/Donut

## 5. Validações realizadas

- `dotnet build Dashboard.slnx` → **0 avisos / 0 erros** (CS8603 corrigido na `Rota` do cshtml).
- `dotnet test Dashboard.Core.Tests` → **9/9 aprovados**.
- Runtime (porta 5140, perfil http):
  - `/Atendimentos` → HTTP 200; `forma=coluna`, `dimensaoAtiva=nenhuma`, 1 série, `valorTotal=5463`, período 90 dias;
  - `/Atendimentos?dimensao=convenio&forma=donut` → HTTP 200; 3 séries, 1 ponto cada;
  - `/Atendimentos?dimensao=sexo&forma=linha` → HTTP 200; 2 séries, 90 pontos cada;
  - `/Atendimentos?dimensao=nenhuma&forma=donut` → HTTP 200; clamp para `coluna`;
  - `/` (Index original) → HTTP 200; payload `dados-indicadores` presente.

## 6. Validação visual humana da página `/Atendimentos`

**Pendente — não executada nesta sessão.** Foi realizada apenas **validação técnica HTTP** (código 200, contrato e serialização conferidos por inspeção do JSON). A checagem visual humana (renderização dos gráficos, Donut, legenda, seletor) é a **gate restante** do ATD-VIS-05 e está agendada como ponto de retomada.

## 7. Estado do Git

- **Nenhum commit** foi criado (orientação mantida).
- Novos do dia (untracked): 4 arquivos de ATD-VIS-05 + artefatos de ATD-VIS-04 (`IndicadorVisao.cs`, `Dashboard.Core.Tests/`) + `Dashboard.slnx` (modificado no ATD-VIS-04).
- Demais `M`/`??` em bin/obj são artefatos de build; alterações de código em `IndicatorFilter.cs`, `AGENTS.md`, docs e `opencode.json` pertencem a tarefas anteriores (T-08, ADR-009/010, investigações).
- Sem reset/revert/stash.

## 8. Bloqueio existente — R-05 / CASAMATER

Permanece **congelado** conforme `docs/handoffs/2026-09-24-bloqueio-r05-casamater.md`: o CASAMATER está indisponível/contido (timeout no estágio Post-Login). Isso impede a execução dos testes de integração do T-08 (CA-03, CA-09, CA-18) e mantém o T-08 em **"Implementado (validação pendente)"**. Nenhuma tentativa de conexão foi feita hoje para validar as fatias demo.

## 9. O que NÃO foi implementado

- Nenhuma integração SQL em `/Atendimentos` — **os dados apresentados são 100% DEMO** (determinísticos, gerados em `IndicadoresVisaoDemonstracao`).
- Nenhuma alteração em `Dashboard.Data`, `Dashboard.Data.Tests`, `AtendimentosRepository`, `IIndicatorRepository`, `IndicatorData`, `IndicatorFilter` ou `PeriodoResolutor` (camada de evidência congelada).
- Nenhum Provider/Service/Mapper/Factory/Strategy criado.
- Nenhuma alteração de PRD, SPEC, ADR, PLAN, `AGENTS.md` ou `opencode.json`.
- O fluxo `/` original **continua preservado** e funcional.
- Nenhuma migração do JSON demo da Index para o contrato de visão (etapa futura conforme SPEC §7).
- Nenhum commit/reset/revert/stash.

## 10. Pendência imediata para a próxima sessão

1. **Validação visual humana** da página `/Atendimentos` (`dotnet run --project Dashboard.Web` → `http://localhost:5140/Atendimentos`): conferir renderização Linha/Coluna/Donut, legenda, total/período e troca de dimensão (Convênio → Particular/Convênio/SUS; Sexo → Masculino/Feminino).
2. Após a gate visual, decidir o próximo passo da visão (ex.: migrar o fluxo da Index para o contrato — que é o cenário adiado pela SPEC §7 — ou aguardar o desbloqueio do provider SQL).

## 11. Ponto de retomada recomendado

**Retomar por:** validação visual humana de `/Atendimentos` (item 10), usando o mesmo `dotnet run --project Dashboard.Web` com o perfil http (porta 5140). Em paralelo, monitorar a disponibilidade do CASAMATER para retomar o fechamento do T-08 (R-05) — a fatia demo **não depende** do banco.

---

**Deixado explícito:** os dados de `/Atendimentos` são **DEMO**; essa página **não tem integração SQL**; a **fonte real de dados permanece pendente**; o fluxo `/` original está **preservado**; T-08/R-05 segue **congelado**. Sessão encerrada.