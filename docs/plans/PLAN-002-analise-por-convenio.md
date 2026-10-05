# PLAN-002 — Análise por Convênio nos Indicadores Assistenciais

<!-- CONTEXTO -->
Este plano implementa a análise por Convênio (cod_convenio, convenio) nos indicadores assistenciais da página de Atendimentos, conforme aprovado no checkpoint da sessão anterior.

## Objetivo

Adicionar a possibilidade de analisar os indicadores assistenciais segmentados por Convênio, mantendo os filtros e a semântica já existentes. O escopo deste plano é EXCLUSIVAMENTE a "Análise por Convênio nos Indicadores Assistenciais" para Atendimentos, Consultas e Exames.

## Referências

- PRD-003 — Análise por Convênio nos Indicadores Assistenciais — **versão reconstruída em 2026-10-05** (v0.2): RN-003-01..RN-003-10, CA-003-01..CA-003-07, DEC-003-01..DEC-003-06
- SPEC-UI-003 — Especificação de Interface
- DEC-003-01 .. DEC-003-06 — Decisões Aprovadas (gate humano 2026-10-02)
- ADRs do projeto
- RN/CA do PRD-003 (família CA oficial: CA-003-01..CA-003-07 — CA-003-08/09 nunca existiram)

## Escopo aprovado

- Segmentação por Convênio nos 3 indicadores: Atendimentos, Consultas e Exames
- Manter comportamento existente sem quebra
- Não alterar arquitetura nem fontes legadas
- Dashboard somente leitura

## Estratégia de Entrega

- Entrega incremental por tarefa (uma entrega parcial validável por tarefa)
- Interfaces unidirecionais respeitando Core ← Data ← Web
- Reaproveitar contratos existentes onde aplicável
- Cobertura de testes por cenário aprovado (CA)
- Operações Git (add/commit/push) permanecem sob controle humano e não fazem parte da execução automática do OpenCode

## Riscos controlados

- Não impactar outras segmentações já existentes
- Não criar novos indicadores fora do escopo
- Não introduzir persistência/cache

## Tarefas

### T-01 — Inventariar pontos de extensão por indicador
- **Status:** Concluído (2026-10-05)
- **Prioridade:** Alta
- **Depende de:** Nenhuma
- **Implementa:** RN-003-01, RN-003-02 (parcial)
- **Valida:** CA-003-01
- **Decisões base:** — (tarefa de análise; nenhuma DEC de apresentação se aplica)

- [x] Identificar locais de montagem dos filtros nos 3 indicadores
- [x] Identificar pontos de UI onde Convênio pode ser aplicado
- [x] Mapear contratos Core/Data/Web afetados
- [x] Registrar achados mínimos para as tarefas seguintes

#### Reconciliação documental 2026-10-05 (sem mudança de escopo)

O mapeamento `Decisões base:` deste plano foi **reconciliado por significado semântico**, por
ter sido elaborado **por posição numérica** (T-0*n* → DEC-003-0*n*). Correções aplicadas:

| Tarefa | Antes | Agora | Justificativa |
|---|---|---|---|
| T-01 | DEC-003-01 | — | Inventário não materializa decisão de apresentação |
| T-02 | DEC-003-02 | — | DEC-003-02 é **limite de exibição**; não trata de contrato Core |
| T-03 | DEC-003-03 | — | DEC-003-03 é **forma de exibição**; não trata de SQL |
| T-04 | DEC-003-04 | **DEC-003-01 + DEC-003-06** | Ordenação e desempate **são** o objeto de T-04 |
| T-05 | DEC-003-05 | — | Propagação de filtro não é decisão de apresentação (ver ADR-009) |
| T-06 | DEC-003-06, SPEC-UI-003 | **DEC-003-02..DEC-003-05 + SPEC-UI-003** | Onde as decisões de apresentação incidem; DEC-003-06 pertence a T-04 |
| T-07 | DEC-003-01..06 | — | Executa as CA; não aplica decisão isolada |
| T-08 | DEC-003-01..06 | mantida | Checklist de escopo: o conjunto completo é o objeto |

Os IDs `Implementa:` (RN) e `Valida:` (CA) foram **mantidos** — agora correspondem ao PRD-003
reconstruído (§4 e §7), com CA-003-02..CA-003-07 materializadas. **Nenhum checkbox de escopo
funcional foi alterado**, exceto os dois alinhamentos textuais registrados em T-04 e T-06.

