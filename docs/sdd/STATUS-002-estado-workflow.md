# STATUS-002 — Estado do Workflow (transição Leanwork SDD + Atendimentos Reais)

> **Tipo:** estado do workflow — recuperação persistente entre sessões
> **Data:** 2026-10-08 (origem: 2026-09-28; última atualização anterior: 2026-10-07)
> **Substitui:** `STATUS-001-checkpoint-kickoff.md` (2026-09-16) — **preservado,
> não apagado**; descreve o kick off, hoje superado
> **Regra:** ao encerrar uma sessão, ATUALIZE este arquivo. Não crie handoff novo
> por padrão.

## 1. Ordem de recuperação do contexto
1. `AGENTS.md` — regras permanentes, stack, comandos
2. Este arquivo — estado atual
3. `docs/plans/PLAN-001-dashboard-indicadores.md` — tarefas T-XX
4. `docs/plans/PLAN-002-analise-por-convenio.md` — tarefas T-XX (ciclo encerrado)
5. `docs/plans/PLAN-003-visualizacao-grafica-por-convenio.md` — tarefas P3-TXX (ciclo encerrado)
6. `docs/reviews/` — findings R-XX
7. `docs/handoffs/` — histórico como evidência; não reabrir

## 2. Fase atual
| Fase | Artefato | Estado |
|---|---|---|
| Arquitetura | `docs/architecture/proposta-arquitetural.md` + ADR-007..010 | Aprovada |
| Requisitos | PRD-001 (Epic) / PRD-002 (Atendimentos) | Rascunho — revisão 2 |
| Especificação | SPEC-UI-001, SPEC-VIS-001 | Aguardando validação humana |
| Plano | PLAN-001 (T-01..T-21) | Execução em andamento — **T-14 CONCLUÍDA** (entrega 2026-10-07, extensão de mini-gráficos reais 2026-10-08, fechamento documental 2026-10-08; §3.3 e §3.3.1); **T-11 com diagnóstico pré-implementação concluído** (2026-10-08; §3.4); **T-11A (faturamento realizado) implementada, validada e commitada** (`6061aba`, 2026-10-09; §3.4.2) — **T-11 permanece PARCIALMENTE CONCLUÍDA** ("A Faturar" pendente de definição funcional); **T-13 com diagnóstico de ponte de dados concluído** (2026-10-09; §3.5) — implementação **não iniciada** |
| Plano | PLAN-002 (T-01..T-08) — Análise por Convênio | **ENCERRADO (2026-10-06)** — T-01..T-08 concluídas, build 0/0, 72/72 testes, P19 reconciliado, UI integrada, **gate humano visual aprovado (2026-10-06)**; sem pendência funcional aberta; evolução gráfica não iniciada *(afirmação correta em 2026-10-06; a evolução gráfica foi executada depois, em 2026-10-07 — ver linha seguinte)* |
| Plano | PLAN-003 (P3-T01..P3-T05) — Visualização Gráfica por Convênio | **ENCERRADO (2026-10-07)** — registro retroativo; gráfico horizontal SVG em Atendimentos, Consultas e Exames; responsividade validada (DevTools mobile); build 0/0, 72/72 testes, validações humanas concluídas; commits `0038c54`, `6619782`, `2c62fae`, `e9a3917` |
| Revisão | REVIEW-T-08-2026-09-18 | Aprovado com ressalvas |

> **Nota de reconciliação (2026-10-02):** Reconciliação documental do estado do workflow realizada sem alterar documentos históricos.

## 3. Tarefas
| Tarefa | Status | Próxima ação |
|---|---|---|
| T-01..T-07 | Concluído | — |
| **T-08** | **Implementado e Validado (reconciliado 2026-10-02)** | Integrados CA-03/CA-09/CA-18 (estado reconciliado 2026-10-02) |
| **T-09, T-10** | **Concluído (reconciliado 2026-10-07)** | Entregas já existentes no código (`ConsultasVisaoRepository`/`ExamesVisaoRepository`, commits `9ab8f9c`/`f43aac0`) — PLAN-001 atualizado por reconciliação documental, sem reescrita de histórico; CA-09 com lacuna de validação declarada |
| **T-11** | **Parcialmente concluída (2026-10-09)** — regras fechadas (2026-10-09, §3.4.1) e **T-11A (faturamento realizado = `FECHADO IN ('F','E')`) implementada, validada e commitada** (`6061aba`) | **"A Faturar" pendente de definição funcional** — ver §3.4.2 e PLAN-001 T-11 |
| **T-13** | **Pendente — diagnóstico de ponte de dados concluído (2026-10-09)** | Metadados e cardinalidade `PAGAR`×`PAGARC` comprovados (K5 = 1:1); implementação **não iniciada**; pendências humanas de classificação/data abertas — ver §3.5 e PLAN-001 T-13 |
| T-12, T-19 | Pendente | Elegíveis por dependência — ver §6, tensão objeto de reconciliação documental em 2026-10-02 |
| **T-14** | **Concluído** (entrega e validação 2026-10-07; extensão de mini-gráficos reais 2026-10-08; fechamento documental 2026-10-08) | — (registros em §3.3 e §3.3.1) |
| T-15..T-18, T-20, T-21 | Pendente | Bloqueadas |

