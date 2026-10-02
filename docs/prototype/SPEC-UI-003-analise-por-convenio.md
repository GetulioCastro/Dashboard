## 6. DEC-003-01..DEC-003-06 — Como afetam a Interface

| DEC | Título | Efeito na Interface (sem impor solução) | Status |
|---|---|---|---|
| **DEC-003-01** | Ordenação dos convênios | **Aprovada:** ordenação padrão por **volume decrescente**. | **APROVADA** (2026-10-02) |
| **DEC-003-02** | Limite de exibição (quantidade máxima) | **Aprovada:** exibir **todos os convênios presentes** no período. Não utilizar Top N nem agrupamento "Outros". Quantidade não varia por viewport. | **APROVADA** (2026-10-02) |
| **DEC-003-03** | Apresentação/forma de exibição | **Aprovada:** apresentação em **tabela**, com identificação nominal do convênio (nome/descrição) e volume correspondente. Sem barras horizontais. | **APROVADA** (2026-10-02) |
| **DEC-003-04** | Paginação/rolagem | **Aprovada:** **rolagem vertical responsiva** quando necessária. Sem paginação. | **APROVADA** (2026-10-02) |
| **DEC-003-05** | Drill-down/ação de clique | **Aprovada:** seção **expansível contextual** vinculada ao indicador. Sem drill-down/clique, sem nova rota/página dedicada. | **APROVADA** (2026-10-02) |
| **DEC-003-06** | Critério de desempate/estabilidade de ordenação | **Aprovada:** em caso de empate de volume, ordenar por **identificação nominal do convênio em ordem ascendente (A–Z)**. | **APROVADA** (2026-10-02) |

## 7. Decisões de UX (Aprovadas no Gate Humano)

As decisões DEC-003-01..DEC-003-06 foram **aprovadas** em gate humano (2026-10-02), com base na auditoria UX Impeccable:

1. **Ordenação (DEC-003-01):** Volume decrescente (padrão).
2. **Desempate (DEC-003-06):** Em caso de empate de volume, ordenar por **identificação nominal do convênio em ordem ascendente (A–Z)**.
3. **Limite (DEC-003-02):** Exibir **todos os convênios presentes** no período. **Não** utilizar Top N nem "Outros". Quantidade não varia por viewport.
4. **Forma de exibição (DEC-003-03):** **Tabela**, com identificação nominal do convênio (nome/descrição) e volume correspondente. **Não** adicionar barras horizontais nesta entrega.
5. **Paginação vs Rolagem (DEC-003-04):** **Rolagem vertical responsiva** quando necessária. **Não** utilizar paginação.
6. **Local/Exposição (DEC-003-05):** **Seção expansível contextual** vinculada ao indicador. **Sem drill-down/clique**. **Sem** nova rota/página dedicada.
7. **Diretrizes confirmadas:** preservar contexto (indicador, período, filtros, totais), não causar regressão funcional nas vertical slices existentes, coerência com Bootstrap 5, viewport 1024x765 como piso de qualidade (não dimensão fixa), evitar complexidade antecipada.

## 8. Novos Pontos PENDENTE/VALIDAR Identificados

- **DEC-003-01..DEC-003-06:** **APROVADAS** em gate humano (2026-10-02). Não há novos pontos pendentes identificados.  
- **Diretrizes mantidas:** ajustes de **apresentação/navegação**, quando eventualmente necessários no futuro, devem ser tratados no gate de UX, sem introduzir **regressão funcional** (regras, filtros, totais e agregações consolidados preservados), conforme RN-003-08/CA-003-07.