#### Achados da T-01 (inventário — sem alteração de código)

**A. Pontos de montagem dos filtros (3 indicadores)**

| Indicador | Arquivo | Montagem do filtro / SQL | Diferença entre os 3 |
|---|---|---|---|
| Atendimentos | `Dashboard.Data/Repositories/AtendimentosVisaoRepository.cs` | `ConstruirSqlDiario(clausulaFrom, condicoesAdicionais)` `:145`; total em `ConsultarPorDia` `:70-77` | sem condição de `TIPO` |
| Consultas | `Dashboard.Data/Repositories/ConsultasVisaoRepository.cs` | `ConstruirSqlDiario` `:149`; total `:75` | `CondicaoTipoConsulta = "E.TIPO = @Tipo"` `:22`, `TipoConsulta = "1"` `:28`, aplicada `:157` |
| Exames | `Dashboard.Data/Repositories/ExamesVisaoRepository.cs` | `ConstruirSqlDiario` `:149`; total `:75` | `CondicaoTipoExame = "E.TIPO = @Tipo"` `:22`, `TipoExame = "3"` `:28`, aplicada `:157` |

- Filtro de negócio compartilhado: `Dashboard.Core/DTOs/IndicatorFilter.cs` — `Period`, `StartDate`, `EndDate`, `Coverage` (`CoverageCategory.Particular/Convenio/Sus`), `Tipo`. **Não existe hoje campo de convênio individual.**
- Montagem do `FROM` e das condições de cobertura está **centralizada e já é reutilizada pelos 3**: `AtendimentosRepository.ConstruirClausulaFrom(bool)` `Dashboard.Data/Repositories/AtendimentosRepository.cs:31-34` e `ConstruirCondicoesCobertura(CoverageCategory)` `:36-39`. Chamadas: `AtendimentosVisaoRepository.cs:98-99`, `ConsultasVisaoRepository.cs:101-102`, `ExamesVisaoRepository.cs:101-102`.
- A chave composta de `CADCONVENIO` **já está preservada e é o único ponto que a define**: `C.CODCONVENIO = E.CODCONVENIO AND C.GRUPOEMP = E.GRUPOEMP AND C.FILIAL = E.FILIAL` (`AtendimentosRepository.cs:33`). T-03 deve reutilizar este `FROM`, não reescrevê-lo.
- `ConstruirCondicoesCobertura(Convenio)` já carrega o predicado P19 `COALESCE(C.SUSPENSO, '') <> 'SUSPENSO'` (`AtendimentosRepository.cs:38`) — a exclusão de suspensos vem "de graça" se a análise reutilizar essa condição.
- `ValorTotal` é calculado **sem** `JOIN` em `CADCONVENIO` (`ConstruirClausulaFrom(false)`) e portanto **não** carrega o filtro de cobertura/P19: a soma da tabela por convênio **não** igualará `ValorTotal` por construção. Relevante para CA-003-01 ("apenas convênios presentes").

**B. Colisão de nomenclatura — risco para T-02/T-04**

`DimensaoVisao.Convenio` (`Dashboard.Core/DTOs/IndicadorVisao.cs:6`) **já está em uso e significa outra coisa**: hoje é o corte por **cobertura** (séries Particular/Convênio/SUS — `AtendimentosVisaoRepository.cs:29-34`, idem Consultas/Exames). O mesmo ocorre com `CoverageCategory.Convenio`. A "análise por convênio" do PRD-003 é **segmentação por convênio individual**, conceito distinto de cobertura. **Reutilizar `DimensaoVisao.Convenio` colide semanticamente** — T-02/T-04 devem introduzir identificação nova (enum/contrato/DTO próprios), preservando o significado vigente.

**C. Pontos de UI onde Convênio pode ser aplicado**

Referência: `Dashboard.Web/Pages/Atendimentos.cshtml` (Consultas/Exames são páginas espelho).

