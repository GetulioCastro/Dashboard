# PRD-003 — Análise por Convênio nos Indicadores Assistenciais

> **Versão:** reconstruída em **2026-10-05** sob gate humano, a partir das decisões e
> evidências já aprovadas e persistidas no ciclo. Substitui, sem apagar o histórico, o
> artefato materializado parcialmente em 2026-10-02 (commit `cfc5ecc`).
>
> **Procedência e falha de materialização original (fato verificado):** este artefato foi
> criado no commit `cfc5ecc` já **truncado**, começando em `## 6.` — 50 linhas, das quais
> só §6 (DEC-003-01..DEC-003-06) e §7.1 (CA-003-01) tinham sido gravadas. A verificação de
> histórico Git somente leitura (2026-10-05) confirmou: **um único blob por arquivo em todo
> o objeto alcançável** (`PRD-003` = `fe3758f`, `SPEC-UI-003` = `42a1416`), **nenhum
> segundo commit**, **nenhum stash**, **apenas `main`/`origin/main`**, e
> `git diff cfc5ecc HEAD` vazio para estes arquivos — ou seja, **nunca existiu** uma versão
> completa a restaurar. Não houve truncamento nem deleção: houve falha de captura na
> materialização original.
>
> **O que esta reconstrução é:** reconciliação de artefato SDD, **não** nova especificação
> de produto. Cada RN/CA abaixo é rastreável a uma decisão ou evidência já persistida.
> **O que não foi recuperado** (e não foi inventado): o documento de "autorização" citado na
> versão parcial ("regra 3 e 4"), §1–§5 originais, a redação normativa original de
> RN-003-01..RN-003-10 e de CA-003-02..CA-003-07, além de personas, stakeholders, métricas
> e diagramas — **ausentes na materialização original e sem qualquer fonte no repositório**.
> **Lacuna anterior registrada (`PLAN-002:104`):** RN-003-01..10 e CA-003-02..07
> **não estavam definidos no repositório**. **CA-003-08 e CA-003-09 nunca existiram** — a
> família oficial desta demanda é **CA-003-01..CA-003-07**; nenhum cenário artificial foi
> criado para preencher numeração.

## 1. Objetivo e contexto

Permitir a **análise nominal dos convênios** — identificação nominal + volume — nos
indicadores assistenciais do Dashboard (Atendimentos, Consultas e Exames), para o período
selecionado, **sem alterar** as regras vigentes dos indicadores nem a semântica da categoria
de cobertura "Convênio" já existente.

A capacidade corresponde à **reativação**, aprovada em gate humano em 2026-10-05, das regras
já arquivadas no PRD Epic:

- **RN-23 — "Distribuição de atendimentos por convênio"** (`docs/prds/PRD-001-dashboard-indicadores.md:448`)
- **RN-25 — "Apresentação em tabela/gráfico por convênio"** (`PRD-001:450`)

`PRD-001:452` condiciona o retorno das RNs arquivadas ao pedido formal dos indicadores "com
suas regras no novo modelo" — **este documento é esse pedido formal**. **RN-24**
("Convênios filtrados por unidade", `PRD-001:449`) **não** é reativada: unidade permanece fora
do escopo (RN-45, `PRD-001:218`).

## 2. Escopo

### 2.1 Incluído

- Análise nominal por convênio nos três indicadores assistenciais: **Atendimentos,
  Consultas e Exames**.
- Análise sobre o **período selecionado**, com o **conjunto válido** do próprio indicador.
- Para cada convênio presente: **identificação nominal** e **volume correspondente**.
- Apresentação em tabela, ordenada conforme as DEC aprovadas (§6).
- Estados vazio e de erro herdados do tratamento já existente na página do indicador.

### 2.2 Excluído