### 3.1 — PLAN-002 (Análise por Convênio) — registro aditivo (2026-10-05)

Novo plano, **independente** do grafo T-01..T-21 do PLAN-001. Não altera o estado da §3.

| Tarefa | Status | Próxima ação |
|---|---|---|
| **T-01** — Inventariar pontos de extensão por indicador | **Concluído** (2026-10-05) | — |
| **T-03** — Extender repositórios Data (SQL/consulta) para Convênio | **Concluído** (2026-10-05) | Entrega: 3 repositórios em `Dashboard.Data/Repositories` implementando `IIndicadorConvenioNominalRepository` (Atendimentos/Consultas/Exames); JOIN por chave composta, `C.DESCR` integral, filtros RN-07 e tipos 1/3; L2 e L3 resolvidas com evidência read-only; build 0 avisos/0 erros; 48 testes aprovados |
| **T-04** — Regra de ordenação + desempate (R5) | **Concluído** (2026-10-05) | Ordenação ajustada para `ORDER BY COUNT(1) DESC, C.DESCR ASC, C.CODCONVENIO ASC, C.GRUPOEMP ASC, C.FILIAL ASC` nos 3 repositórios nominais (desempate técnico determinístico). Preserva identidade da chave composta, não altera `Identificacao = C.DESCR`. R5 resolvida. Build 0 avisos/0 erros; 48 testes aprovados. |
| **T-05** — Integrar no backend Web (PageModel) filtros por Convênio | **Concluído** (2026-10-05) | Registrados os três repositórios nominais na DI do Web (`IIndicadorConvenioNominalRepository` → Atendimentos/Consultas/Exames). Sem alteração de UI, sem alteração de autenticação, sem impacto em repositórios/contratos existentes. Build 0 avisos/0 erros; 48 testes aprovados. |
| **T-06** — UI da seção nos 3 indicadores | **Concluído** (2026-10-06) | Seção `<details>` expansível com tabela, estados vazio/erro e navegação dos cards preservada; entrega registrada em `PLAN-002` (§ Entrega da T-06) |
| **T-07** — Testes CA-003-01..07 | **Concluído** (2026-10-06) | `ConvenioNominalRepositoryTests.cs`: 24 testes novos (16 SQL estático + 8 integração); CA-003-01/03/04/07 automatizados, CA-003-02/05/06 por regressão + revisão analítica de UI; suíte **72/72**; divergência de narrativa P19 (T-03 vs SQL) **reconciliada no gate humano final (2026-10-06)** — narrativa corrigida, P19 aplicado |
| **T-08** — Revisão final e escopo | **Concluído** (2026-10-06) | Checklist 4/4 e critérios globais 5/5; sem indicador novo, escopo exclusivo dos 3 indicadores, nenhuma rota nova; **ciclo PLAN-002 encerrado** (T-01..T-08 concluídos); evolução gráfica não iniciada *(correto na data da tarefa; executada depois em 2026-10-07 pelo PLAN-003 — ver §3.2)* |

**T-02 — CONCLUÍDA em 2026-10-05 (contratos Core, RN-003-03/RN-003-04, valida CA-003-02).**
Criados `Dashboard.Core/DTOs/IndicadorConvenioNominal.cs` (`ConvenioNominalVolume`,
`IndicadorConvenioNominalVisao`) e `Dashboard.Core/Contratos/IIndicadorConvenioNominalRepository.cs`
(`ObterConvenioNominalAsync(IndicatorFilter, CancellationToken)`). **Contrato novo, não extensão**:
`IndicatorFilter`, `IndicadorVisao`, `DimensaoVisao`, `CoverageCategory`,
`IIndicadorVisaoRepository` e `IIndicatorRepository` **permanecem inalterados** — nenhum campo de
convênio nominal foi adicionado ao filtro, porque a análise nominal **não é um filtro** (DEC-003-05
proíbe drill-down/clique; RN-003-01..10 não exigem seleção de convênio). Nomenclatura
`ConvenioNominal` preserva a distinção entre cobertura (RN-26) e identificação nominal. Nenhuma
regra de ordenação no contrato — R5 permanece para T-04. Sem `MODOFAT` em Core. Sem SQL, sem
Web/UI, sem DI (T-03/T-05), sem banco, sem Git.

**L1 — RESOLVIDA em 2026-10-05 (reconstrução documental do PRD-003):**
`docs/prds/PRD-003-analise-por-convenio.md` foi **reconstruído (v0.2)** sob gate humano. Causa
diagnosticada como **falha de materialização original**, não truncamento: o artefato foi criado
já incompleto no commit `cfc5ecc` (2026-10-02), e a verificação de histórico Git somente leitura
confirmou **um único blob** em todo o objeto alcançável, **nenhum segundo commit, nenhum stash,
apenas `main`/`origin/main`** — não existia versão completa a restaurar. Materializados: §1–§5,
**RN-003-01..RN-003-10**, **CA-003-02..CA-003-07**, matriz RN↔CA↔DEC (§5.1), premissas abertas
(§5.2). **CA-003-01 preservada semanticamente; DEC-003-01..DEC-003-06 preservadas literalmente**
( apenas o cabeçalho de estado obsoleto "PENDENTE/VALIDAR" → aprovadas em 2026-10-02).
**Nada foi inventado:** §1–§5 originais, o documento de "autorização" citado na versão parcial e
personas/stakeholders/métricas/diagramas permanecem **não recuperados** (sem fonte no repositório).
**CA-003-08 e CA-003-09 nunca existiram** — família oficial **CA-003-01..CA-003-07**.

