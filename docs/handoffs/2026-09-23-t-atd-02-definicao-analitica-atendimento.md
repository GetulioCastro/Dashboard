# Handoff — Fechamento da Definição Analítica de Atendimento (T-ATD-02) (23/09/2026)

> **Data:** 2026-09-23
> **Modo:** MODO ENGENHEIRO — FECHAMENTO DA FRENTE (análise documental, sem implementação)
> **Tema:** Indicador **ATENDIMENTOS** — definição analítica/operacional de "atendimento" frente à consulta capturada pelo SQL Server Profiler no Sisac Desktop.
> **Escopo:** confrontação documental. Nenhum código, banco, permissão ou documento de regra foi alterado. Nenhuma consulta ao banco foi executada nesta frente.

---

## 1. Estado da frente

Frente **fechada como análise documental** e entregue para validação humana. **Não foi dada continuidade automática para T-ATD-03.**

## 2. Definição formal registrada

> Um atendimento é um **evento** de atendimento associado a um **paciente**. Na consulta operacional utilizada pelo Sisac Desktop, **cada linha retornada representa um paciente e um atendimento**.

- **Não** equivale a `COUNT(DISTINCT CODPACIENTE)`.
- **ENTIDADE:** Paciente.
- **EVENTO:** Atendimento realizado para o paciente.
- **GRANULARIDADE OPERACIONAL:** uma linha elegível de `ENTRADA` = um atendimento de um paciente.
- **MEDIDA ANALÍTICA:** quantidade de eventos de atendimento elegíveis (filtros e regras a reconciliar no Core).
- **DIMENSÕES:** Data, dia da semana, horário, tipo, cobertura, sexo, médico. Não se assume comportamento idêntico das dimensões sobre múltiplos eventos do mesmo paciente.

## 3. Consulta operacional do Sisac Desktop (Profiler) — registro

```
FROM ENTRADA E
LEFT JOIN CADPACIENTE P  ON E.CodPaciente = P.CodPaciente AND P.GrupoEmp = E.GrupoEmp
LEFT JOIN CADCONVENIO C  ON E.CodConvenio = C.CodConvenio AND C.GrupoEmp = E.GrupoEmp AND C.Filial = E.Filial
LEFT JOIN CADMEDICO M    ON E.CodMedico   = M.CodMedico   AND M.GrupoEmp = E.GrupoEmp AND M.Filial = E.Filial
LEFT JOIN ESPECIALIDADE ES ON M.AMBEsp = ES.Codigo
WHERE E.DataHoraEnt >= @P1 AND E.DataHoraEnt <= @P2
  AND E.Tipo IN (@P3,...,@P9)
  AND E.GrupoEmp = @P10 AND E.Filial = @P11
  AND E.Fechado <> @P12
  AND E.LoteEnt <> @P13 AND E.LoteEnt <> @P14
ORDER BY P.Paciente, E.DataHoraEnt

P1=2026-09-01 00:00:00 · P2=2026-09-10 23:59:00 · P3..P9=Tipo 1,2,3,4,5,6,7
P10=GrupoEmp 01 · P11=Filial 01 · P12=Fechado 'C' · P13=LoteEnt 'I' · P14=LoteEnt 'INAT'
```

## 4. Resultado da confrontação (12 pontos)