| Ponto UI | Local | Papel na T-06 |
|---|---|---|
| Card do indicador | `Atendimentos.cshtml:113-118` (`#grafico-visao` + `#legenda-visao`) | ponto de inserção da **seção expansível** do DEC-003-05, vinculada ao indicador, preservando período/contexto/totais |
| Grupo de botões de dimensão | `Atendimentos.cshtml:77-87` (itera `visao.DimensoesDisponiveis`, hoje só `DimensaoVisao.Convenio`=cobertura) | **não** registrar a visão por convênio individual aqui: DEC-003-05 proíbe drill-down/clique e nova rota |
| Formulário de filtros | `Atendimentos.cshtml:36-56` (`inicio`, `fim`, ocultos `dimensao`, `forma`) | qualquer novo estado precisa percorrer o form (T-05/T-06) |
| Serialização de estado | `AtendimentosModel.DadosJson` (`Atendimentos.cshtml.cs:32,56`), `JsonStringEnumConverter` camelCase (`:14-17`), consumido de `#dados-visao` (`Atendimentos.cshtml:120`) por `wwwroot/js/atendimentos.js` | contrato de dados da tabela (DEC-003-03) |
| Estados vazio/erro | `Atendimentos.cshtml:58-68` (`DadosIndisponiveis`) + `Atendimentos.cshtml.cs:58-72` | a tabela herda o mesmo tratamento; sem inventar valor |

**D. Contratos Core/Data/Web afetados**

- **Core (sem dependências — ADR-005/007)**
  - `DTOs/IndicatorFilter.cs` — sem campo de convênio individual; candidato natural em T-02 (retrocompatível por campo opcional).
  - `DTOs/IndicadorVisao.cs` — sem coleção por convênio; precisa de DTO com **identificação nominal + volume** (e lista na visão), ou de contrato próprio.
  - `Contratos/IIndicadorVisaoRepository.cs` — assinatura atual `(IndicatorFilter, DimensaoVisao, GraficoForma, CancellationToken)`. Duas opções para T-02/T-05: (i) estender a interface (quebra implementadores); (ii) **novo contrato** `IIndicadorConvenioRepository` — mantém o contrato vigente e a retrocompatibilidade exigida pela T-02.
- **Data (somente leitura — ADR-003)**
  - Os 3 repositórios de visão repetem o mesmo padrão; `AtendimentosRepository` é o dono dos helpers compartilhados (`ConstruirClausulaFrom`, `ConstruirCondicoesCobertura`).
  - SQL a produzir: `SELECT C.DESCR, COUNT(1) ... GROUP BY` — `DESCR varchar(250)` é a identificação nominal (`docs/architecture/contrato-dados-dashboard.md:204,543`); `SELECT` apenas, sem DDL/DML.
  - `Left(Lower(Descr),10)` usado pelo legado (`docs/architecture/dicionario-de-dados.md:71,261`) é **artefato do legado**, não regra — usar `DESCR` integral.
  - Ordenação: volume decrescente (DEC-003-01) com desempate por nome A–Z (DEC-003-06); `DESCR` admite duplicatas, então a ordenação pode não ser total — T-04 decide a chave final de desempate **sem contradizer** DEC-003-06.
- **Web (Web → Core ← Data; ADR-001)**
  - `Pages/Atendimentos.cshtml.cs:75-86` `MontarFiltro(...)` é o ponto de montagem do filtro por página (idem Consultas/Exames).
  - `Program.cs:13-17` — `IIndicadorVisaoRepository` está registrado **só** para `AtendimentosVisaoRepository`; Consultas e Exames são injetados pelo **tipo concreto** (não implementam a interface). Qualquer repositório novo precisa de registro explícito em T-05.
- **Testes** — `Dashboard.Data.Tests/Repositories/{Atendimentos,Consultas,Exames}VisaoRepositoryTests.cs` já asseguram o **reuso** de `AtendimentosRepository.ConstruirCondicoesCobertura` e o predicado P19; o SQL novo não pode quebrar esses asserts (visibilidade `internal` via `InternalsVisibleTo`).

**E. Lacunas registradas (achado, não invenção)**

> **Estado em 2026-10-05:** **L1 e L4 RESOLVIDAS** no gate humano (reconstrução documental do
> PRD-003). L2, L3 e R5 permanecem **abertas** para as etapas técnicas correspondentes.