**L4 — RESOLVIDA em 2026-10-05 (decisão humana):** a análise nominal representa os **convênios
efetivamente presentes nos registros válidos** de cada indicador no período, **preservando RN-07,
P19 e demais regras aprovadas**. **NÃO** se introduz `MODOFAT = 'C'` como nova regra de produto,
e a análise **não depende** de a categoria de cobertura "Convênio" estar selecionada. Deixa de
bloquear T-03. Materializado em `PRD-003` §3.3 e RN-003-02.

**L2 — RESOLVIDA na T-03 (2026-10-05):** estrutura necessária confirmada read-only
(`INFORMATION_SCHEMA.COLUMNS`: `CADCONVENIO` com `CODCONVENIO,GRUPOEMP,FILIAL,DESCR,MODOFAT,SUSPENSO`
e `ENTRADA` com as colunas do filtro). P17 permanece como premissa de homologação em
`contrato-dados-dashboard.md` para o banco-alvo SisacHTML5 — registro em `PLAN-002` (§ Entrega da T-03).

**L3 — RESOLVIDA na T-03 (2026-10-05):** `CADCONVENIO` 603 linhas / 603 chaves distintas
`(CODCONVENIO,GRUPOEMP,FILIAL)`; `INNER JOIN` com `ENTRADA` perde < 0,0012% — chave
funcional verificada com evidência read-only.

**R5 — RESOLVIDA na T-04 (2026-10-05):** desempate determinístico final
`ORDER BY COUNT(1) DESC, C.DESCR ASC, C.CODCONVENIO ASC, C.GRUPOEMP ASC, C.FILIAL ASC`
nos 3 repositórios nominais, sem contradizer DEC-003-01/DEC-003-06.

**Colisão de nomenclatura:** `DimensaoVisao.Convenio` e `CoverageCategory.Convenio` já significam **cobertura** (Particular/Convênio/SUS), não convênio individual — não reutilizar. Materializado como RN-003-03 e CA-003-02.

**Reconciliação do PLAN-002 (2026-10-05):** o mapeamento `Decisões base:` foi corrigido de **posicional** (T-0*n* → DEC-003-0*n*) para **semântico** — T-04 passa a DEC-003-01 + DEC-003-06; T-06 passa a DEC-003-02..DEC-003-05 + SPEC-UI-003; T-01/T-02/T-03/T-05/T-07 sem DEC de apresentação. Nenhum checkbox de escopo funcional alterado. SPEC-UI-003, ADR, PRD-001, PRD-002, PLAN-001 e código **não alterados**.

### 3.2 — PLAN-003 (Visualização Gráfica por Convênio) — registro aditivo (2026-10-07)

Plano **retroativo**, criado sob decisão humana no fechamento documental de 2026-10-07
(`docs/plans/PLAN-003-visualizacao-grafica-por-convenio.md`). Independente do PLAN-001 e do
PLAN-002; usa IDs `P3-TXX` próprios para **evitar colisão** com as tarefas `T-XX` dos planos
anteriores. Não altera o estado das §3 e §3.1.

| Tarefa | Status | Evidência |
|---|---|---|
| **P3-T01** — Preparação/definição do padrão gráfico por convênio | **Concluído** (2026-10-07) | Diagnóstico: infraestrutura nominal completa nas 3 páginas (seção, tabela, `Model.ConvenioNominal`, Identificacao + Volume); regra de negócio confirmada preservada |
| **P3-T02** — Atendimentos por Convênio | **Concluído** (2026-10-07) | Commit `0038c54`; validação visual humana concluída; build 0/0; 72/72 testes |
| **P3-T03** — Consultas por Convênio | **Concluído** (2026-10-07) | Commit `6619782`; validação visual humana concluída; implementação single-file; regra `TIPO = '1'` intacta; build 0/0; 72/72 testes |
| **P3-T04** — Exames por Convênio | **Concluído** (2026-10-07) | Commit `2c62fae`; validação visual humana concluída; implementação single-file; regra `TIPO = '3'` intacta; build 0/0; 72/72 testes |
| **P3-T05** — Consistência/responsividade e validação global | **Concluído** (2026-10-07) | Commit `e9a3917` (`.convenio-rolagem svg { min-width: 40rem; }` em `site.css`); consistência estrutural 10/10 entre as 3 páginas; validação mobile via DevTools; validação global: repo limpo, build 0/0, 72/72 testes, nenhum bloqueador |

