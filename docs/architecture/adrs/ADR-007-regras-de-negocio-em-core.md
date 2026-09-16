### ADR-007: Dashboard.Core como fonte de verdade das regras de negócio dos indicadores

- **Contexto**: O novo Dashboard Sisac Brasil acessa dados do sistema legado VCL/Delphi no SQL Server e precisa calcular indicadores (atendimentos, faturamento, produtividade médica, ocupação, glosas, convênios). A proc `SP_ATUALIZA_DASHBOARD` do legado revela como o dashboard anterior calculava esses indicadores. A arquitetura decide onde as regras de negócio residem e como o SQL é utilizado.

- **Decisão**: `Dashboard.Core` é a fonte de verdade das regras de negócio dos indicadores. O legado é fonte de conhecimento e referência para engenharia reversa — não uma especificação que deve ser copiada automaticamente. `Dashboard.Data` fornece os dados necessários ao cálculo, podendo utilizar SQL para filtros técnicos, joins e agregações necessárias à performance. SQL não deve se tornar o local da regra semântica dos indicadores. `Dashboard.Core` aplica as regras de negócio e calcula as medidas-síntese. `Dashboard.Web` permanece responsável pela apresentação e formatação. Nenhuma decisão semântica ainda pendente deve ser presumida para liberar uma query.

- **Justificativa**:
  - A `SP_ATUALIZA_DASHBOARD` revela as fórmulas que o legado usava (ex.: faturamento = `SUM(ENTRADA.Total)` com `Fechado IN ('F','E')`; ocupação = `SUM(PacienteDia)*100/SUM(LeitoDia)`). Essas fórmulas são regras de negócio — não devem ser literadas como constante no SQL, mas parametrizadas a partir de decisões em Core.
  - Filtrar por período, juntar dimensões (unidade, convênio, especialidade) e agregar linhas são operações técnicas que podem ficar no SQL para evitar transferência de milhões de linhas (FATURA: 8,2M; ENTRADA: 1,9M; GLOSA: 5M).
  - Manter as regras em Core permite que sejam validadas, testadas e ajustadas pelo time de negócio sem alterar queries.

- **Alternativas consideradas**:
  - Copiar as regras da `SP_ATUALIZA_DASHBOARD` diretamente nas novas queries SQL — descartada porque torna o dashboard secundário ao legado, dificulta testes unitários e compromete manutenibilidade
  - Processar tudo em C# (sem agregação SQL) — descartada porque transfere milhões de linhas para memória, degradando performance
  - Manter regras em ambas as camadas (SQL + C#) — descartada porque bifurca a fonte de verdade, gerando inconsistência

- **Consequências**:
  - Positivas: regras de negócio centralizadas e testáveis em Core; SQL focado em performance; mudanças de regra não afetam queries; novos indicadores adicionam lógica em Core sem duplicar agregações SQL
  - Negativas / dívidas plantadas: queries recebem parâmetros de Core (conjuntos de status, colunas de referência) — contratos entre Data e Core precisam ser documentados (T-07); decisões semânticas pendentes (Q2–Q6) bloqueiam a definição dos parâmetros

- **Referências**:
  - ADR-001 (três camadas) — esta ADR refina as responsabilidades de cada camada
  - ADR-003 (somente leitura) — permanece válida; SQL executa SELECT com parâmetros recebidos de Core
  - ADR-004 (sem persistência) — se cache for necessário (T-03), a decisão avulsa referencia esta ADR como base
  - ADR-005 (Core sem dependências) — reforçada: regras de negócio em Core sem acoplamento a SQL
  - ADR-006 (legado como referência) — reforçada: o legado é fonte para engenharia reversa; o novo dashboard define suas próprias regras