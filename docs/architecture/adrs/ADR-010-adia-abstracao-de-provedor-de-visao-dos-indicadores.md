### ADR-010: Adiar a abstração de provedor de visão dos indicadores

> **Status da decisão:** **VALIDADA** pelo responsável pelo projeto (MODO ENGENHEIRO, 2026-09-23). Esta ADR registra uma decisão arquitetural — **não é implementação** e não altera código.
> **Origem:** análise arquitetural de menor mudança (2026-09-23) sobre o fluxo de dados demo dos indicadores (`IndicadoresDemonstracao → IndexModel → Razor/JS`), antes da implementação da fonte SQL. A decisão adota o **Cenário A** — refatorar e criar abstrações somente quando o provider SQL existir.

- **Contexto**: O Dashboard hoje possui **uma única fonte de dados** de indicadores: a classe demo `IndicadoresDemonstracao` (`Dashboard.Web/Dados`) que alimenta a página índice (`IndexModel`) e é serializada para o frontend (`dashboard.js`). O contrato de visão consumido pelo frontend é um JSON estável (`id, nome, unidade, monetario, forma, valorReferencia, referencia, pontos`), já validado visualmente. Existem ainda, fora do fluxo Web: o DTO `IndicadorVisao` em `Dashboard.Core` (não utilizado), a camada de evidência escalar `IIndicatorRepository → IndicatorData` (congelada nesta etapa) e o `AtendimentosRepository` em `Dashboard.Data`. Considerou-se criar uma interface `IIndicadoresVisaoProvider` para "preparar" a futura troca Demo → SQL.

- **Decisão**:
  - **NÃO criar `IIndicadoresVisaoProvider` (nem qualquer abstração de provedor de visão) nesta etapa.**
  - O cenário adotado permanece: **`IndicadoresDemonstracao → IndexModel → Razor/JS`**.
  - **`IndicadorDemonstracao` NÃO está sendo promovido a contrato arquitetural definitivo** — permanece como DTO demonstrativo interno do `Dashboard.Web`.
  - `IndicadorVisao` não é alterado, movido nem promovido nesta etapa (mantido como direção futura).

- **Justificativa**:
  - **Menor mudança:** o acoplamento atual é de um **único call-site** (`Index.cshtml.cs`) a uma classe demo determinística e **sem estado** — não há acoplamento danoso que a abstração remova hoje.
  - **Ausência de benefício concreto imediato:** não existem testes do `IndexModel` nem segundo consumidor; DI/testabilidade teriam valor apenas especulativo.
  - **A interface não reduz o custo da futura integração SQL:** o bloqueio real quando a fonte SQL chegar será a **localização do DTO de visão** (hoje no `Web`; `Dashboard.Data` não pode referenciá-lo) — custo **independente** da existência ou não da interface.
  - **Contrato definitivo ainda desconhecido:** abstrair agora sobre `IndicadorDemonstracao` (DTO de nome "demo") cristalizaria um contrato provisório e **aumentaria o risco de retrabalho** quando o contrato real for definido.
  - **Aderência ao princípio da menor mudança:** nada a criar hoje; a integração SQL futura é pequena e localizada.

- **Alternativas consideradas**:
  - Criar `IIndicadoresVisaoProvider` agora e registrar o provider demo via DI (Cenário B) — **descartada nesta etapa**: não desbloqueia o futuro (localização do DTO), não traz benefício concreto imediato, introduz abstração sem demanda atual e arrisca retrabalho sobre contrato provisório.
  - Adaptar/promover `IndicadorVisao` como contrato de visão agora — **descartada**: exigiria alterações de tipo em Razor/JS validados e anteciparia um contrato cuja forma final depende do provider SQL.

- **Consequências**:
  - Positivas: nenhum arquivo de código alterado; menor mudança mantida; frontend validado intocado; regra para a futura integração registrada.
  - Negativas / dívidas plantadas: quando o provider SQL for autorizado haverá refatoração pequena e localizada — **primeiro** definir o contrato definitivo na camada arquitetural adequada (provavelmente `Dashboard.Core`, com base em `IndicadorVisao` evoluído) **e somente depois** criar as abstrações/providers necessários; pode envolver a migração do DTO de visão para `Core` e um ajuste de tipo no Razor.

- **Condições que poderão justificar revisão futura desta decisão**:
  - Existência de **segundo provider simultâneo** de visão;
  - Necessidade de **alternância Demo/SQL por configuração** (runtime/config);
  - Necessidade **concreta de testes unitários** do `IndexModel`;
  - **Entrada efetiva do provider SQL** autorizada.

- **Regra futura:**
  - Quando o provider SQL for autorizado: **primeiro** definir o contrato definitivo na camada arquitetural adequada e **somente depois** criar abstrações/providers necessários. Não criar implementação futura antecipadamente.

- **O que fica explicitamente fora desta decisão**:
  - A definição do contrato definitivo de visão — etapa própria, quando o provider SQL for autorizado.
  - Qualquer criação de interface/classe ou alteração de `.cs`, `.cshtml`, JavaScript, CSS — proibidas nesta etapa.
  - Alterações em `IIndicatorRepository`, `IndicatorData`, `IndicatorFilter`, `PeriodoResolutor` e `AtendimentosRepository` — congelados, fora de escopo.
  - Investigação SQL / banco / frentes encerradas (Convênio, Sexo, `user_token`/`cod_fi`, SisacGR, prefixo "old") — não reabrir.

- **Referências**:
  - ADR-001 (três camadas) — mantida; nenhuma camada nova introduzida.
  - ADR-005 (Core sem dependências) — mantida; o contrato futuro deve residir na camada adequada.
  - ADR-007 (regras de negócio em Core) — mantida; regras não migram para o SQL.
  - `docs/handoffs/2026-09-22-fechamento-vertical-slice-dashboard.md` — registra a slice vertical demo validada (fonte `IndicadoresDemonstracao`).
  - `Dashboard.Web/Pages/Index.cshtml.cs` — único call-site de `IndicadoresDemonstracao.Obter()`.
  - `Dashboard.Core/DTOs/IndicadorVisao.cs` — DTO de visão não utilizado, reservado como direção futura.