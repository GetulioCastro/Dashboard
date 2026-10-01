### ADR-009: Identidade, credenciais, sessão, autorização e contexto do Dashboard vinculados ao SisacHTML5

> **Status da decisão:** **VÁLIDADA** pelo responsável pelo projeto (MODO ENGENHEIRO, 2026-09-22). Esta ADR registra decisões arquiteturais — **não é implementação** e não define mecanismo técnico.
> **Origem:** investigações encerradas de autenticação do SisacHTML5 (código Delphi) e do schema de `ACESSOW`/`ACESSOUSUW`/`USUARIO` — as decisões abaixo derivam das conclusões validadas dessas investigações, sem reabri-las.

- **Contexto**: O DashboardSB é um módulo do novo **SisacHTML5** (ADR-008) e precisa decidir como tratar identidade, credenciais, sessão, autorização e contexto do usuário. A investigação de autenticação do SisacHTML5 legado foi **encerrada** em nível suficiente para a decisão arquitetural; a investigação SQL do schema de `ACESSOW`, `ACESSOUSUW` e `USUARIO` foi **encerrada** sem retorno à engenharia reversa do código Delphi. O responsável pelo projeto validou explicitamente as decisões registradas abaixo. As tabelas do ecossistema Sisac não possuem PK/FK formais declaradas envolvendo `ACESSOW`, `ACESSOUSUW` e `USUARIO`, e o vínculo lógico observado (`ACESSOUSUW.Usuario` → `AcessoW.USUARIO`) foi confirmado em **9 de 15** registros de permissão — `USUARIO` foi **descartada** como tabela de autenticação (zero correspondências com `AcessoW`). Essas evidências fundamentam o princípio de que a identidade pertence ao SisacHTML5, mas **não** definem — e esta ADR não define — como o Dashboard a consumirá tecnicamente.

- **Decisão**:
  - **1. Identidade** — O **SisacHTML5 é a fonte exclusiva de identidade** do usuário do Dashboard. O Dashboard **não terá cadastro próprio de usuários**. O usuário do Dashboard deve ser **necessariamente um usuário já existente no SisacHTML5**.
  - **2. Credenciais** — O Dashboard **não terá credenciais próprias**: não será criado usuário/senha específico para o Dashboard; não será transportada, armazenada ou reproduzida **senha MASTER do Sisac**; a autenticação permanece vinculada ao mecanismo de identidade do SisacHTML5.
  - **3. Sessão / vínculo com o Sisac** — O Dashboard **não possui sessão de autenticação independente** do SisacHTML5. Dashboard e SisacHTML5 são **intrinsecamente vinculados**; se não existir identidade/sessão válida no Sisac, o Dashboard **não considera o usuário autenticado por conta própria**.
  - **4. Autorização** — Na primeira versão, a autorização do Dashboard é **binária**: acesso **permitido** ou **negado**, sem níveis distintos de acesso aos indicadores. A regra essencial é: **"Este usuário pode acessar os indicadores do Dashboard?"**
  - **5. Contexto de empresa/filial** — Um mesmo usuário pode estar relacionado a mais de uma empresa/filial no ecossistema Sisac. Na primeira fase, o contexto operacional do Dashboard é limitado à **EMPRESA**; suporte específico a **FILIAL** fica como possibilidade futura. O transporte técnico de `GrupoEmp`/`Filial` **não é definido nesta ADR** — será investigado em etapa posterior.
  - **6. Disponibilidade de módulos** — A disponibilidade de um módulo do Dashboard é uma **configuração da implantação/deployment**, independente da autorização individual do usuário. Um módulo/indicador somente está disponível quando: **(a)** o módulo estiver habilitado naquela implantação **E** **(b)** o usuário estiver autorizado a acessar o Dashboard/indicadores. Se o módulo estiver desabilitado na implantação, seu menu/opção é ocultado ou desabilitado, independentemente das permissões do usuário.
  - **Princípios consolidados**:
    - `SisacHTML5` = **fonte de identidade**;
    - `Dashboard` = **consumidor** dessa identidade/contexto;
    - `Dashboard` **não possui autenticação independente**;
    - `Disponibilidade do módulo` = **configuração da implantação**;
    - `Autorização do usuário` = **decisão de acesso**;
    - `Indicador disponível` = **módulo habilitado** `AND` **usuário autorizado**.

- **Justificativa**:
  - A investigação encerrada (código Delphi do SisacHTML5) confirmou que a autenticação pertence ao próprio Sisac (fluxo de login do sistema, `AcessoW` como base de identidade) — duplicar identidade/credenciais no Dashboard criaria uma segunda fonte de verdade e risco de divergência.
  - A investigação SQL encerrada afastou `USUARIO` como base de autenticação e mostrou que o vínculo lógico relevante está no ecossistema do próprio Sisac (`ACESSOUSUW` → `AcessoW`), reforçando que a identidade deve ser consumida, não recriada.
  - Autorização binária na primeira versão atende ao MVP (um papel único de acesso aos indicadores) sem antecipar níveis de permissão ainda não definidos pelo produto.
  - Separar **disponibilidade de módulo** (configuração da implantação) de **autorização do usuário** (decisão de acesso) evita confundir infraestrutura com permissão e mantém cada responsabilidade no seu lugar.
  - Limitar o contexto à **EMPRESA** na primeira fase evita introduzir complexidade de multi-filial sem decisão de produto.
  - **Não transportar/reproduzir senha MASTER do Sisac** protege o mecanismo de identidade do ecossistema e mantém o Dashboard como consumidor, não detentor, de segredos.