**Decisões do ciclo:** SVG nativo, barras horizontais, gráfico antes da tabela, todos os
convênios preservados (sem Top N, sem "Outros", sem agregação), nomes completos na tabela,
scroll vertical preservado, sem Chart.js, padrão idêntico nos 3 indicadores.
**Fora do ciclo (não executado):** Top 10 + "Demais convênios", comparativo
Consultas × Exames, redesign de identidade visual.

### 3.3 — PLAN-001 / T-14 — registro aditivo (2026-10-08)

Registro aditivo do fechamento documental da **T-14** (PLAN-001). Não altera o conteúdo das
seções anteriores; o texto da §6 que descrevia T-14 como "próxima frente, pendente" **mantém-se
como estado anterior** e é contextualizado ao final desta seção.

| Tarefa | Status | Evidência |
|---|---|---|
| **T-14** — Cards dos indicadores assistenciais (Atendimentos, Consultas, Exames) | **Concluído** (entrega e validação humana 2026-10-07; extensão de mini-gráficos reais validada e commitada em 2026-10-08; fechamento documental 2026-10-08) | Commits `a1159c1`, `61ca098`, `100ba8f`, **`64c93af`**; build Release **0 erros / 0 avisos**; testes **72/72**; validação runtime humana concluída (2026-10-07 e 2026-10-08); repositório versionado |

**Estado registrado:**

- A **Home opera em modo híbrido**: **Atendimentos**, **Consultas** e **Exames** com dados
  **reais**; os demais indicadores permanecem **demonstrativos**;
- o **filtro de período da Home** atualiza os 3 indicadores reais, por handler Razor Pages
  dedicado; os dados reais **não usam** fallback demonstrativo;
- valor **zero real** é distinguível de **indicador indisponível**; mini-gráficos
  demonstrativos **neutralizados** nos 3 cards reais;
- navegação Home → detalhe **preserva o período** em Atendimentos, Consultas e Exames;
  aliases `DataInicial`/`DataFinal` corrigidos em Consultas e Exames;
- textos da Home ajustados para refletir o estado híbrido (commit `100ba8f`);
- validação visual humana concluída; build **0/0**; testes **72/72**.

### 3.3.1 — Extensão final da T-14 — mini-gráficos reais (aditivo, 2026-10-08)

Registro **aditivo** da extensão aprovada como *extensão pequena da T-14* (não nova frente
arquitetural). Commit **`64c93af`** — `feat(dashboard): add real charts to assistential home cards`.
Arquivos alterados: somente `Dashboard.Web/Pages/Index.cshtml.cs` e
`Dashboard.Web/wwwroot/js/dashboard.js`.

- **Atendimentos, Consultas e Exames** exibem **valor real + mini-gráfico real + período real**;
- dados demonstrativos **não participam** dos 3 cards reais (valor, gráfico e cor);
- a Home passou a usar **`DimensaoVisao.Nenhuma`**, obtendo a evolução temporal consolidada;
- pontos reais extraídos de `IndicadorVisao.Series` → `SerieGrafico.Pontos`
  (`{ data, valor }` no JSON); cor extraída da **`SerieGrafico` real**;
- o **modal Maximizar** reutiliza o mesmo conjunto de pontos em memória — **sem novo fetch**;
- **período com valor real zero:** valor = `0`, gráfico = *"Sem movimento no período."*
  (zero não é indisponibilidade);
- **indicador indisponível:** valor = `—`, caption *"Indisponível"*, gráfico
  *"Gráfico indisponível."*;
- o handler `OnGetIndicadoresReaisAsync` permanece como **fonte única** dos 3 cards reais;
- **otimização: 12 → 3 queries por atualização** (1 por indicador; antes 4 × 3 por
  `DimensaoVisao.Convenio`);
- **sem** SQL novo, **sem** repository novo, **sem** service layer nova, **sem** biblioteca
  gráfica nova.

**Validação humana final (runtime, 2026-10-08):** Home com mini-gráficos reais; mudança de
período atualiza valor e gráfico em conjunto; estado **zero real** observado em Exames; modal
Maximizar com gráfico real; navegação Home → detalhe preservando período; build Release
**0 erros / 0 avisos**; testes **72/72**.

**Decisão humana — `_IndicatorCard.cshtml` (08/10/2026): DISPENSADO.**
> "A criação do partial `_IndicatorCard.cshtml` foi dispensada. A implementação reutilizou o
> loop e a estrutura de cards já existentes em `Index.cshtml`, sem necessidade de nova
> abstração."

O partial **não foi criado e não será criado retroativamente**; o artefato permanece **não
entregue literalmente** e sua ausência **não é bloqueante** — T-14 permanece **Concluída**.

**Risco registrado, sem tratamento nesta tarefa:** período **sem teto máximo** — intervalos
muito longos geram linhas/payload proporcionais; decisão futura.

**Contextualização do texto anterior (§6):** a nota de 2026-10-07 que definia T-14 como
"próxima frente técnica (pendente de execução; não iniciada)" **refletia o estado daquela data**
e permanece preservada. A T-14 foi executada e validada no mesmo ciclo, em 2026-10-07, e
fechada documentalmente em 2026-10-08. **T-15 permanece Pendente** e não foi iniciada.

### 3.4 — PLAN-001 / T-11 — diagnóstico pré-implementação (aditivo, 2026-10-08)