- **Top N** e agrupamento "Outros" (`DEC-003-02`).
- **Paginação** (`DEC-003-04`).
- **Drill-down/clique** sobre convênios e **nova rota/página dedicada** (`DEC-003-05`).
- Barras horizontais dentro da tabela (`DEC-003-03`).
- **Filtro por unidade** (RN-45; RN-24 não reativada).
- **Nova regra de produto por `MODOFAT = 'C'`** — ver §3.3 e §5.2 (decisão humana de 2026-10-05).
- Alteração de autenticação, identidade ou autorização (ADR-009).
- Qualquer escrita, DDL, DML ou persistência no banco (ADR-003, ADR-008).
- Indicadores novos, novas segmentações ou alteração das segmentações existentes.

## 3. Definições e distinções conceituais

### 3.1 Categoria de cobertura "Convênio" (conceito **existente** — RN-26)

Recorte por **categoria de cobertura** — Particular, Convênio, SUS — previsto em RN-26
(`PRD-001:208-212`). Para a categoria Convênio, convênio suspenso **não** é "efetivamente
cadastrado" (decisão de produto P19, `PRD-001:220-224`; predicado técnico em
`docs/architecture/contrato-dados-dashboard.md:226` — `COALESCE(C.SUSPENSO, '') <> 'SUSPENSO'`,
com `DATASUSP` fora do predicado e `SUSPENSO = 'S'` **não** significando suspensão; a regra
restringe-se à categoria Convênio).

**Significado vigente. Não é reutilizado nem alterado por esta demanda.**

### 3.2 Convênio nominal (conceito **novo** — identificação/dimensão desta análise)

Identificação do convênio pelo **nome/descrição** (`CADCONVENIO.DESCR`, integral),
resolvida pela **chave composta** `(CODCONVENIO, GRUPOEMP, FILIAL)`. É uma **dimensão de
identificação**, e **não** uma categoria de cobertura. A distinção já está oficializada no
projeto: cobertura ≠ distribuição por convênio individual (`PRD-001:208` vs. `PRD-001:448`;
`PRD-002:42-43,56,160`). Em código, `DimensaoVisao.Convenio` e `CoverageCategory.Convenio`
**já significam cobertura** e **não** podem ser reutilizados para esta dimensão
(registrado em `PLAN-002`, T-01 §B).

### 3.3 Convênio presente

Convênio com **participação no conjunto válido** do indicador no período selecionado. Não há
filtro adicional por categoria de cobertura: a análise nominal **não depende** de a categoria
"Convênio" estar selecionada e **não introduz `MODOFAT = 'C'` como nova regra de produto**.

**Decisão humana (2026-10-05):** listar os convênios efetivamente presentes nos registros
válidos de cada indicador no período, preservando RN-07, P19 e as demais regras aprovadas.

### 3.4 Regras herdadas preservadas

| Regra | Origem | Tratamento nesta demanda |
|---|---|---|
| RN-07 — quantidade total no período (regra de contagem) | `PRD-001:162` | **Preservada** — o conjunto válido da análise é o do indicador |
| RN-08 — análise por tipo e categoria de cobertura | `PRD-001:163` | **Preservada** — sem alteração |
| RN-09 — período de referência = data do atendimento | `PRD-001` / `dicionario-de-dados.md:365` | **Preservada** |
| RN-26 — análise por categoria de cobertura | `PRD-001:208-212` | **Preservada** — ver §3.1 |
| P19 — convênio suspenso não é efetivamente cadastrado | `PRD-001:220-224`; `contrato-dados-dashboard.md:225-226` | **Preservada** — exclusão de suspensos na análise nominal |
| RN-23 / RN-25 — distribuição por convênio / apresentação em tabela | `PRD-001:448,450` (arquivadas) | **Reativadas** como regras-base (§1) |

## 4. Regras de negócio — RN-003-01..RN-003-10

- **RN-003-01:** Os indicadores assistenciais **Atendimentos, Consultas e Exames** devem
  permitir **análise nominal por convênio**, apresentando, para cada convênio presente, sua
  **identificação nominal** e o **volume correspondente** no período selecionado.
