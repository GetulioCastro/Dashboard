# Handoff — Encerramento da Frente de Investigação do Indicador Atendimentos por Convênio (23/09/2026)

> **Data:** 2026-09-23
> **Modo:** MODO ENGENHEIRO — FECHAMENTO DA SESSÃO (investigação somente leitura)
> **Tema:** Indicador **ATENDIMENTOS** — frente "Atendimentos por Convênio".
> **Escopo:** consultas somente leitura em `CASAMATER` (entidade `dashboard_readonly`). Nenhum código, banco, permissão ou documento foi alterado.

---

## 1. Estado da investigação

Frente **encerrada por decisão humana** sem conclusão do SQL definitivo do indicador. A investigação poderá ser retomada em outra janela ou pelo DBA com credencial apropriada.

## 2. Comportamento observado das consultas (`dbo.ENTRADA`)

- Consultas sobre `ENTRADA` apresentaram comportamento **intermitente**: inicialmente executaram normalmente (segundos); posteriormente passaram a **exceder 300–360 segundos** (timeouts).
- Consultas simples e de metadados (`sys.columns`, `sys.indexes`, `SELECT DB_NAME()`) permaneceram responsivas.
- O diagnóstico de sessões, esperas e bloqueios **não pôde ser concluído**: `dashboard_readonly` **não possui `VIEW SERVER STATE`** (`VIEW SERVER PERFORMANCE STATE` negada em `sys.dm_exec_requests`, `sys.dm_exec_sessions`, `sys.dm_exec_sql_text`, `sys.dm_os_waiting_tasks`).
- **Nenhuma sessão foi cancelada.**
- **Nenhum dado, objeto, permissão ou código foi alterado.**
- **A causa da contenção permanece NÃO CONFIRMADA.**

## 3. Evidências confirmadas — Atendimentos por Convênio

- **Origem da contagem:** `dbo.ENTRADA` (mestre de movimentos, 1.912.675 linhas; `CODMOVIMENTO` praticamente único — 1 código duplicado = 2 linhas; `COUNT(*)` ≈ `COUNT(DISTINCT CODMOVIMENTO)`).
- **Campo de convênio:** `ENTRADA.CODCONVENIO varchar(10)` — 123 códigos distintos em uso; 1 `NULL` e 6 vazios.
- **Cadastro:** `dbo.CADCONVENIO` — 597 linhas, 323 códigos distintos; `DESCR varchar(250)`, `MODOFAT varchar(1)`, `GRUPOEMP`/`FILIAL char(2)`; índice único `CADCONVENIO0 = (CODCONVENIO, GRUPOEMP, FILIAL)`.
- **Duplicidade de cadastro:** 277 códigos com mais de uma linha, separados por `GRUPOEMP/FILIAL`. **Não tratar como erro de negócio** — caso `150` explicado pelo suporte: duplicação de cadastro por criação e posterior encerramento de uma filial, com registros preservados por movimentos históricos.
- **MODOFAT por código distinto:** `C`=124, `P`=32, `S`=3, vazio=162, `NULL`=2. Apenas 3 códigos com `MODOFAT` divergente entre linhas: **150** (usado em `ENTRADA`; `01/01` CASAMATER `P` × `01/02` NORTE `C`), **330/331** (não usados).
- **Regra de contagem aplicada (RN-07):** `FECHADO <> 'C' AND COALESCE(LOTEENT,'') <> 'INAT'`:
  - Global: `C`=1.307.004 · `P`=515.967 · `S`=16.893 · sem código=2.
  - 2024 (87.798 registros no ano): `C`=70.982 · `P`=12.032 (total 83.014) — **sem SUS nem "não classificado" em 2024**.
  - Discrepância leve frente ao total documentado (83.014 ÷ 82.118) — provável variação na comparação de `LOTEENT`; lacuna registrada, não invalida RN-07.
- **JOIN direto `ENTRADA`↔`CADCONVENIO` por `CODCONVENIO` multiplica linhas** (soma do "top" executado superou o total real de 2024). **Não usar JOIN simples por `CODCONVENIO` como regra definitiva.**
- **Período:** `DATAHORAENT` (datetime) — min `1899-12-30` (14 sentinelas Delphi-zero), max `2026-08-12`; 0 registros futuros; 2025=170, 2026=32 → janela representativa atual vai até 2024.
- **Ranking TOP 15 por convênio (2024):** consulta montada e validada sintaticamente (contagem 1:1 sobre `ENTRADA`, cadastro consolidado 1 linha/código, sinalização `AMBIGUO(...)`), mas **NÃO concluída** por contenção — **pendente**.

## 4. Vedações registradas (decisão humana desta sessão)

1. NÃO executar novas consultas sobre `dbo.ENTRADA` nesta janela.
2. NÃO contornar a ausência de `VIEW SERVER STATE`.
3. NÃO alterar permissões da conta `dashboard_readonly`.
4. NÃO cancelar sessões, processos ou consultas.
5. NÃO aumentar timeout.
6. NÃO implementar o SQL definitivo de "Atendimentos por Convênio".
7. NÃO alterar código do Dashboard.
8. NÃO alterar banco.
9. NÃO modificar PRD, PLAN, ADRs ou documentação existente sem autorização explícita.

Decisões de modelagem a respeitar nas próximas etapas:

- **Não transformar `MODOFAT` (`C`/`P`/`S`) em regra de negócio sem decisão funcional explícita.**
- **Não tratar duplicidade de `CADCONVENIO` por `GRUPOEMP/FILIAL` como erro de negócio.**
- **Não usar JOIN simples por `CODCONVENIO` como regra definitiva.**

## 5. Pendências

- SQL definitivo do indicador "Atendimentos por Convênio" — **pendente** (inclui ranking TOP N validado sem multiplicação).
- Causa da contenção em `ENTRADA` — **não confirmada**; retomar em outra janela ou diagnosticar pelo DBA com `VIEW SERVER STATE`.
- Dimensão **Sexo**: localizada `dbo.CADPACIENTE.SEXO varchar(1)`, 314.494 registros, `CODPACIENTE` sem duplicidade; distribuição de valores e relacionamento com `ENTRADA` ainda **não medidos** (frente futura).

## 6. Próxima ação

Definida por **decisão humana** (não há automação pendente deste lado).