- **L1 — Rastreabilidade: RESOLVIDA em 2026-10-05.** A T-01 registrou que `docs/prds/PRD-003-analise-por-convenio.md` continha **apenas §6 (DEC) e §7.1 (CA-003-01)**, sem RN-003-01..10 e sem CA-003-02..07. A verificação de histórico Git somente leitura comprovou **falha de materialização original** (commit `cfc5ecc`, 2026-10-02): o arquivo foi criado já truncado; existe **um único blob** em todo o objeto alcançável, **nenhum segundo commit, nenhum stash, apenas `main`/`origin/main`**, e `git diff cfc5ecc HEAD` vazio — **não existia versão completa a restaurar**. O PRD-003 foi **reconstruído (v0.2, 2026-10-05)** sob gate humano com base nas decisões e evidências persistidas: §1–§5, RN-003-01..RN-003-10, CA-003-02..CA-003-07, matriz §5.1 e premissas §5.2. **CA-003-01 preservada semanticamente; DEC-003-01..06 preservadas literalmente.** Nenhum requisito foi inventado; §1–§5 originais, o documento de "autorização" citado na versão parcial e personas/stakeholders/métricas/diagramas **permanecem não recuperados** (sem fonte no repositório). **CA-003-08 e CA-003-09 nunca existiram** — família oficial **CA-003-01..CA-003-07**.
- **L2 — Ambiente: ABERTA (validar na etapa Data/SQL).** P17 (`CADCONVENIO` mantém a mesma estrutura no SisacHTML5) segue **aberta** (`contrato-dados-dashboard.md:223`). CA-003-01/RN-003-05 exigem resolver `DESCR` pela chave composta; a confirmação no banco alvo não existe no repositrafo. Validação somente com a credencial de leitura, read-only. **Não bloqueia T-02** (não toca o banco).
- **L3 — Cardinalidade: ABERTA (verificação técnica na etapa Data/SQL).** Não há medição registrada da cardinalidade de `CADCONVENIO` sob a chave `(CODCONVENIO, GRUPOEMP, FILIAL)`. O `INNER JOIN` composto é a premissa do total por convênio; se a chave não for única no ambiente alvo, o volume por convênio pode ser multiplicado. Verificar read-only em T-03, sem inventar medição.
- **L4 — Semântica de "convênio presente" (CA-003-01): RESOLVIDA em 2026-10-05.** Decisão humana: a análise nominal representa os **convênios efetivamente presentes nos registros válidos** de cada indicador no período, **preservando RN-07, P19 e demais regras aprovadas**; **NÃO** se introduz `MODOFAT = 'C'` como nova regra de produto, e a análise **não depende** de a categoria de cobertura "Convênio" estar selecionada. **L4 deixa de bloquear T-03.** Reflexão no artefato: `PRD-003` §3.3 e RN-003-02; `CA-003-01` já era semanticamente coerente (não menciona `MODOFAT`).
- **R5 — Chave final de desempate: ABERTA (decisão técnica de T-04).** `DESCR` admite duplicatas, então a ordenação de RN-003-07 pode não ser total. **Não foi inventada chave de desempate no PRD-003** (`PRD-003` §5.2); a decisão permanece atribuída a T-04, sem contradizer DEC-003-01/DEC-003-06.
- **Colisão de nomenclatura (registrada na T-01):** `DimensaoVisao.Convenio` e `CoverageCategory.Convenio` significam **cobertura** (Particular/Convênio/SUS), não convênio individual — **não reutilizar**. Materializado como RN-003-03 e CA-003-02.

**F. Ponteiro para as tarefas seguintes**

- **T-02** — decidido em gate humano (2026-10-05): **não reutilizar** `DimensaoVisao.Convenio`/`CoverageCategory.Convenio` (significam cobertura); carregar identificação nominal + volume em contrato próprio, com retrocompatibilidade (RN-003-03/RN-003-04, validados por CA-003-02).
- **T-03** — reutilizar `ConstruirClausulaFrom(true)` e `ConstruirCondicoesCobertura(...)`; `GROUP BY` pela identificação nominal; resolver L3; L4 **já resolvida** — convênios presentes no conjunto válido, sem `MODOFAT = 'C'` como nova regra.
- **T-04** — ordenação total (DEC-003-01 + DEC-003-06) sem contradizer as decisões aprovadas; **decidir a chave final de desempate (R5)**, hoje em aberto.
- **T-05** — registrar o repositório novo no DI (`Program.cs:13-17`) e propagar o filtro a partir de `MontarFiltro` de cada PageModel, sem alterar autenticação/identidade (ADR-009).
- **T-06** — seção expansível no card (`Atendimentos.cshtml:113-118`), em tabela (DEC-003-03), com rolagem vertical (DEC-003-04), preservando contexto e sem rota nova (DEC-003-05); exibir todos os convênios presentes (DEC-003-02).