- **RN-003-02:** A análise nominal considera **apenas convênios presentes no conjunto válido**
  do indicador no período selecionado — isto é, os que têm participação nos registros válidos —
  **sem** filtro adicional por categoria de cobertura (`MODOFAT`).
- **RN-003-03:** A análise nominal é representada por um contrato em Core que carrega
  **identificação nominal + volume por convênio**, conceitualmente **distinto** da categoria de
  cobertura "Convênio" (RN-26), cujo significado vigente **não** é reutilizado nem alterado.
- **RN-003-04:** A introdução da análise nominal é **retrocompatível**: os contratos e filtros
  existentes continuam produzindo os mesmos resultados, sem alteração de assinatura que
  impeça implementações vigentes — exigência de retrocompatibilidade/não regressão funcional dos
  contratos e filtros existentes (ratificada em gate humano, 2026-10-05).
- **RN-003-05:** A identificação nominal é resolvida em `CADCONVENIO` pela chave composta
  **`(CODCONVENIO, GRUPOEMP, FILIAL)`**, usando `DESCR` integral; a consulta é **estritamente
  somente leitura** e agrupa por identificação nominal, nos três indicadores.
- **RN-003-06:** O conjunto válido da análise nominal é o **mesmo** do indicador: as regras
  vigentes, em particular **RN-07** (regra de contagem), são preservadas; a análise **não**
  altera as demais segmentações existentes.
- **RN-003-07:** A relação de convênios é ordenada por **volume decrescente**; em caso de
  empate, por **identificação nominal em ordem ascendente (A–Z)**.
- **RN-003-08:** A análise nominal é propagada a partir da página do indicador sem alterar
  autenticação, identidade ou autorização, e **sem introduzir regressão funcional** — regras,
  filtros, totais e agregações consolidados preservados.
- **RN-003-09:** A análise nominal é apresentada em **tabela**, exibindo **todos** os convênios
  presentes, com **rolagem vertical** quando necessária, sem Top N, sem agrupamento "Outros" e
  sem paginação.
- **RN-003-10:** A análise nominal é exposta de forma **contextual**, por **seção expansível
  vinculada ao indicador**, sem drill-down/clique sobre os convênios e sem nova rota ou página
  dedicada; os estados vazio e de erro existentes são preservados.

## 5. Rastreabilidade, premissas e dependências

### 5.1 Matriz RN ↔ CA ↔ DEC

| RN | Descrição curta | CA | DEC-003 aplicáveis | Camada |
|---|---|---|---|---|
| RN-003-01 | Análise nominal nos 3 indicadores | CA-003-01, CA-003-06 | 03 | Negócio |
| RN-003-02 | Só convênios presentes no conjunto válido | CA-003-01, CA-003-05 | — | Negócio |
| RN-003-03 | Contrato em Core: identificação nominal + volume, distinta da cobertura | CA-003-02 | — | Core |
| RN-003-04 | Retrocompatibilidade de contratos/filtros | CA-003-02 | — | Core |
| RN-003-05 | Leitura somente leitura, chave composta, `DESCR` integral | CA-003-01, CA-003-03 | — | Data |
| RN-003-06 | Mesmo conjunto válido do indicador; RN-07 preservada | CA-003-03, CA-003-05 | — | Data/Negócio |
| RN-003-07 | Ordenação volume desc. + desempate A–Z | CA-003-01, CA-003-04 | **01, 06** | Core |
| RN-003-08 | Propagação sem alterar autenticação; sem regressão funcional | CA-003-05, CA-003-07 | — | Web |
| RN-003-09 | Tabela, todos, rolagem vertical | CA-003-06 | **02, 03, 04** | UI |
| RN-003-10 | Seção expansível, sem drill-down/rota; estados vazio/erro | CA-003-07 | **05** | UI |

