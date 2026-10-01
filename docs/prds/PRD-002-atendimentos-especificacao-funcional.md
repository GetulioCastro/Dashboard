# PRD-002: Especificação Funcional do Indicador Atendimentos

**Cliente/Produto:** Dashboard Sisac Brasil (módulo do novo SisacHTML5)
**Tipo:** Feature/PBI — Especificação funcional preliminar do PBI **Atendimentos**
**PRD de referência (Epic):** `docs/prds/PRD-001-dashboard-indicadores.md`
**Autor:** Agente IA
**Data:** 2026-09-23
**Status:** Rascunho — especificação funcional **preliminar** (documental)

> **Nota (2026-09-23):** este documento registra a **especificação funcional preliminar** do indicador **ATENDIMENTOS**. É **somente documentação** — não há implementação, não há execução de SQL, não há alteração de código, banco ou interface. Nenhuma decisão técnica de medida é feita aqui. A referência funcional/visual usada é um dashboard em Power BI que apresenta "Atendimentos" como medida analisável por diferentes dimensões.

---

## 1. Contexto funcional

O DashboardSB não deve ser tratado apenas como uma tela que apresenta quantidades. Seu objetivo é **transformar os dados existentes no Sisac em informações** que permitam ao usuário **responder perguntas de negócio**.

A referência funcional/visual utilizada é um dashboard em Power BI que apresenta **"Atendimentos"** como medida analisável por diferentes dimensões. Essa referência é tratada como **referência funcional e visual**, **não** como especificação técnica ou fonte automática das queries.

---

## 2. Definição funcional

| Campo | Definição |
|---|---|
| **Indicador** | Atendimentos |
| **Objetivo** | Permitir compreender o fluxo de atendimentos em determinado período e analisá-lo por diferentes dimensões operacionais e gerenciais |
| **Medida principal** | Quantidade de atendimentos |

> **IMPORTANTE — fora de escopo desta etapa:** NÃO definir a implementação técnica da medida. Não decidir `COUNT(*)`, `COUNT(CODMOVIMENTO)` ou qualquer outra expressão. Isso será determinado posteriormente por **investigação baseada em evidências do banco** e nas **regras de negócio**.

---

## 3. Dimensões inicialmente desejadas

- Data
- Dia da semana
- Horário
- Tipo
- Sexo
- Médico
- Convênio — **cobertura** (Particular / Convênio / SUS): dentro do **escopo atual** de Atendimentos (RN-26). **Não** é distribuição por convênio específico.
- Convênio individual (ex.: UNIMED, IAPEP Saúde): **capacidade funcional desejada/futura**, fora do MVP atual — enquanto RN-23 permanecer arquivada no PRD-001.

---

## 4. Perguntas de negócio que o indicador deve permitir responder

1. Quantos atendimentos ocorreram no período?
2. Como os atendimentos evoluíram ao longo dos dias?
3. Quais dias da semana concentram maior fluxo?
4. Em quais horários existe maior concentração?
5. Como os atendimentos se distribuem por tipo?
6. Como os atendimentos se distribuem por convênio — **cobertura** (Particular / Convênio / SUS)? *(dentro do escopo atual — RN-26)*

   > **Nota:** esta pergunta refere-se à **cobertura** e é distinta da distribuição por **convênio individual** (ex.: UNIMED, IAPEP Saúde), que permanece como capacidade desejada/futura e **fora do MVP atual** — enquanto RN-23 permanecer arquivada no PRD-001.
7. Como os atendimentos se distribuem por sexo?
8. Quais médicos concentram maior quantidade de atendimentos?
9. Em quais horários consultas superam exames?
10. Em quais horários exames superam consultas?

---

## 5. Exemplos funcionais

**Exemplo 1 — distribuição por sexo:**
100 atendimentos no dia. O usuário pode visualizar:
- Masculino: 43
- Feminino: 57

**Exemplo 2 — distribuição por tipo:**
Atendimentos por tipo:
- Exame: 3.170
- Consulta: 2.116

**Exemplo 3 — distribuição por horário:**
Atendimentos por horário: Consulta e Exame, permitindo **comparar o comportamento das duas categorias ao longo do dia**.

---

## 6. Visualizações desejadas

| Situação | Visualizações |
|---|---|
| Evolução temporal | linha, colunas |
| Comparação | barras, colunas |
| Distribuição | donut, barras |
| Resumo | cartões |

> A escolha definitiva da visualização deverá considerar a **pergunta de negócio** e a **quantidade/dimensionalidade dos dados**.

---

## 7. Filtro inicial

- **Período/data.**

> Outros filtros somente deverão ser definidos após confirmação de **suporte funcional e técnico**.

---

## 8. MVP funcional

**Primeira camada:**
- total de atendimentos;
- período;
- evolução por data;
- por tipo;
- por cobertura (Particular / Convênio / SUS — RN-26);
- por sexo.

**Segunda camada:**
- dia da semana;
- horário;
- comparação Consulta × Exame por horário.

**Evolução futura:**
- médico;
- distribuição por convênio individual (RN-23 arquivada — capacidade desejada, fora do MVP atual);
- comparação entre períodos;
- outras dimensões disponíveis no banco.

---

## 9. Limites desta especificação

Ainda **NÃO** estão definidos:

- tabela fonte;
- registro que representa um atendimento;
- fórmula SQL;
- tratamento de cancelamentos;
- tratamento de registros incompletos;
- tratamento de horários inválidos;
- tratamento de sexo não informado;
- regra definitiva de cobertura/convenio (Particular / Convênio / SUS — sujeita à confirmação; RN-26);
- regra definitiva de distribuição por convênio individual (capacidade futura — RN-23 arquivada);
- regra definitiva para tipos;
- regras para múltiplas empresas/filiais;
- tratamento histórico.

Esses pontos deverão ser determinados **posteriormente, mediante investigação e evidência**.

---

## 10. Princípio funcional

Separar:

| Camada | Elemento |
|---|---|
| **MEDIDA** | Atendimentos |
| **DIMENSÕES** | Data, dia da semana, horário, tipo, cobertura (Particular/Convênio/SUS — RN-26), sexo, médico; convênio individual como capacidade futura (RN-23 arquivada) |
| **VISUALIZAÇÕES** | Card, coluna, barra, linha, donut |

---

## 11. Relação com artefatos existentes

- Regras de negócio vigentes do indicador permanecem no Epic: **RN-07** (quantidade total no período), **RN-08** (análise por tipo e cobertura), **RN-09** (período de referência = data do atendimento) — ver `PRD-001`. A **cobertura** (Particular / Convênio / SUS — RN-26) é distinta da distribuição por **convênio individual** (RN-23, arquivada no PRD-001) — esta permanece como capacidade desejada/futura, fora do MVP atual.
- A **implementação técnica** da medida e o tratamento dos pontos do §9 continuarão **dependentes de investigação com evidência**, sem alterar este documento nem o PRD-001 sem autorização.
- A SPEC-UI-001 descreve a **interface visual** do dashboard (protótipo); esta especificação é **funcional**, não visual.

---

## 12. Referências

- `docs/prds/PRD-001-dashboard-indicadores.md` — PRD Epic (RN-07, RN-08, RN-09, RN-26; hierarquia `PBI: Atendimentos`; RN-23 arquivada/backlog)
- `docs/prototype/SPEC-UI-001-dashboard-indicadores.md` — interface visual (protótipo)
- `docs/architecture/contrato-dados-dashboard.md` — contrato de dados (T-03), domínio Atendimentos
- `docs/architecture/dicionario-de-dados.md` — conhecimento estrutural do legado (evidência, não contrato)