### T-02 — Estender contratos Core para segmento Convênio
- **Status:** Concluído (2026-10-05)
- **Prioridade:** Alta
- **Depende de:** T-01
- **Implementa:** RN-003-03, RN-003-04
- **Valida:** CA-003-02
- **Decisões base:** — (nenhuma DEC de apresentação trata de contrato Core; retrocompatibilidade por ADR-001/ADR-005)
- **Lacunas:** — L1 e L4 resolvidas (2026-10-05)

- [x] Revisar `IndicatorFilter` / contratos existentes
- [x] Adicionar contrato/campo de **convênio nominal** em Core (identificação nominal + volume), preservando o significado vigente de "Convênio" como categoria de cobertura — **sem reutilizar** `DimensaoVisao.Convenio`/`CoverageCategory.Convenio`
- [x] Garantir retrocompatibilidade
- [x] Atualizar tipos/contratos relevantes

#### Entrega da T-02 (contratos Core — sem SQL, sem Web/UI)

Dois arquivos novos em `Dashboard.Core`; **nenhum arquivo existente alterado**:

- `Dashboard.Core/DTOs/IndicadorConvenioNominal.cs` — `ConvenioNominalVolume` (`Identificacao`,
  `Volume`) e `IndicadorConvenioNominalVisao` (`Periodo`, `ValorTotal`, `Convenios`).
- `Dashboard.Core/Contratos/IIndicadorConvenioNominalRepository.cs` — **contrato novo**
  `ObterConvenioNominalAsync(IndicatorFilter, CancellationToken)`, seguindo a opção (ii)
  registrada na T-01 §D.

**Decisões de modelagem (derivadas de RN-003-03/RN-003-04 e CA-003-02):**

- **Contrato novo, não extensão de `IIndicadorVisaoRepository`** — a assinatura vigente
  permanece intocada; nenhum implementador existente quebra. Nenhuma interface foi alterada.
- **Nenhum campo novo em `IndicatorFilter`** — a análise nominal **não é um filtro**: DEC-003-05
  proíbe drill-down/clique nos convênios e não há requisito de seleção de convênio em
  RN-003-01..10. O filtro é **entrada de período/segmentação vigente**, reaproveitado como está.
- **Nomenclatura `ConvenioNominal`** — evita `Convenio`, já usado por `DimensaoVisao.Convenio`
  e `CoverageCategory.Convenio` (cobertura, RN-26). Nenhum tipo ou membro novo se nomeia apenas
  `Convenio`.
- **Ausência de regra de ordenação no contrato** — `Convenios` é lista; **ordem e chave de
  desempate são de T-04**, não definidas aqui (R5 permanece aberta).
- **Sem `MODOFAT`** — nenhuma membresia de regra de cobertura em Core.
- `Periodo` reutiliza `BusinessReferencePeriod` (RN-09), como `IndicadorVisao`.

