## 6. Decisões Pendentes de Validação Humana (PENDENTE/VALIDAR)

Conforme regra 3 e 4 da autorização: **não transformar prematuramente em solução visual específica**. As decisões abaixo são **legítimas, ainda não evidenciadas** neste PRD e devem ser **validadas por humano** **antes** de planejamento/implementação.

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

> **Orientação:** Nenhuma das DEC-003-XX acima é tratada como impeditiva para gerar PRD. Elas são registradas como **PENDENTE/VALIDAR** (gate humano). O PRD especifica **o resultado de negócio** (identificar nominalmente + comparar volumes + respeitar regras), deixando a **decisão de apresentação** em aberto, sem inventá-la.

## 7. Critérios de Aceite (CA) — Gherkin

Os critérios abaixo seguem Gherkin em **PT-BR**, com **IDs estáveis** [CA-003-XX]. Cobrem os três indicadores, premissas consolidadas (P19, RN-07) e não regressão.

### 7.1 CA-003-01 — Análise por Convênio apresenta convênios com identificação nominal (Atendimentos)

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