- **Alternativas consideradas**:
  - Cadastro próprio de usuários no Dashboard — **descartada**: cria identidade paralela e bifurca a fonte de verdade de quem é o usuário (viola "Sisac = fonte de identidade").
  - Credenciais próprias do Dashboard (usuário/senha do Dashboard) — **descartada**: o Dashboard não é um sistema de autenticação e duplicaria credenciais.
  - Sessão de autenticação independente do Dashboard — **descartada**: desvincularia o estado do SisacHTML5 e permitiria "autenticação por conta própria", contrariando o vínculo intrínseco entre os sistemas.
  - Autorização granular por indicador na primeira versão — **adiada**: escopo de níveis de acesso não foi definido pelo produto; a versão 1 é binária.
  - Suporte a FILIAL na primeira fase — **adiada**: contexto limitado à EMPRESA.
  - Definir nesta ADR o mecanismo técnico de transporte/validação da identidade (JWT, cookie, token, sessão compartilhada, SSO etc.) — **fora de escopo deliberademente**: decisão técnica posterior, dependente da integração com o SisacHTML5.

- **Consequências**:
  - Positivas: identidade única e fiel ao ecossistema Sisac; nenhum segredo do Sisac trafega pelo Dashboard; autorização simples e auditável (binária); disponibilidade de módulos controlada pela implantação sem depender de permissão individual; contexto de empresa definido sem inventar modelo de transporte.
  - Negativas / dívidas plantadas: o Dashboard **depende funcionalmente do SisacHTML5 para qualquer autenticação** (sem identidade no Sisac, não há acesso); o **mecanismo técnico de integração de identidade permanece em aberto** e precisa ser definido antes da implementação; a **definição de como o Dashboard determina a autorização** (permissão binária de acesso aos indicadores) não está especificada e será objeto de etapa posterior; multi-filial fica para evolução.

- **O que fica explicitamente fora desta decisão**:
  - O **mecanismo técnico** de transporte/validação da identidade entre SisacHTML5 e Dashboard (JWT, cookie, token, sessão compartilhada, SSO ou qualquer outro) — **não definido nesta ADR**.
  - Como `GrupoEmp`/`Filial` serão transportados/referenciados tecnicamente — **a investigar em etapa posterior**.
  - Como as tabelas `AcessoW`, `ACESSOUSUW` e `USUARIO` serão utilizadas pelo Dashboard — **nada é inferido** sem evidência; as investigações foram encerradas e qualquer uso futuro exige nova etapa autorizada.
  - A reabertura da investigação de `Login.pas`, `ElsoftProc.pas`, criptografia, `SenhaSistema` ou do mecanismo de suporte do SisacHTML5 — **não retomar**.
  - Senhas, hashes, tokens ou segredos — **não registrar transportar nem armazenar**.
  - Integração da identidade com o módulo do SisacHTML5 na prática — decisão de implementação futura.

- **Referências**:
  - `docs/handoffs/2026-09-21-fechamento-autenticacao.md` — encerramento da investigação de autenticação do SisacHTML5 (código Delphi); `AcessoW` na autenticação/autorização, `ACESSOUSUW` na autorização granular; decisão de autenticação arquitetural registrada como **pendente** à época.
  - `docs/handoffs/2026-09-21-encerra-investigacao-schema-acessow-acessousuw-usuario.md` — encerramento da investigação read-only do schema; `USUARIO` descartada como base de autenticação; vínculo lógico `ACESSOUSUW` → `AcessoW` em 9/15; sem PK/FK formais; vínculo por `NOME` não avaliado. **→ Esta ADR fecha a decisão arquitetural que aquele handoff deixou pendente.**
  - `docs/handoffs/2026-09-21-encerra-investigacao-r02-r05.md` — encerramento das tentativas de consulta ao CASAMATER (T-08); preservação do estado validado e próximos passos do T-08 (contexto do branch de trabalho, não influencia esta decisão de identidade).
  - `docs/architecture/t03-acesso-sql-readonly.md` — acesso exclusivamente leitura ao banco do SisacHTML5 (CASAMATER, `dashboard_readonly`); contexto de infraestrutura que o Dashboard não deve ampliar para autenticação.
  - `docs/plans/PLAN-001-dashboard-indicadores.md` — Q9 registra **autenticação fora do escopo atual** do plano; esta ADR registra a decisão sem alterar o plano.
  - ADR-001 (três camadas) — mantida; esta ADR não altera as camadas.
  - ADR-003 (somente leitura) / ADR-008 (SisacHTML5 única fonte operacional) — alinhadas: o princípio "Dashboard = consumidor" aqui registrado é coerente com "Dashboard = módulo somente leitura do SisacHTML5".
  - ADR-007 (regras em Core) / ADR-005 (Core sem dependências) — mantidas; decisões de autorização de acesso (não de negócio dos indicadores) pertencem à fronteira de identidade do Dashboard, a detalhar em etapa técnica futura.