1. **Definição formal:** atendimento = evento associado a paciente; 1 linha = 1 paciente + 1 atendimento.
2. **Granularidade:** 1 linha de `ENTRADA` elegível = 1 evento; `LEFT JOIN`s enriquecem e **não multiplicam** linhas.
3. **Entidade:** Paciente.
4. **Evento:** atendimento realizado para o paciente (registro `ENTRADA` elegível).
5. **Medida:** quantidade de eventos de atendimento elegíveis — **não decidida** (`COUNT(*)` × `COUNT(CODMOVIMENTO)` × `COUNT(DISTINCT ...)`).
6. **Dimensões:** data, dia da semana, horário, tipo, cobertura, sexo, médico (PRD-002).
7. **Filtros operacionais observados:** período por `DATAHORAENT` (alinhado a RN-09); TIPO 1–7; GrupoEmp/Filial 01/01; Fechado<>'C'; LoteEnt<>'I' e <>'INAT'; joins compostos por `GRUPOEMP`/`FILIAL`.
8. **Regras documentadas:** RN-07 (contagem/`FECHADO<>'C' AND COALESCE(LOTEENT,'')<>'INAT'` — T-08), RN-08 (tipo e cobertura), RN-09 (período = data do atendimento), RN-26 (cobertura Particular/Convênio/SUS), contrato-dados §3.3 (regra legada), dicionário §3 (tipo 1–6).
9. **Divergências operação × documentação:**
   - **LOTEENT (GAP/RECONCILIAÇÃO PENDENTE — sem escolha):** operação `<> 'I' AND <> 'INAT'` (e exclui NULL por semântica de comparação) × RN-07 `COALESCE(LOTEENT,'') <> 'INAT'` (inclui NULL). Corroboração: handoff 2026-09-23 (discrepância 83.014 ÷ 82.118, "provável variação na comparação de `LOTEENT`").
   - **Tipo 7:** presente na operação; mapeamento documentado cobre 1–6; **7 sem definição funcional suficiente** (gap RN-08).
   - **Cobertura:** join operacional composto `(CODCONVENIO, GRUPOEMP, FILIAL)` — diferente do JOIN simples por `CODCONVENIO` invalidado; `MODOFAT` **não** é regra automática (RN-26 exige reconciliação funcional).
   - **Período (nota):** `@P2 = 23:59:00` (truncado ao minuto) — nuance de borda, não divergência de coluna.
10. **Gaps que exigem decisão:** (a) filtro `LOTEENT` (`'I'`/`'INAT'`/NULL) — reconciliação RN-07 × operação; (b) tipo `7` (e `P/U/''/NULL`); (c) cobertura por contexto `GRUPOEMP/FILIAL` + SUS condicional (P16), sem implementar Particular/Convênio/SUS nesta etapa; (d) médico executante × `MEDRESP` (P30) e critério ativo; (e) sexo "não informado" e cobertura numérica do vínculo; (f) função de contagem/filtros finais (Core) + revalidação de janela de performance (T-ATD-01 H bloqueada por contenção).
11. **O que está fechado:** conceito evento×paciente; granularidade (1 linha = 1 atendimento de 1 paciente); data de referência `DATAHORAENT` (RN-09); tipos 1–6; join operacional de cobertura identificado; origem de sexo `CADPACIENTE.SEXO` confirmada; médico com `NOME` em `CADMEDICO` e join composto.
12. **O que não está fechado:** `LOTEENT` operação × RN-07; tipo 7; regra funcional de cobertura; papel médico/ativo; cobertura quantitativa de sexo; medida definitiva; homologação no SisacHTML5 (T-03 P1–P4).

## 5. Conclusão registrada

> **Atendimento não é simplesmente um paciente distinto.** Atendimento é um **evento associado a um paciente**. A consulta operacional do Sisac Desktop representa cada atendimento como **uma linha**.

> A **medida do Dashboard deve contar eventos de atendimento elegíveis**, conforme os filtros e regras de negócio que forem formalmente reconciliados.

## 6. Vedações registradas (desta frente)

- NÃO implementar código/SQL definitivo; NÃO criar Repository/interface/migration.
- NÃO alterar banco, permissões ou executar consultas pesadas em `ENTRADA`.
- NÃO alterar PRD-001, PRD-002, SPEC-UI-001, PLAN-001, ADRs ou contratos.
- NÃO escolher entre o filtro operacional (`LOTEENT 'I'/'INAT'`) e a RN-07 — classificado como **GAP/RECONCILIAÇÃO PENDENTE**.
- NÃO concluir que `MODOFAT` é a regra de cobertura.
- NÃO inferir significado novo para o tipo `7`.
- NÃO reabrir investigações já encerradas.

## 7. Pendências absorvidas / repasse

- Gaps em aberto (item 10) permanecem para **decisão humana** na etapa de definição em Core (fase futura), incluindo:
  - Reconciliar `LOTEENT` (operação × RN-07).
  - Revalidar janela de performance/medições de `ENTRADA` (T-ATD-01 H — "não comprovada nesta janela por limitação de performance").
  - Homologação no SisacHTML5 (T-03 P1–P4).

## 8. Próxima ação

Definida por **decisão humana** — validação do fechamento desta frente; **não há automação pendente e não há continuidade automática para T-ATD-03**.