Cobertura: 10/10 RN com CA; 7/7 CA com RN; 6/6 DEC com RN associado (DEC-003-01 e
DEC-003-06 → RN-003-07; DEC-003-02/03/04 → RN-003-09; DEC-003-05 → RN-003-10).
Nenhum item órfão.

### 5.2 Premissas técnicas abertas (não são RN — permanecem para validação na etapa correspondente)

- **L2 — Ambiente / premissa P17:** "`CADCONVENIO` mantém a mesma estrutura no SisacHTML5"
  permanece **aberta** (`docs/architecture/contrato-dados-dashboard.md:223`). A resolução de
  `DESCR` pela chave composta é validada **somente com a credencial de leitura**, na etapa
  Data/SQL. Não confirmar por inferência.
- **L3 — Cardinalidade:** não há medição registrada da cardinalidade de `CADCONVENIO` sob
  `(CODCONVENIO, GRUPOEMP, FILIAL)`. Se a chave não for única no ambiente alvo, o volume por
  convênio pode ser multiplicado. Verificação read-only na etapa Data/SQL; sem inventar
  medição.
- **R5 — Chave final de desempate:** `DESCR` admite duplicatas, de modo que a ordenação de
  RN-003-07 pode não ser total. **A chave final de desempate NÃO é definida aqui** — a
  decisão/verificação permanece atribuída à **T-04**, sem contradizer DEC-003-01/DEC-003-06.
- **P17** (mesma de L2) segue registrada em `PLAN-001` como pendência aberta do ciclo anterior.

## 6. Decisões de apresentação Aprovadas (DEC-003-01..DEC-003-06)

> **Estado:** as seis decisões abaixo foram **APROVADAS em gate humano em 2026-10-02**, com
> base na auditoria UX Impeccable. Este cabeçalho substitui o estado anterior
> "PENDENTE/VALIDAR" herdado da materialização parcial — **correção de estado factual, sem
> alteração de conteúdo e sem reabertura de decisão UX**. Efeitos na interface em
> `docs/prototype/SPEC-UI-003-analise-por-convenio.md` §6/§7.

- **DEC-003-01 — Ordenação dos convênios (APROVADA)**  
  Ordenação padrão por **volume decrescente**.  
  **Decisão humana (2026-10-02):** aprovada com base na auditoria UX Impeccable.

- **DEC-003-02 — Limite de exibição (APROVADA)**  
  Exibir **todos os convênios presentes** no período selecionado. **Não** utilizar Top N nem agrupamento "Outros" nesta entrega.  
  **Decisão humana (2026-10-02):** a quantidade de itens não deve variar em função do viewport.

- **DEC-003-03 — Apresentação/forma de exibição (APROVADA)**  
  Apresentação em **tabela**, com, no mínimo: **identificação nominal do convênio (nome/descrição)** e **volume correspondente**.  
  **Decisão humana (2026-10-02):** não adicionar barras horizontais dentro da tabela nesta entrega.

- **DEC-003-04 — Paginação/rolagem (APROVADA)**  
  Utilizar **rolagem vertical responsiva** quando necessária. **Não** utilizar paginação nesta entrega.  
  **Decisão humana (2026-10-02):** aprovada.

- **DEC-003-05 — Drill-down/ação de clique (APROVADA)**  
  Expor a análise por convênio de forma **contextual** através de **seção expansível** vinculada ao indicador.  
  **Não** implementar drill-down/clique nos convênios nesta entrega. **Não** criar nova rota/página dedicada para esta análise.  
  **Decisão humana (2026-10-02):** aprovada.

- **DEC-003-06 — Critério de desempate/estabilidade de ordenação (APROVADA)**  
  Quando houver empate de volume, ordenar por **identificação nominal do convênio** em **ordem ascendente (A–Z)**.  
  **Decisão humana (2026-10-02):** aprovada.

## 7. Critérios de Aceite (CA) — Gherkin

Os critérios abaixo seguem Gherkin em **PT-BR**, com **IDs estáveis** [CA-003-XX]. Cobrem os três indicadores, premissas consolidadas (P19, RN-07) e não regressão.