Registro aditivo do **diagnóstico funcional/técnico da T-11 (Faturamento)**, feito sem código e
sem tocar o baseline da demo (Home e cards reais intactos). O detalhamento completo — regras
fechadas, evidências físicas, pendências P20–P26, tabela das queries Q1–Q7, gates humanas e
esboços de `FaturamentoDto`/`IFaturamentoRepository`/`FaturamentoRepository` — está no bloco
**"Diagnóstico pré-implementação"** da tarefa T-11 em `docs/plans/PLAN-001-dashboard-indicadores.md`.

**Estado registrado:**

- **Regras funcionais fechadas:** faturado = `SUM(ENTRADA.Total)` com `FECHADO IN ('F','E')`
  (RN-10/RN-12, evidência **R** no dicionário §13.3); a faturar = convênio + alta + faturamento
  incompleto (RN-31/RN-33, T-02); período = data de emissão da guia (RN-13) + comparação com
  período anterior (RN-32); convênio cadastrado (RN-11); estados `A/P/F/E/C` (`E`=**Enviada**);
  formatação `R$` (RN-05) na Web;
- **Evidências físicas (E) suficientes** no contrato T-03 §8 para `ENTRADA`, `FATURA`,
  `BI_Faturamento` e `CADCONVENIO`; `RECEBER` descartado como base por RN-12;
- **Pendências físicas restantes:** P20–P25 (contrato T-03 §8.5) + P26 (decisão documental);
- **Próximo passo:** executar as **queries read-only Q1–Q6 pós-demo** (sqlcmd + conn string já
  disponíveis; nada executado nesta rodada) e obter **2 gates humanas** — (1) equivalência
  *"guia emitida" ≡ `FECHADO IN ('F','E')`* (classificada **U**, dicionário §13.12), (2) coluna
  da data de emissão (P22, orientada pela Q3);
- com isso, T-11 segue para implementação **sem nova decisão de negócio**; T-12 (Q3/profissional),
  T-13 (Q5/composição) e T-15 (toca `Index.cshtml` — baseline) **não foram iniciadas**.

**Nesta rodada:** nenhum arquivo de código alterado, nenhum SQL executado, nenhum build/test,
nenhuma DI/UI alterada, nenhum servidor iniciado; Git apenas com commits de documentação.

### 3.4.1 — PLAN-001 / T-11 — fechamento das regras de faturamento (aditivo, 2026-10-09)

Registro **aditivo** (preserva §3.4 acima). A **decisão humana, validada pelo DBA da empresa em
2026-10-09**, fechou as regras de estado do faturamento:

- `FECHADO = 'F'` = conta **faturada e ainda NÃO enviada**;
- `FECHADO = 'E'` = conta **faturada e enviada**;
- `FECHADO = 'P'` = parcial, ainda não cobrada;
- `FECHADO = 'X'` = ignorada;
- **Faturamento do Dashboard = estados `F` + `E`**;
- **Data de referência oficial = `ENTRADA.DataHoraEnt`** (resolve P22).

**"A Faturar" permanece pendente de definição funcional. Nenhuma regra deve ser inferida a partir
dos estados `FECHADO`** — não associar automaticamente `P`, `F`, `P+F` nem qualquer outro conjunto.

**Correção da hipótese antiga:** a hipótese *"guia emitida" ≡ `FECHADO IN ('F','E')`* (gate 1 de
§3.4, classificada **U**, dicionário §13.12) fica **substituída** pela regra validada acima —
`F` = faturada não enviada, `E` = faturada enviada, **faturamento total = `F` + `E`**. A hipótese
anterior permanece **apenas como histórico** e **não se aplica mais**. **"A Faturar" NÃO é marcado
como resolvido.**

**Evidências técnicas confirmadas (2026-10-08/09, somente leitura — CASAMATER):** fonte física no
`CASAMATER`; núcleo `ENTRADA + CADMEDICO + CADCONVENIO + FATURA` validado (`SELECT` efetivo de
`dashboard_readonly` nos 4 objetos, `HAS_PERMS_BY_NAME = 1`); `SELECT TOP (0)` compilou com **exit
code 0**; expressão `(F.Valor + F.CustoOP + F.Filme) * F.Quant`; join `FATURA F ON F.CodPaciente =
E.CodMovimento`; agregado jan/2026: `E` = 64.875 itens / 6.822 atendimentos / R$ 9.095.334,40 ·
`F` = 33 / 33 / R$ 2.528,00 · `P` = 541 / 7 / R$ 46.940,50 · `X` ausente no recorte. A evidência
**não altera** a regra humana.

**Consequência:** T-11 fica implementável para o **faturamento total = `F` + `E`**; **"A Faturar"
continua bloqueado** por falta de definição funcional. Nenhum código/SQL/build/DI nesta rodada
documental.

**Nesta rodada:** somente os 2 documentos alterados (`PLAN-001` e este `STATUS-002`); nenhum SQL,
build, código, Home ou DI; nenhum `git add/commit/push`.

### 3.4.2 — PLAN-001 / T-11A — implementação do faturamento realizado (aditivo, 2026-10-09)