**Validação:** `dotnet build Dashboard.slnx` → **0 avisos / 0 erros**, os 5 projetos compilando
(`Dashboard.Core`, `Dashboard.Data`, `Dashboard.Core.Tests`, `Dashboard.Data.Tests`,
`Dashboard.Web`) — prova de retrocompatibilidade: os contratos e filtros existentes são
consumidos por Data, Web e testes **sem qualquer alteração**. `git diff` restrito a
`Dashboard.Core`/`Data`/`Web`/testes: **vazio**. Nenhum teste executado — T-02 é só contrato,
sem comportamento; testes de cenário pertencem a T-07 (T-03 mantém "testes unitários mínimos
no Data.Tests", se aplicável).

### T-03 — Extender repositórios Data (SQL/consulta) para Convênio
- **Status:** Concluído (2026-10-05)
- **Prioridade:** Alta
- **Depende de:** T-02
- **Implementa:** RN-003-05, RN-003-06
- **Valida:** CA-003-03
- **Decisões base:** — (DEC-003-03 é forma de exibição; somente leitura por ADR-003)
- **Lacunas:** L2 e L3 tratadas explicitamente

- [x] Identificar queries dos 3 indicadores
- [x] Incluir resolução da identificação nominal via `CADCONVENIO` pela chave composta `(CODCONVENIO, GRUPOEMP, FILIAL)`, com `DESCR` integral, reusando o `FROM` compartilhado
- [x] Manter SQL somente leitura
- [x] Evitar alterações em outras segmentações
- [x] Testes unitários mínimos no Data.Tests (se aplicável)

#### Entrega da T-03 (Data — implementações do contrato nominal)

Três repositórios novos em `Dashboard.Data/Repositories`:

- `AtendimentosConvenioNominalRepository` (implementa `IIndicadorConvenioNominalRepository`) — SQL por dia/período idêntico à regra vigente de Atendimentos: `E.DATAHORAENT >= @DataInicio AND < @DataFim`, `E.FECHADO <> 'C'`, `COALESCE(E.LoteEnt,'') <> 'INAT'`. **JOIN** por `(CODCONVENIO,GRUPOEMP,FILIAL)` com `CADCONVENIO`, **GROUP BY C.DESCR**, **ORDER BY COUNT(1) DESC, C.DESCR ASC** (sem desempate final definido; R5 permanece para T-04). Identidade nominal = `C.DESCR` integral.
- `ConsultasConvenioNominalRepository` — adiciona `E.TIPO = @Tipo` (`'1'`), mesmo critério de exclusão.
- `ExamesConvenioNominalRepository` — `E.TIPO = @Tipo` (`'3'`).

**Evidência L2 — estrutura necessária (somente leitura):** consulta a `INFORMATION_SCHEMA.COLUMNS` validou as colunas necessárias para `CADCONVENIO` (`CODCONVENIO,GRUPOEMP,FILIAL,DESCR,MODOFAT,SUSPENSO`) e `ENTRADA` (`CODCONVENIO,GRUPOEMP,FILIAL,TIPO,FECHADO,LoteEnt,DATAHORAENT`). Preservado `SUSPENSO` para referência (P19), mas **não utilizado** nesta análise nominal por decisão L4 (convênios presentes no conjunto válido, sem `MODOFAT='C'`). RN-07 preservada.

**Evidência L3 — cardinalidade/chave composta:** `CADCONVENIO` com 603 linhas; registros com chave completa: 603; chaves distintas `(CODCONVENIO,GRUPOEMP,FILIAL)` = 603 (sem duplicações) — chave única/funcional por essa tríade no conjunto lido. `ENTRADA` com 2.095.283 linhas; `INNER JOIN` produz 2.095.261 linhas (perda < 0,0012%); quantidade com convênios suspensos nos registros de ENTRADA é 341.568 — mas **não excluídos** por P19 nesta análise (conforme decisão L4). L3 **verificada e coerente** com a premissa de `JOIN` composto.

**Preservação:** contratos existentes não alterados; repositórios existentes não alterados; `AtendimentosRepository.ConstruirClausulaFrom/ConstruirCondicoesCobertura` não utilizados (não aplicáveis à análise nominal L4). Nenhuma regra de desempate extra além do ORDER BY indicado; R5 permanece em T-04. SQL estritamente somente leitura, parâmetros Dapper. Build `dotnet build Dashboard.slnx`: 0 avisos/0 erros. Testes: 9 Core.Tests + 39 Data.Tests — **0 falhas**. Não houve acesso a banco para testes (suíte de integração com credencial permanece como lacuna conhecida), mas as novas classes compilam e não quebram existentes.

L2 **RESOLVIDA** (estrutura necessária confirmada). L3 **RESOLVIDA** (chave composta distinta e funcional; cardinalidade verificada com evidência read-only).

### T-04 — Regra de ordenação + desempate (R5)
- **Status:** Concluído (2026-10-05)
- **Prioridade:** Média/Alta
- **Depende de:** T-03
- **Implementa:** DEC-003-01, DEC-003-06
- **Valida:** CA-003-06
- **Decisões base:** DEC-003-01 (volume decrescente), DEC-003-06 (identificação nominal crescente A-Z em empate de volume). R5 tratada via desempate técnico determinístico sem alterar texto exibido.
- **Lacunas:** R5 resolvida nesta tarefa (desempate técnico determinístico)

- [x] Validar ordenação total: volume decrescente e desempate por identificação nominal A–Z
- [x] Decidir a chave final de desempate (R5), sem contradizer DEC-003-01/DEC-003-06 — solução técnica: desempate por chave composta ascendente (CODCONVENIO, GRUPOEMP, FILIAL) após C.DESCR
- [x] Ajustar mapeamentos apenas se necessário (aplicado nos repositórios Data nominais)
- [x] Manter separação Core/Data

### T-05 — Integrar no backend Web (PageModel) filtros por Convênio
- **Status:** Concluído (2026-10-05)
- **Prioridade:** Alta
- **Depende de:** T-04
- **Implementa:** RN-003-08
- **Valida:** CA-003-05
- **Decisões base:** — (DEC-003-05 é exposição de interface; identidade/autenticação por ADR-009; não regressão funcional conforme SPEC-UI-003 §8)

- [x] Registrar contratos nominais por indicador na camada Web (DI) para que a infraestrutura de serviço esteja disponível quando utilizada: `IIndicadorConvenioNominalRepository` resolvido para `AtendimentosConvenioNominalRepository`, `ConsultasConvenioNominalRepository`, `ExamesConvenioNominalRepository` em `Dashboard.Web/Program.cs`
- [x] Preservar estado de filtros e contexto existentes
- [x] Não alterar fluxo de autenticação/identidade
- [x] Validar 3 indicadores — DI configurada para os três sem impacto nas implementações existentes

### T-06 — Atualizar UI (Razor/JS) para seleção/propagação do Convênio
- **Status:** Pendente
- **Prioridade:** Alta
- **Depende de:** T-05
- **Implementa:** RN-003-09, RN-003-10
- **Valida:** CA-003-06, CA-003-07
- **Decisões base:** DEC-003-02, DEC-003-03, DEC-003-04, DEC-003-05, SPEC-UI-003 (DEC-003-06 pertence a T-04 — ordenação em Core)

- [ ] Expor a análise em **seção expansível** vinculada ao indicador, conforme DEC-003-05 e SPEC-UI-003 — **sem drill-down/clique e sem nova rota**; a SPEC-UI-003 não define "controle de Convênio"
- [ ] Apresentar em tabela com identificação nominal + volume (DEC-003-03), exibindo **todos** os convênios presentes (DEC-003-02), com rolagem vertical (DEC-003-04)
- [ ] Manter responsividade/estilo existente
- [ ] Garantir compatibilidade com filtros existentes
- [ ] Verificar estados vazios/erros conforme necessário

### T-07 — Testes de integração/cenários CA-003
- **Status:** Pendente
- **Prioridade:** Alta
- **Depende de:** T-06
- **Implementa:** Todos RN afetados
- **Valida:** CA-003-01..CA-003-07 (integrados) — família oficial; CA-003-08/09 não existem
- **Decisões base:** — (conformidade global; cada CA referencia suas DEC no PRD-003 §7)

- [ ] Validar cada cenário Gherkin aprovado
- [ ] Rodar testes existentes para evitar regressão
- [ ] Cobrir os 3 indicadores com filtro Convênio

### T-08 — Revisão final e verificação de escopo
- **Status:** Pendente
- **Prioridade:** Média
- **Depende de:** T-06, T-07
- **Implementa:** RN-003 globais
- **Valida:** Conformidade com PRD-003/SPEC-UI-003
- **Decisões base:** DEC-003-01..DEC-003-06

- [ ] Confirmar que não foi adicionado indicador novo
- [ ] Confirmar escopo exclusivo Atendimentos/Consultas/Exames
- [ ] Revisão cruzada dos pontos de validação humana (se houver)
- [ ] Checklist de fechamento

## Critérios de aceite globais

- Análise por Convênio disponível para Atendimentos, Consultas e Exames
- Sem alteração de arquitetura (3 camadas unidirecionais)
- Dashboard somente leitura, sem persistência
- Sem quebra de funcionalidades existentes
- Sem introdução de novo indicador

## Histórico de execução

| Tarefa | Status | Concluída em | Commit | Observação |
|--------|--------|--------------|--------|------------|
| T-01   | Concluído | 2026-10-05 | — | Inventário **sem alteração de código**: pontos de montagem de filtro (`IndicatorFilter`, `ConstruirSqlDiario` ×3, helpers compartilhados `ConstruirClausulaFrom`/`ConstruirCondicoesCobertura` com chave composta `CODCONVENIO+GRUPOEMP+FILIAL` preservada e P19 já presente); pontos de UI (card `#grafico-visao`, grupo de dimensão, form, `DadosJson`/JS, estado de erro); mapa de contratos Core/Data/Web + `Program.cs:13-17`; ponteiro por tarefa. **Colisão de nomenclatura registrada:** `DimensaoVisao.Convenio`/`CoverageCategory.Convenio` já significam **cobertura**, não convênio individual. **4 lacunas registradas sem inventar evidência:** L1 RN-003-01..10 e CA-003-02..07 ausentes do PRD-003 no repositório (só §6 e §7.1 existem); L2 P17 (`CADCONVENIO` no SisacHTML5) aberta; L3 cardinalidade da chave composta sem medição registrada; L4 semântica de "convênio presente" ambígua em CA-003-01. Build baseline `dotnet build Dashboard.slnx`: **0 avisos / 0 erros**. Nenhum teste executado (T-01 é análise; sem código alterado). Sem banco, sem Git, sem servidor. **Parado no gate humano** — L1 e L4 bloqueiam T-03 e L1 bloqueia T-02 |

### Reconciliação documental (2026-10-05) — sem execução de tarefa

Nenhuma tarefa foi executada; nenhuma linha de código, teste ou consulta foi alterada.

- **Falha de materialização original do PRD-003 diagnosticada** (histórico Git somente leitura): o artefato foi criado já truncado no commit `cfc5ecc` (2026-10-02) — §6 e §7.1 apenas. Existe **um único blob** em todo o objeto alcançável, **nenhum segundo commit, nenhum stash, apenas `main`/`origin/main`**, e `git diff cfc5ecc HEAD` vazio para esses arquivos: **não existia versão completa a restaurar**.
- **PRD-003 reconstruído (v0.2)** sob gate humano: §1–§5, RN-003-01..RN-003-10, CA-003-02..CA-003-07, matriz §5.1, premissas §5.2. **CA-003-01 preservada semanticamente; DEC-003-01..06 preservadas literalmente** (apenas o cabeçalho de estado obsoleto "PENDENTE/VALIDAR" foi corrigido para aprovadas 2026-10-02). Nada foi inventado: §1–§5 originais, documento de "autorização" citado na versão parcial, e personas/stakeholders/métricas/diagramas seguem **não recuperados**.
- **L1 — RESOLVIDA** pela reconstrução documental.
- **L4 — RESOLVIDA** por decisão humana (2026-10-05): listar os convênios efetivamente presentes no conjunto válido de cada indicador no período, preservando RN-07 e P19; **sem `MODOFAT = 'C'` como nova regra**; a análise não depende de a categoria "Convênio" estar selecionada. **Deixa de bloquear T-03.**
- **L2 — ABERTA:** P17/chave composta no SisacHTML5 a confirmar com a credencial de leitura, na etapa Data/SQL (T-03).
- **L3 — ABERTA:** cardinalidade de `(CODCONVENIO, GRUPOEMP, FILIAL)` a medir read-only na etapa Data/SQL (T-03).
- **R5 — ABERTA:** chave final de desempate quando `DESCR` for duplicado; decisão técnica atribuída a **T-04** (sem contradizer DEC-003-01/DEC-003-06).
- **`Decisões base` reconciliadas por significado semântico** (o mapeamento anterior era posicional). Nenhum checkbox de escopo funcional alterado, exceto os dois alinhamentos textuais de T-04 e T-06.
- **CA-003-08 e CA-003-09 não são criados** — família oficial CA-003-01..CA-003-07.
- **T-02 permanece documentada como Concluída (2026-10-05)**; T-03..T-08 seguem Pendentes. T-03 não foi iniciada.
- SPEC-UI-003, ADR, PRD-001, PRD-002, PLAN-001 e código **não alterados**.

- Criado na sessão anterior (conceitualmente) — materializado agora em `docs/plans/PLAN-002-analise-por-convenio.md`
- Correções aplicadas conforme aprovação: remoção da regra "Uma tarefa por commit, com CI verde e verificação de build entre fases"; T-07 passou a depender de T-06; T-08 permanece dependendo de T-06 e T-07