**Família oficial desta demanda: CA-003-01..CA-003-07.** CA-003-08 e CA-003-09 **nunca existiram** e **não são criados** — a cobertura declarada acima é satisfeita por CA-003-01 (Atendimentos), CA-003-03 (Consultas e Exames) e CA-003-07 (não regressão).

### 7.1 CA-003-01 — Análise por Convênio apresenta convênios com identificação nominal (Atendimentos)

> RN-003-01, RN-003-02, RN-003-05, RN-003-07 · DEC-003-01, DEC-003-03 · CA relacionadas: CA-003-02, CA-003-03

```gherkin
Funcionalidade: Análise por Convênio nos Indicadores Assistenciais

Cenário: [CA-003-01] Listar convênios com identificação nominal e volumes no indicador Atendimentos
  Dado que estou visualizando o indicador "Atendimentos"
  E que um período está selecionado
  Quando acesso a visão "Análise por Convênio"
  Então devem ser listados os convênios presentes no indicador "Atendimentos" para o período selecionado
  E cada convênio deve ser identificado por sua **identificação nominal do convênio** (nome/descrição do convênio), resolvida via CADCONVENIO com chave composta (CODCONVENIO, GRUPOEMP, FILIAL)
  E para cada convênio deve ser exibido seu **volume** no indicador "Atendimentos"
  E apenas convênios presentes (com participação no conjunto válido do indicador) são listados
  E convênios suspensos **não** são listados (conforme P19)
```

### 7.2 CA-003-02 — A análise nominal não altera a semântica de cobertura existente

> RN-003-03, RN-003-04 · DEC: nenhuma aplicável · T-02

```gherkin
Cenário: [CA-003-02] A análise nominal não altera a semântica de cobertura existente
  Dado que um indicador assistencial possui as categorias de cobertura vigentes
  E que um período está selecionado
  Quando a análise nominal por convênio é disponibilizada
  Então os contratos, séries e totais das categorias de cobertura permanecem funcionalmente inalterados
  E a regra P19 continua preservada onde já se aplica à cobertura
  E a identificação nominal do convênio permanece conceitualmente distinta da categoria de cobertura "Convênio"
```

### 7.3 CA-003-03 — Análise nominal nos indicadores Consultas e Exames com o mesmo conjunto válido

> RN-003-05, RN-003-06 · DEC: nenhuma aplicável · T-03

```gherkin
Cenário: [CA-003-03] Análise nominal nos indicadores Consultas e Exames com o mesmo conjunto válido
  Dado que estou visualizando o indicador "Consultas"
  E que um período está selecionado
  Quando acesso a visão "Análise por Convênio"
  Então são listados os convênios presentes no indicador "Consultas" para o período selecionado
  E cada convênio é identificado por sua identificação nominal e exibido com seu volume
  E o volume respeita a regra de contagem vigente do indicador (RN-07), sem alteração das demais segmentações
  Dado que estou visualizando o indicador "Exames"
  E que um período está selecionado
  Quando acesso a visão "Análise por Convênio"
  Então são listados os convênios presentes no indicador "Exames" para o período selecionado
  E cada convênio é identificado por sua identificação nominal e exibido com seu volume
  E convênios suspensos não são listados (conforme P19)
```

### 7.4 CA-003-04 — Ordenação da análise nominal por volume decrescente com desempate A–Z

> RN-003-07 · DEC-003-01, DEC-003-06 · T-04

```gherkin
Cenário: [CA-003-04] Ordenação da análise nominal por volume decrescente com desempate A–Z
  Dado que a visão "Análise por Convênio" apresenta mais de um convênio
  Quando a lista é exibida
  Então os convênios são ordenados por volume em ordem decrescente (DEC-003-01)
  E, havendo volumes iguais, a ordem entre eles é pela identificação nominal em ordem ascendente, de A a Z (DEC-003-06)
  E a ordenação é estável entre recargas do mesmo período
```