Registro **aditivo** do fechamento documental da **T-11A**. Preserva integralmente §3.4 e §3.4.1.
A T-11A implementou **exclusivamente** `FATURAMENTO REALIZADO = FECHADO IN ('F','E')`, já
commitada e enviada.

**Implementação entregue (commit `6061aba` — `feat(data): implement realized billing repository`):**

- `Dashboard.Core/DTOs/FaturamentoDto.cs` — DTO mínimo (`DataInicial`, `DataFinal`, `ValorFaturado`);
- `Dashboard.Data/Repositories/FaturamentoRepository.cs` — SQL constante parametrizado, período
  semiaberto, `SUM((F.Valor + F.CustoOP + F.Filme) * F.Quant)`, `SUM` NULL tratado no C# como `0`,
  sem `COALESCE` por linha, sem concatenação de SQL;
- `Dashboard.Data.Tests/Repositories/FaturamentoRepositoryTests.cs` — 5 testes (3 unitários de SQL + 2 de integração);
- **sem** integração com Home, **sem** registro em DI, **sem** endpoint/página nova.

**Fonte física (somente leitura):** banco **CASAMATER**; núcleo `ENTRADA + CADMEDICO +
CADCONVENIO + FATURA`; join `FATURA F ON F.CodPaciente = E.CodMovimento`; filtros de faturamento
(`E.Tipo IN ('1','3','4','5','6','7')`, `E.Fechado IN ('F','E')`, `E.LoteEnt <> 'INAT'`,
`E.Restrito <> 'Z'`, `E.GrupoEmp='01'`, `E.Filial='01'`, `C.ModoFat IS NOT NULL`,
`E.Guia <> 'GUIA MEDICO'`, `E.Guia <> 'LENTEC'`, `M.REDUZIDO IS NOT NULL`); período em
`E.DataHoraEnt`.

**Regras F/E/P/X (decisão humana validada pelo DBA, 2026-10-09):** `F` = faturada e ainda **NÃO
enviada**; `E` = faturada e **enviada**; `P` = **parcial, ainda não cobrada**; `X` = **ignorada**.
Faturamento realizado = `F` + `E`. Data de referência = `ENTRADA.DataHoraEnt`.

**Permissões (evidência operacional):** `dashboard_readonly` já possuía `SELECT` em `ENTRADA` e
`CADCONVENIO`; **concedido** `SELECT` em **`CADMEDICO`** e **`FATURA`**; nenhum outro acesso. A
sub-slice não depende de `CADPACIENTE`, `FECHAMENTO` nem `LOCAL`.

**Validação da fórmula (jan/2026, `F`+`E`):** linhas pós-join = **64.908**; NULLs simultâneos em
`F.Valor`/`F.CustoOP`/`F.Filme`/`F.Quant` = **5 linhas**; fórmula literal = **R$ 9.097.862,40**;
COALESCE diagnóstica = **R$ 9.097.862,40**; diferença **R$ 0,00** → fórmula **literal** adotada
(COALESCE **não** promovido a regra).

**Validação técnica final:** build **Release 0 erros/0 avisos**; `Dashboard.Core.Tests` **9/9**;
`Dashboard.Data.Tests` **68/68**; **total 77/77, 0 falhas**.

**Incidente 18456 (objetivo):** na validação inicial, **25 testes de integração** falharam com
**SQL Server erro 18456** para `dashboard_readonly`. Diagnóstico: endpoint/banco/usuário/código da
`SqlConnectionFactory` corretos; env lida diretamente pelos testes; login OK via SSMS/sqlcmd;
factory isolada passou com a senha atual. **Causa operacional confirmada:** processo usava valor
**desatualizado** de `ConnectionStrings__SisacDatabase`. Após atualizar no escopo **User** e
recarregar explicitamente, resultado final **77/77**. **Sem registrar senha nem connection string.**

**Estado funcional:** FATURAMENTO REALIZADO **implementado e validado**; "A Faturar" **pendente de
definição funcional** (não entregue); **CA-04** atendido para faturamento realizado; **CA-14**
parcialmente pendente na distinção "A Faturar". **T-11 permanece PARCIALMENTE CONCLUÍDA** — o
PLAN-001 exige também "A Faturar".

**Não iniciadas nesta rodada:** T-12, T-13 e T-15 (nenhuma foi iniciada).

**Nesta rodada:** somente os 2 documentos alterados (`PLAN-001` e este `STATUS-002`); nenhum SQL,
build, teste, código, Home ou DI; nenhum `git add/commit/push`.

### 3.5 — PLAN-001 / T-13 — diagnóstico de ponte de dados (aditivo, 2026-10-09)

