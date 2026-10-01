# Handoff formal — 2026-09-25 — Fechamento ATD-VIS-04 e ATD-VIS-05

> **Data:** 2026-09-25
> **Modo:** MODO ENGENHEIRO — FECHAMENTO DOCUMENTAL
> **Escopo:** registrar o estado final de ATD-VIS-04 e ATD-VIS-05 após a normalização da massa demonstrativa e a validação visual humana.
> **Documento relacionado:** `docs/handoffs/2026-09-24-fechamento-atd-vis-04-atd-vis-05.md` permanece como registro histórico da sessão anterior; este documento complementa o fechamento ao registrar a gate visual concluída.

---

## EVIDÊNCIA

### Validação visual humana

A validação visual foi concluída em 2026-09-25 na página `/Atendimentos`.

- Período exibido: **28/06/2026 a 25/09/2026**.
- Total exibido: **5.490**.
- Dimensão **Convênio**: `1.021`, `4.030`, `439` para as séries exibidas.
- Dimensão **Sexo**: `2.463`, `3.027` para as séries exibidas.
- Gráficos, legendas, dimensões e formas foram verificados visualmente.
- A divergência entre as massas categóricas e o total foi eliminada.
- **Gate visual humana: FECHADA.**

Os valores `5.463`, `1.029/4.025/436` e `2.462/3.028` registrados ou considerados durante a etapa anterior são fixtures intermediárias e não representam o estado final validado.

### Evidência técnica previamente executada

- `dotnet build Dashboard.slnx`: **0 avisos / 0 erros**.
- `dotnet test Dashboard.Core.Tests --no-build`: **9/9 aprovados**.
- Harness C# em memória `DailyMassHarness`: **PASS**.
- Cobertura do harness: **90/90** datas em cada dimensão.
- Casos de `totalDia = 0` e `somaProvisoria = 0`: **PASS**.
- Não houve execução de SQL, conexão com CASAMATER ou alteração de banco.
- Não foram executados testes de integração de `Dashboard.Data` nesta etapa.

---

## CAUSA

As séries categóricas eram geradas de forma independente por `ValorCategoria`, com perturbação proporcional e arredondamento. A soma das categorias, portanto, podia divergir do total diário exibido pelo contrato. A causa era da geração demonstrativa e seu arredondamento, não uma inconsistência do banco, do Sisac ou dos dados operacionais.

---

## DECISÃO

Foi adotado o **Cenário A**, aprovado na validação humana:

1. Calcular valores provisórios independentes por categoria e data.
2. Calcular as quotas proporcionais ao total diário.
3. Atribuir a cada categoria a parte inteira da quota, limitada pelo valor provisório disponível.
4. Distribuir as unidades restantes pelo maior resto.
5. Desempatar de forma determinística pela ordem das categorias.
6. Tratar explicitamente `totalDia = 0` e `somaProvisoria = 0`.

A decisão normaliza somente a massa **demonstrativa** de `IndicadoresVisaoDemonstracao`; não representa regra de negócio do Sisac nem autoriza alteração da fonte operacional.

---

## EXECUÇÃO

A implementação está em:

- `Dashboard.Web/Dados/IndicadoresVisaoDemonstracao.cs`

A execução ocorre por data, antes da consolidação do período:

- `SeriesPorCategoria()` gera os provisórios por categoria e data;
- `ReconciliarCategorias()` aplica a normalização por data;
- `ObterQuotas()` calcula e distribui as quotas;
- `ValorCategoria()` e a série temporal original foram preservados;
- a reconciliação ocorre somente na geração demonstrativa;
- não foi criada reconciliação agregada pós-consolidação;
- não houve alteração no JavaScript, no contrato serializado, no SQL, no Core ou no fluxo original da Index.

A invariável da implementação é: **cada data mantém sua massa total e suas categorias mantêm a soma dessa massa após a normalização**.

---

## VALIDAÇÃO

O estado final foi validado nos seguintes níveis:

- **Contrato ATD-VIS-04:** tipos, propriedades, enums, serialização e testes do contrato mantidos.
- **Geração ATD-VIS-05:** valores por data e por categoria conferidos.
- **Massa final em 25/09/2026:**
  - Convênio: `1.021 + 4.030 + 439 = 5.490`;
  - Sexo: `2.463 + 3.027 = 5.490`.
- **Série temporal:** o total agregado do período permaneceu consistente com a soma dos totais diários.
- **Casos-limite:** datas sem total e séries provisórias sem soma foram tratados sem falha.
- **Build e testes unitários:** results registrados acima, sem erro.
- **Gate visual:** gráficos, legendas, dimensões, formas e total conferidos em navegador.

A validação não consultou o SisacHTML5 e não substitui a validação de uma futura fonte SQL.

---

## FECHAMENTO

- **ATD-VIS-04:** `VALIDADA`.
- **ATD-VIS-05:** `VALIDADA`.
- **Gate visual:** `FECHADA`.
- **Escopo funcional fechado:** visão demonstrativa de Atendimentos, com contrato, geração, normalização e validação visual documentados.
- **Fonte dos dados:** permanece explicitamente demonstrativa; não foi criada integração SQL.
- **Estado Git:** nenhum commit, reset, revert ou stash foi executado. Alterações e artefatos preexistentes da árvore de trabalho foram preservados.

Este handoff substitui, para fins de estado final, a indicação de gate visual pendente do handoff de 24/09; não apaga nem reescreve o histórico daquele documento.

---

## PENDÊNCIAS

- Não há pendência funcional ou documental dentro de ATD-VIS-04 e ATD-VIS-05.
- T-08, R-05, CASAMATER e qualquer integração SQL permanecem fora do escopo e não foram reabertos.
- `SPEC-VIS-001`, `PRD-002`, `PLAN-001` e ADR-010 não foram alterados: ATD-VIS-04/05 não são tarefas do PLAN-001 e este handoff é o registro de execução e validação correspondente.
- A SPEC-VIS-001 permanece como documento-base de contrato; este fechamento não altera seu escopo nem transforma a fonte demo em contrato SQL.
- Nenhuma próxima funcionalidade, integração de dados ou alteração de arquitetura foi iniciada.