### 7.5 CA-003-05 — A análise nominal respeita período e filtros vigentes nos três indicadores

> RN-003-02, RN-003-06, RN-003-08 · DEC: nenhuma aplicável · T-05

```gherkin
Cenário: [CA-003-05] A análise nominal respeita período e filtros vigentes nos três indicadores
  Dado que um período está selecionado no filtro da página
  E que demais filtros vigentes estão aplicados
  Quando a visão "Análise por Convênio" é carregada
  Então a análise é apresentada para o período selecionado
  E o mesmo ocorre nas páginas de Consultas e Exames
  E o estado dos filtros é preservado ao voltar da visão
  E nenhuma informação de autenticação, identidade ou autorização é exigida, exibida ou alterada por esta análise
```

### 7.6 CA-003-06 — Apresentação da análise nominal em tabela com todos os convênios presentes

> RN-003-01, RN-003-09 · DEC-003-02, DEC-003-03, DEC-003-04 · T-06

```gherkin
Cenário: [CA-003-06] Apresentação da análise nominal em tabela com todos os convênios presentes
  Dado que a visão "Análise por Convênio" está aberta
  Quando a tabela é exibida
  Então a apresentação é em tabela, com identificação nominal do convênio e volume correspondente (DEC-003-03)
  E todos os convênios presentes no período são listados, sem Top N e sem agrupamento "Outros" (DEC-003-02)
  E quando a lista excede o espaço disponível, a rolagem é vertical (DEC-003-04), sem paginação
  E a quantidade de itens listados não varia em função do tamanho da janela
```

### 7.7 CA-003-07 — Exposição contextual e não regressão dos indicadores existentes

> RN-003-08, RN-003-10 · DEC-003-05 · SPEC-UI-003 §8 · T-06

```gherkin
Cenário: [CA-003-07] Exposição contextual e não regressão dos indicadores existentes
  Dado que estou visualizando um indicador assistencial
  Quando expando a seção da análise por convênio
  Então a seção é expansível e vinculada ao próprio indicador, preservando período, filtros e totais no contexto (DEC-003-05)
  E não há drill-down ou clique sobre os convênios, nem nova rota ou página dedicada
  E nenhum indicador existente sofre regressão funcional: regras, filtros, totais e agregações consolidados permanecem preservados
  E em estado sem dados, a seção apresenta o mesmo tratamento de estado vazio já existente
  E em estado de erro, nenhum valor é inventado ou exibido
```

## 8. Histórico de revisões

| Data | Versão | Evento |
|---|---|---|
| 2026-10-02 | v0.1 (parcial) | Artefato materializado de forma incompleta (commit `cfc5ecc`): apenas §6 (DEC-003-01..06) e §7.1 (CA-003-01). Sem §1–§5, sem RN-003, sem CA-003-02..07. Decisões DEC aprovadas em gate humano nesta data |
| 2026-10-05 | v0.2 (reconstruída) | **Falha de materialização original diagnosticada** (verificação Git somente leitura: um único blob por arquivo, nenhum segundo commit/stash/ramo, `git diff` vazio). Reconstrução sob gate humano com base nas decisões e evidências persistidas: §1–§5, RN-003-01..RN-003-10, CA-003-02..CA-003-07, matriz §5.1, premissas abertas §5.2. CA-003-01 preservada semanticamente; DEC-003-01..06 preservadas literalmente; estado/cabeçalho das DEC corrigido (PENDENTE/VALIDAR → aprovadas 2026-10-02). Decisões humanas do gate: **R1** reativação de RN-23/RN-25 (RN-24 não reativada); **R2** família RN-003-01..10 ratificada; **R3** CA-003-02 = não regressão da semântica de cobertura; **R4** RN-003-04 mantida como RN desta feature; **R5 (desempate com `DESCR` duplicado) permanece aberta para T-04** |