Registro **aditivo** do encerramento controlado da sessão de diagnóstico da **T-13 (Despesas)**.
Consolida **somente o que foi comprovado**; **nenhuma regra de negócio nova** foi criada e
**nenhuma implementação** foi iniciada. Detalhamento em PLAN-001 T-13 ("Diagnóstico
pré-implementação").

**Ambiente efetivo (corrigido nesta sessão):** servidor/máquina **`LOKI` / `WIN-NM5QNCQTHEP`**,
banco **`CASAMATER`**, login **`dashboard_readonly`**. A instância `DESENVHMSISAC02\MSSQLSERVER2022`
é o ambiente **local/notebook** e **não** é o alvo principal das investigações atuais (divergência
de ambiente registrada; não reconciliada por inferência).

**Permissões:** `SELECT` concedido e validado em **`dbo.PAGAR`** e **`dbo.PAGARC`** (endpoint
`SisacDatabase` corrigido; ver incidente 18456 em §3.4.2 — aqui **sem** nova ocorrência).

**Metadados confirmados (INFORMATION_SCHEMA.COLUMNS):**

- `PAGAR` (financeiro): `VALOR`, `VALORPAG`, `SALDO`, `DATAVENC`, `DATAPAG`, `DATAPREV`,
  `DataEmissao`, `CODFORNECEDOR`, `NFISCAL`, `NPARC`, `GRUPOEMP`, `FILIAL`;
- `PAGARC` (classificação): `TIPOCUSTO`, `NATOP`, `CCUSTO`, `REPASSE` (+ `VALOR`, `DATAVENC`,
  `DATAPAG`, `DATAEMISSAO`, `CODFORNECEDOR`, `NFISCAL`, `NPARC`, `GRUPOEMP`, `FILIAL`);
- **ausentes em `PAGARC`:** `VALORPAG`, `SALDO`, `DATAPREV`;
- **a classificação não existe em `PAGAR`** — reside em `PAGARC`.

**Ponte PAGAR × PAGARC (K5 = `CODFORNECEDOR`+`NFISCAL`+`NPARC`+`GRUPOEMP`+`FILIAL`):**

| Medida | Valor |
|---|---|
| `PAGAR` linhas / chaves | 212.664 / 212.664 |
| `PAGARC` linhas / chaves | 193.836 / 193.836 |
| `PAGAR` com par em `PAGARC` | 191.797 (90,19%) |
| `PAGAR` sem par | 20.867 (9,81%) |
| `PAGARC` sem par em `PAGAR` | 2.039 |
| Cardinalidade por K5 | **1:1** |
| Média / máximo `PAGARC` por chave casada | 1,0000 / 1 |
| Classificações múltiplas por chave | 0 (TIPO/NATOP/REPASSE/CCUSTO) |

**Consequências comprovadas:** K5 é **obrigatória**; **não** usar K4 (sem `NPARC`); o risco de
fan-out/dupla contagem por multiplicação fica **eliminado** pela K5; **não** somar `PAGAR.VALOR` e
`PAGARC.VALOR` juntos; o risco residual é de **cobertura/classificação**, não de multiplicação.

**Pendências humanas (não resolvidas por SQL):** (1) mapeamento `TIPOCUSTO`/`NATOP`/`CCUSTO`/`REPASSE`
→ fixa × variável; (2) tratamento dos 20.867 títulos `PAGAR` sem par; (3) data de referência
(`DataEmissao`/`DATAVENC`/`DATAPAG`/`DATAPREV`); (4) inclusão provisionada/paga/cancelada/estornada;
(5) regra de repasse médico; (6) executar as distribuições Q1–Q6 revisadas.

**Próxima retomada:** recuperar este contexto; **não** reabrir investigação encerrada; **não** repetir
grants/permissões; **não** redescobrir K5; iniciar pelas distribuições agregadas de
`TIPOCUSTO`/`NATOP`/`REPASSE`/`CCUSTO`; analisar preenchimento das datas; identificar campos de
cancelamento/estorno; levar ao gate humano; só então discutir o repositório da T-13.

**Nesta rodada:** nenhuma query de dados executada no fechamento; nenhum código, build, DI, Home ou
Git write; somente os 2 documentos de status/planejamento alterados.

## 4. R-05 — pendência aberta (bloqueio externo)

**Sequência registrada dos fatos (2026-09-28):**

1. Houve uma falha inicial de autenticação com `dashboard_readonly`.
2. Posteriormente, o login foi provisionado no SQL Server de São Luís.
3. Os testes seguintes apresentaram falha de conectividade/transporte para
   `172.16.2.138:1433`, antes de uma nova validação conclusiva de autenticação.
4. A conectividade TCP apresentou comportamento variável.

**Estado:** R-05 permanece bloqueado externamente, aguardando a infraestrutura.

**Causa:** não inferir causa de rede, ACL, firewall ou SQL sem evidência. Não
atribuir qualquer relação causal ao ZeroTier.

**Fatos que permanecem válidos:**
- SQL Server local anterior: `172.18.100.253`
- Novo SQL Server de São Luís: `172.16.2.138`
- Banco: `CASAMATER`
- Usuário da aplicação: `dashboard_readonly`
- Dashboard somente leitura (ADR-003, ADR-008)

**ZeroTier:** é uma VPN pessoal utilizada para acesso remoto ao notebook de
trabalho. Não faz parte do caminho de acesso ao SQL Server e permanece
formalmente excluído desse caminho.

**Origem desta seção:** itens da sequência 1–4 e a caracterização de
comportamento variável foram fornecidos pelo responsável do projeto em
2026-09-28 e **não foram reconfirmados por teste neste ambiente**. Tratá-los
como declaração registrada, não como medição.

## 5. Divergência documental do R-05

`docs/handoffs/2026-09-24-bloqueio-r05-casamater.md` contém uma descrição da
sequência da falha que **não corresponde à investigação efetivamente realizada**.

O histórico desse arquivo **não é alterado**. A divergência fica registrada
como divergência, e a §4 é a **versão corrigida** do estado atual do R-05.

Não tratar como estado atual nenhuma das afirmações que aquele documento faz e
que a §4 substitui, em particular:
- sequência "handshake OK → autenticação OK → timeout pós-login";
- `SELECT 1` como evidência de sucesso;
- qualquer conclusão sobre firewall, ACL, rota ou SQL sem evidência
  correspondente.

Não inventar causa alternativa para substituir a descrição registrada na §4.

**Segunda divergência, mesma natureza:** `docs/architecture/t03-acesso-sql-readonly.md`
registra, em 2026-09-17, TCP/1433 OK com logon aceito para `172.18.100.253`,
tratado ali como o SQL alcançável. O estado atual registra `172.18.100.253` como
o SQL Server local **anterior** e `172.16.2.138` como o novo alvo. As duas
informações **não são conciliadas por inferência**; a reconciliação depende da
resposta da infraestrutura. O documento histórico não é reescrito.

## 6. Tensão a resolver (decisão humana)
O grafo de dependências marca T-09..T-13 e T-19 como elegíveis, mas o
`REVIEW-T-08-2026-09-18` orienta explicitamente: *"não avançar a T-09"* enquanto
R-01..R-05 estiverem abertos. As duas leituras não concordam. Decidir antes de
retomar a execução.

> **Atualização (2026-10-07):** T-09 e T-10 foram reconciliadas como **Concluídas** (§3) — a
> tensão remanescente aplica-se a T-11..T-13/T-19. A decisão humana de 2026-10-07 definiu
> **T-14 como próxima frente técnica** (pendente de execução; não iniciada).

> **Atualização (2026-10-08):** a T-14 foi executada, validada em runtime em 2026-10-07 e
> fechada documentalmente em 2026-10-08 — ver §3.3. A nota acima permanece como registro do
> estado de 2026-10-07. **T-15 segue Pendente e não foi iniciada**; escolha da próxima frente
> **não** foi feita nesta rodada.

Em 2026-10-02, por reconciliação documental autorizada, diferenciou-se ESTADO HISTÓRICO (documentos antigos, inalterados, refletindo bloqueios das respectivas datas) de ESTADO RECONCILIADO (2026-10-02): R-01, R-02, R-03/P19 e R-05 encontram-se RESOLVIDOS posteriormente; R-04 encontra-se SUPERADO pelas validações/testes posteriores. As divergências documentais registradas em §5 não foram alteradas. Os handoffs históricos permanecem inalterados.

## 7. Investigações encerradas — não reabrir
| Encerramento | Fonte |
|---|---|
| ATD-VIS-04 e ATD-VIS-05 (gate visual) | `2026-09-25-fechamento-formal-atd-vis-04-atd-vis-05.md` |
| Schema `ACESSOW`/`ACESSOUSUW`/`USUARIO` | `2026-09-21-encerra-investigacao-schema-...md` |
| Autenticação Delphi (`Login.pas`, `ElsoftProc.pas`) | `2026-09-21-fechamento-autenticacao.md` |
| `user_token` / `cod_fi` / servidor `:8022` | `2026-09-22-fechamento-user-token-codfi.md` |
| Identidade e sessão do Dashboard API | `2026-09-22-investigacao-identidade-sessao-dashboard-api.md` |
| Frente Atendimentos por Convênio | `2026-09-23-encerra-frente-investigacao-atendimento-convenio.md` |
| Frente Atendimentos por Sexo | `2026-09-23-encerra-frente-investigacao-atendimento-sexo.md` |
| T-ATD-02 (análise documental) | `2026-09-23-t-atd-02-definicao-analitica-atendimento.md` |
| Diretórios com prefixo `old`/`OLD` | `2026-09-23-handoff-encerramento-sessao.md` |
| SghProg / SisacHTML5 como contrato de dados | `2026-09-22-fechamento-vertical-slice-dashboard.md` |
| Gate de conexão T-05 | `2026-09-17-checkpoint.md` |

## 8. Itens sem lastro documental no repositório
- **`INFRA-01`** — não há registro de ticket em `docs/`
- **ZeroTier** — não há menção a ZeroTier em `docs/`; o fato é declaração do
  responsável, registrada na §4

Nenhum dos dois foi encerrado por documento anterior. Se devem constar como
investigações concluídas, precisam de um documento que os registre.

## 9. Regras de conduta ao retomar
- Não repetir diagnóstico de conectividade enquanto R-05 estiver bloqueado
- Não inventar resultado de teste ausente — a lacuna é o achado
- Não criar handoff por encerramento rotineiro de sessão; atualizar este arquivo
- Não alterar `Dashboard.Web` fora do escopo da tarefa
- Não alterar documentos históricos para "corrigir" divergência — registrar a
  divergência
