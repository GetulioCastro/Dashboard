### ADR-008: Banco do novo SisacHTML5 como única fonte operacional do Dashboard

> **Nota de revisão (2026-09-17 — fechamento documental T-02):** o **banco SQL padrão que será instalado para o SisacHTML5 é essencialmente o mesmo banco utilizado pelo SghProg** — com poucas mudanças e exclusões de colunas; a estrutura geral, tabelas e organização seguem o padrão do SghProg. Esta premissa **não altera a decisão abaixo** (o banco operacional do SisacHTML5 permanece a única fonte operacional do Dashboard), mas deve ser registrada como orientação para o T-03:
> - o **SghProg continua sendo uma fonte válida de conhecimento estrutural** para a engenharia reversa;
> - **não se assume que exista um modelo de dados completamente novo** e **não se descartam automaticamente** tabelas, campos ou estruturas conhecidas do SghProg;
> - cabe ao **T-03 confirmar** quais estruturas realmente permanecem no banco padrão do SisacHTML5;
> - diferenças, exclusões ou alterações entre SghProg e SisacHTML5 devem ser tratadas como **evidência de evolução do modelo**;
> - **regras de negócio não são copiadas automaticamente do SghProg** para o Dashboard — a estrutura pode orientar a investigação, mas as regras são determinadas pelas decisões atuais do produto (ADR-007).

- **Contexto**: O Dashboard Sisac Brasil fará parte do novo sistema **SisacHTML5**, que substituirá o sistema legado VCL/Delphi. Quando o novo sistema estiver em operação, o banco legado **não estará em funcionamento**. As decisões anteriores (ADR-003 e ADR-006) previam que o Dashboard consultaria o banco legado em modo somente leitura durante o runtime. A gate de negócio (2026-09-16) redefine a fonte de dados do Dashboard.

- **Decisão**:
  - O banco do novo **SisacHTML5** é a **única fonte operacional de dados** do Dashboard.
  - O sistema legado VCL/Delphi e seu banco **não serão consultados pelo Dashboard em runtime**, não serão dependência de runtime, não serão fonte de produção e não serão utilizados em queries da nova aplicação.
  - O legado será utilizado exclusivamente durante a construção, como **fonte de conhecimento, engenharia reversa e referência histórica** — jamais como contrato de dados ou schema a copiar.
  - O histórico de **2025 e anos anteriores** existente no banco novo será utilizado para **homologação e demonstração**.
  - As **regras semânticas de negócio** dos indicadores pertencem ao **Dashboard.Core** (ADR-007). O **Dashboard.Data** acessa o banco novo com filtros técnicos, joins, agregações e otimizações SQL. O **Dashboard.Web** é responsável pela apresentação.
  - O Dashboard é **somente leitura** (ADR-003 revisada — agora aplicada ao banco novo).
  - Não há Application Layer, CQRS, Mediator ou camadas adicionais (ADR-001).

- **Justificativa**:
  - O novo SisacHTML5 é um sistema independente; no momento em que estiver em operação, o banco legado não existirá mais em runtime.
  - Elimina toda dependência de schema legado e qualquer acoplamento a estruturas que serão descontinuadas.
  - Mantém o Dashboard seguro (leitura) sobre os dados operacionais do próprio SisacHTML5.
  - O conhecimento extraído do legado (dicionário de dados e regras documentadas) continua válido apenas para compreender conceitos e apoiar decisões de modelagem do novo produto.

- **Alternativas consideradas**:
  - Continuar consultando o banco legado (SQL somente leitura) — descartada: o banco legado será desativado; criaria dependência de runtime e contrato de schema de um sistema que estará fora de operação.
  - Migrar dados do legado para o banco novo e consultar espelhos — adiada: a migração de dados de produção não faz parte do escopo atual; o histórico 2025+ já estará disponível no banco novo para homologação.
  - Exportar indicadores via arquivos — descartada: frágil e sem consulta em tempo real.

- **Consequências**:
  - Positivas: independência total do legado; contrato de dados do Dashboard definido pelo modelo do novo SisacHTML5; nenhum risco de consultar schema descontinuado; regras permanecem em Core (testável e validável).
  - Negativas / dívidas plantadas: as queries do Dashboard dependem do **schema do novo banco**, que ainda está em definição (tratado em T-03 — contrato de dados); a paridade dos históricos 2015+ com relatórios do legado **não é garantida** (homologação validará o quanto for compatível).

- **Referências**:
  - ADR-001 (três camadas) — mantida.
  - ADR-003 (somente leitura) — **revisada**: o alvo passa a ser o banco do novo SisacHTML5.
  - ADR-004 (sem persistir indicadores) — **revisada**: contexto passa a ser o banco novo; consultas históricas podem demandar tratamento de lentidão (requisito no PRD).
  - ADR-005 (Core sem dependências) — mantida.
  - ADR-006 (legado como referência) — **supersedida parcialmente**: o acesso ao legado é eliminado; permanece valendo a ideia de "legado = referência de conhecimento".
  - ADR-007 (regras de negócio em Core) — mantida.