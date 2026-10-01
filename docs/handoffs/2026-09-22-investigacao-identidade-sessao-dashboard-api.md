# Handoff — Investigação técnica: como o Dashboard.Api identificará o usuário/contexto autenticado pelo SisacHTML5 (22/09/2026)

> **Data:** 2026-09-22
> **Modo:** MODO ENGENHEIRO — INVESTIGAÇÃO TÉCNICA
> **Escopo:** identificação e validação técnica, em nível de investigação, da identidade e do contexto de EMPRESA associados a um usuário autenticado pelo SisacHTML5, respeitando integralmente a ADR-009. **Nenhuma solução foi escolhida nem implementada.**
> **Arquivos de referência:** `docs/architecture/adrs/ADR-009-identidade-autenticacao-e-autorizacao-sob-sisac-html5.md`, `docs/handoffs/2026-09-21-fechamento-autenticacao.md`, `docs/handoffs/2026-09-21-encerra-investigacao-schema-acessow-acessousuw-usuario.md`, `docs/architecture/t03-acesso-sql-readonly.md`, `docs/plans/PLAN-001-dashboard-indicadores.md`
> **Fontes de evidência adicionais (somente leitura):** código-fonte do SisacHTML5 (`C:\ProjetosSB\SisacHTML5\`) — `Main.pas`, `MainModule.pas`, `ServerModule.pas`, `SGAdm.pas`, `ElsoftProc.pas`, `AplicacaoExterna.pas`, `Suprimentos\Main.pas`, `Sus\Main.pas`, `Clinicas\Main.pas`, `Client_API_REST\uAuthService.pas`
> **Nenhum SQL foi executado nesta investigação.**

---

## 1. OBJETIVO

Responder à questão central: **"Como o Dashboard.Api consegue identificar e validar, tecnicamente, que a requisição recebida pertence a um usuário autenticado pelo SisacHTML5 e qual é o contexto de EMPRESA associado a essa identidade?"** — mantendo a hipótese do responsável (consulta direta à base de dados) **apenas como hipótese a investigar**, nunca como solução pré-definida.

## 2. CONTEXTO E DECISÕES JÁ ESTABLECIDAS

A **ADR-009** já decidiu (sem definir mecanismo técnico):
- SisacHTML5 = fonte exclusiva de identidade; o Dashboard não tem cadastro próprio, credenciais próprias nem autenticação independente.
- O usuário do Dashboard deve existir no SisacHTML5; não transportar/armazenar/reproduzir senha MASTER.
- Autorização binária (permitido/negado); contexto limitado à **EMPRESA** na primeira fase; FILIAL como evolução.
- Disponibilidade de módulo = configuração da implantação, independente da autorização.

Esta investigação **não redefine** nada disso — apenas levanta evidência técnica para a próxima decisão.

## 3. EVIDÊNCIAS ENCONTRADAS

### 3.1. A identidade autenticada vive na sessão do SisacHTML5 (uniGUI)
- `SGAdm.pas` (linhas 10, 41, 57, 104-107): `TdmSGA` é um **DataModule por sessão** (`UniMainModule.GetModuleInstance(TdmSGA)`), contendo `QUsuario` (usuário autenticado), `QEmpresa` (empresa/filial), `SghIni`, `ConfigEmpresa`, `QDashBoard`, entre outros.
- `Main.pas` (linhas 94-95): no fluxo normal (sem `user_token` na URL), a identidade vem da sessão: `sUsuario := dmSGA.QUsuario.FieldByName('Nome').AsString;`.
- `MainModule.pas` (linhas 19-33): `TUniMainModule` (por sessão) expõe propriedades `usuario` (String), `codFilial` (String), e `FunbenToken`/`FunbenTokenValidade` (integração FUBEN, fora do escopo). **Não foi localizado nenhum ponto de atribuição de `UniMainModule.usuario`/`codFilial`** nesta varredura — as propriedades existem, mas o uso/atribuição não foi provado (ver §9).
- `uniGUI` mantém a sessão do navegador (servidor-side) por sessão/servidor; os módulos dependentes da sessão (`dmSGA`, `UniMainModule`) são instanciados por sessão via `GetModuleInstance`.

### 3.2. A contexto de EMPRESA/FILIAL vem de `CONFIGEMPRESA`
- `ElsoftProc.pas` → `TrocaEmpresa(sGE,sFI)` (linhas 7655-7671): abre `dmSGA.QEmpresa` com `SELECT ... FROM CONFIGEMPRESA (NoLock) WHERE GrupoEmp = :pGE and Filial = :pFI`. A sessão guarda a empresa como par **GrupoEmp + Filial** e o nome (`Empresa`).
- `Main.pas` (linhas 123-125): `sFilialLogada := GrupoEmp + Filial + ' - ' + Empresa`, lido de `dmSGA.QEmpresa`.
- `ElsoftProc.pas` → `SghLog` (linhas 7686-7690): o contexto padrão de escrita de log é `dmSGA.QEmpresa` (GrupoEmp/Filial da sessão).

### 3.3. Existe TRANSPORTE de identidade+filial entre o SisacHTML5 e o portal "novo"
- `AplicacaoExterna.pas` (linhas 19, 51-61): formulário com `TUniURLFrame` que embute uma aplicação externa; `CarregarModulo(sUrl)` apenas injeta um cache-breaker (`?t=`/`&t=`), **não** adiciona identidade por si só.
- `Suprimentos/Main.pas` (linhas 77-84): `CarregarModulo('http://html5.sisacbrasil.com:8022/Home/PaginaPrincipal?user_token=<valor>&cod_fi=01')` — o SisacHTML5 (uniGUI) **já abre o portal novo num iframe passando `user_token` e `cod_fi` (filial) na query string**. É o mecanismo existente de transporte de identidade/contexto entre o surface uniGUI e o portal `Home/PaginaPrincipal`. *(Os valores literais presentes no código-fonte não são reproduzidos neste documento.)*
- `Main.pas` (linhas 94-122) e `Sus/Main.pas` (linhas 714-750): o próprio SisacHTML5, quando aberto com `user_token` na URL, faz: `ConectaBD` → (se `cod_fi` não vazio) `TrocaEmpresa('01', cod_fi)` → `sUsuario := TNetEncoding.Base64.Decode(user_token)` → `EditaUsuario(...)` → `SelecUsuario(sUsuario)`. **Ou seja, o `user_token` é, na prática, o NOME do usuário codificado em Base64**, transportado por parâmetro de URL, sem etapa de assinatura/validação visível no código analisado.
- `Sus/Main.pas` (linhas 744-750): o bloco `user_token` está **comentado** e o módulo usa bypass fixo (`sUsuario := 'SISTEMA'`, `sGE := '01'`, `sFI := '01'`). Apenas registro factual.

### 3.4. Autenticação "de suporte" via API externa (SAC)
- `uAuthService.pas` (linhas 14-44): cliente REST para `https://sisacbrasil.com/ApiSAC/Api` → recurso `Autenticar` (`Login`, `Matricula`, `Senha`). Mecanismo de **suporte**, já registrado no handoff de fechamento de autenticação; **não reaberto**. Não é genérico para usuários comuns.

### 3.5. O dashboard legado usa a sessão (usuário + empresa) para dados e autorização de visualização
- `Main.pas` → `DashResMensal` (linhas 255-402): o dashboard interno lê `DASH_RESUMO_MENSAL WHERE GrupoEmp = :pGE and Filial = :pFI` com valores da sessão (`dmSGA.QEmpresa`), e restringe gráficos financeiros a `sUsuario = 'SISTEMA'` (linhas 340-388). Exemplo de como "dados por empresa" e "exibição por usuário" já convivem no ecossistema.

### 3.6. Disponibilidade de menu por empresa (configuração)
- `Suprimentos/Main.pas` → `ConfigMenu` (linhas 88-130): itens de menu são ocultados conforme `dmSGA.QEmpresa.FieldByName('SetorEsp')` — evidência de que parte da "disponibilidade" é derivada de configuração da empresa/implantacão (contexto próximo da decisão 6 da ADR-009, mas **não** é a autorização individual).

## 4. FLUXO TÉCNICO ATUALMENTE OBSERVADO

1. Usuário acessa o SisacHTML5 (uniGUI) por navegador.
2. Na sessão (uniGUI), após autenticar, a identidade fica em `dmSGA.QUsuario` (e o nome em `Main.pas.sUsuario`); a empresa fica em `dmSGA.QEmpresa` (par GrupoEmp+Filial de `CONFIGEMPRESA`).
3. O surface uniGUI abre o portal novo (`Home/PaginaPrincipal`, host `html5.sisacbrasil.com:8022`) num `TUniURLFrame`, repassando **`user_token` (Base64 do nome) e `cod_fi` (filial)** na URL (`Suprimentos/Main.pas`).
4. No lado uniGUI, quando a própria URL carrega `user_token`+`cod_fi`, há decodificação e seleção do usuário/empresa no banco (`Main.pas`); o SUS hoje usa bypass fixo (`Sus/Main.pas`).
5. O DashboardSB (módulo do novo SisacHTML5; hoje projeto ASP.NET Core Razor Pages) **não possui** nenhum ponto de integração com esse fluxo ainda — não existe `Dashboard.Api` no repositório, nem middleware de autenticação, nem código relacionado a identidade.

**O elo que falta determinar é o lado receptor do portal novo (`Home/PaginaPrincipal`):** como ele valida o `user_token`, se cria sessão própria, e se um `Dashboard.Api` compartilharia essa mesma sessão/credencial. Esse código **não está no repositório investigado** (é outro produto/host) — ver §9.

## 5. RELAÇÃO ENTRE IDENTIDADE, AUTENTICAÇÃO E CADASTRO

Distinção obrigatória (mantida desta investigação):

| Conceito | O que é | Onde está na evidência |
|---|---|---|
| **Cadastro do usuário** | Existência do usuário no ecossistema Sisac (base de dados) | `AcessoW`, `USUARIO`, `ACESSOUSUW` (schema investigado e **fechado**); `SelecUsuario`/`EditaUsuario` leem o cadastro |
| **Autenticação da requisição** | Prova de que a requisição veio de alguém legitimamente autenticado no Sisac | Fluxo de login (sessão) OU fluxo `user_token` (por URL) OU SAC (suporte) |
| **Identificação da identidade** | Quem é (nome/`usuario`) | `dmSGA.QUsuario.Nome` / `Main.pas.sUsuario` / `MainModule.usuario` |
| **Validação do contexto** | Confirmação de que a identidade atua naquele contexto operacional | `dmSGA.QEmpresa` (`CONFIGEMPRESA`) — par GrupoEmp+Filial |
| **Autorização no Dashboard** | Permitido/negado (binária, v1) — ADR-009 | **Não determinada tecnicamente** nesta investigação |
| **Contexto de EMPRESA** | Recorte de dados do Dashboard | Transportado hoje como `cod_fi` + GrupoEmp; fonte `CONFIGEMPRESA` |

**Conclusão forte desta distinção:** **existência/cadastro no banco NÃO é prova de requisição autenticada.** O banco pode confirmar que o usuário existe e qual empresa consta no cadastro, mas não pode comprovar que a requisição em questão foi gerada por uma sessão legítima do Sisac. Para isso é necessário que o **lado Sisac apresente a identidade/contexto** (por seu mecanismo de sessão/transporte) e que o Dashboard **valide** essa apresentação — direto no banco, sozinho, insuficiente.

## 6. RELAÇÃO COM EMPRESA

- A empresa, no ecossistema, é materializada como **par `GrupoEmp` + `Filial`** em `CONFIGEMPRESA` (`ElsoftProc.TrocaEmpresa`), e a sessão guarda essa escolha em `dmSGA.QEmpresa`.
- No transporte observado, a identidade carrega apenas **`cod_fi`** (filial) e o GrupoEmp é fixado como `'01'` (`Main.pas`/`Sus/Main.pas`: `TrocaEmpresa('01', cod_fi)`).
- ADR-009 limita o Dashboard à **EMPRESA** na primeira fase. A evidência indica que o Dashboard precisará receber (do ambiente Sisac) ao menos o par **identidade + empresa**, e que a empresa provavelmente chegara como `GrupoEmp`(+`Filial`) — mas **como exatamente** esse par será transportado/validado para o Dashboard.Api **não está determinado**.
- O mapeamento "qual usuário pode operar em qual empresa" não foi resolvido pelas investigações fechadas (schema): `USUARIO` e `AcessoW` possuem `GrupoEmp`/`Filial`, mas o vínculo lógico entre identidade (=`AcessoW`) e `USUARIO` falhou nos testes G/G1 (0 correspondências), e não há PK/FK formal. Portanto **não há evidência de qual tabela define a relação identidade↔empresa** para o caso do Dashboard.

## 7. PAPEL POSSÍVEL DAS TABELAS `AcessoW`, `ACESSOUSUW` E `USUARIO`

Reproduzindo apenas as conclusões da investigação **já encerrada** (sem inferir uso pelo Dashboard):

- **`AcessoW`** (2 registros; 19 colunas; `NOME`, `PERFIL`(?), `PERFILACESSO`, `USUARIO`, `GrupoEmp`, `Filial`, `SENHA` existente — metadado; nenhum valor). Foi a base de **autenticação/autorização** no código Delphi (`BuscaAcesso`, `AcessoUsuario`, `PerfilUsu`). É o candidato natural de "quem é autorizado" e carrega contexto `GrupoEmp`/`Filial`.
- **`ACESSOUSUW`** (15 registros; 7 colunas): **autorização granular** por código de programa/`Usuario`; vínculo `ACESSOUSUW.Usuario → AcessoW.USUARIO` em 9/15 (3 usuários distintos, 1 com `AcessoW`).
- **`USUARIO`** (3.245 registros; 24 colunas): **descartada** como tabela de autenticação (0 correspondências com `AcessoW`); existe como cadastro amplo (usuários do sistema? médicos?) com `GrupoEmp`/`Filial`, sem PK/FK.

**Status para esta investigação:** a **semântica de `PERFIL`/`PERFILACESSO` não foi coletada** (domínio sensível adjacente) e o vínculo alternativo por `NOME` não foi avaliado. Não há como afirmar, com a evidência atual, **qual tabela o Dashboard.Api deveria consultar** para confirmar autorização binária ou contexto de empresa. Isso permanece **em aberto** para a etapa que decidir o mecanismo.

## 8. ALTERNATIVAS TÉCNICAS ENCONTRADAS (somente investigação — nenhuma escolhida)

> Todas as alternativas são **candidatas a investigação adicional**, com evidências parciais. Nenhuma foi selecionada.

### A. Reutilizar o transporte existente `user_token` + `cod_fi` (query string para o portal novo)
- **Evidência:** `Suprimentos/Main.pas` (linha 82) já chama `Home/PaginaPrincipal?user_token=...&cod_fi=01`; `Main.pas` (linhas 94-122) já consome `user_token`/`cod_fi` e deriva usuário/empresa.
- **Funcionamento observado:** a identidade (Base64 do nome) e a filial viajam pela URL até o portal; o portal precisaria validar e criar sessão.
- **Vantagens:** mecanismo já em uso no ecossistema; sem código novo no surface Sisac.
- **Limitações:** o `user_token` observado no lado uniGUI é só o nome em Base64 — sem assinatura/expiração visível → **não prova autenticação**; a validade real depende do receptor. Transporte em URL (histórico/logs). GrupoEmp fixado em '01'.
- **Riscos:** replicar esse fluxo no Dashboard sem o lado da validação perpetuaria o problema "cadastro ≠ autenticação". Requer evidência do receptor.
- **Informações necessárias:** como `Home/PaginaPrincipal` valida `user_token`, se gera sessão/cookie, e se `Dashboard.Api` pode consumir a mesma sessão.

### B. Compartilhar a sessão/cookie do SisacHTML5 (uniGUI) ou do portal novo
- **Evidência:** a sessão uniGUI existe (`uniGUI` server-side; `dmSGA`/`UniMainModule` por sessão); o portal novo é outro host (`html5.sisacbrasil.com:8022`).
- **Funcionamento observado:** identidade/contexto ficam em objetos de sessão do servidor; cookie do navegador identifica a sessão.
- **Vantagens:** identidade já "carregada" uma vez; consistente com "Dashboard sem autenticação própria".
- **Limitações:** dominios/hosts distintos (`8022` vs onde o Dashboard for hospedado); sem evidência de como um `Dashboard.Api` enxerga essa sessão; cookies de sessão são do servidor uniGUI/portal.
- **Riscos:** acoplamento de sessão entre aplicações; escopo de cookie/domínio.
- **Informações necessárias:** se o portal novo e o Dashboard estarão no mesmo domínio/APP; se existe API de validação de sessão.

### C. Consumir um endpoint existente de validação/identidade
- **Evidência:** existe `ApiSAC` (`Autenticar` — **suporte**), e o portal novo expõe `Home/PaginaPrincipal`. Nenhum endpoint genérico "sessão atual" foi encontrado no código investigado.
- **Funcionamento observado:** SAC autentica suporte (Login/Matricula/Senha) — não é o caso de uso do Dashboard.
- **Vantagens:** se existir no portal novo um endpoint de validação de sessão/token, seria o elo forte.
- **Limitações:** não verificado (fora da base de código acessível).
- **Riscos:** inventar endpoint inexistente.
- **Informações necessárias:** existência/contrato de API do portal novo para validar sessão/identidade.

### D. Consulta direta ao banco (hipótese do responsável — investigada, NÃO adotada)
- **Evidência:** acesso read-only validado ao CASAMATER (`t03-acesso-sql-readonly.md`, `dashboard_readonly`); schema das tabelas de identidade **investigado e fechado**.
- **Funcionamento possível:** o Dashboard confirmaria cadastro/perfil/empresa lendo `AcessoW`/`ACESSOUSUW`/`USUARIO`/`CONFIGEMPRESA`.
- **Vantagens:** o Dashboard já tem leitura; pode corroborar existência, perfil binário e empresa cadastral.
- **Limitações:** **não prova que a requisição atual é autenticada** (nada na base distingue uma requisição legítima de uma forjada); sem PK/FK; vínculo identidade↔empresa não confirmado; `USUARIO` descartada; dependência semântica de `PERFIL` não coletada. Sozinha, é insuficiente para autenticação.
- **Riscos:** tratar cadastro como autenticação (violaria ADR-009); confiar em colunas de autorização não confirmadas.
- **Informações necessárias:** além do banco, quem entrega a afirmação de identidade/contexto vinda do Sisac e como o Dashboard a valida.

## 9. LIMITAÇÕES E LACUNAS

1. **Código do portal novo não acessível:** `Home/PaginaPrincipal` (host `html5.sisacbrasil.com:8022`) é outro produto/host — **como ele valida `user_token` e como cria/víncula sessão é INDETERMINADO**. → **"mecanismo ainda não determinado".**
2. **`MainModule.usuario`/`codFilial`:** propriedades existem, mas nenhum ponto de atribuição foi localizado — uso real não provado.
3. **Flow `user_token` não tem validação visível no lado uniGUI**: a amostra decodifica Base64 e seleciona o usuário sem assinatura/expiração visível; não é prova de autenticação.
4. **Relação identidade↔empresa não resolvida** (schema fechado): vínculo lógico fragil (`ACESSOUSUW`→`AcessoW` 9/15; `USUARIO` descartada; `NOME` não avaliado; sem PK/FK).
5. **Semântica de `PERFIL`/`PERFILACESSO`** não coletada — não dá para afirmar como a autorização binária será confirmada no banco.
6. **Nenhum SQL executado** nesta investigação (respeitando o fechamento das investigações e a nota de degradação do servidor na sessão anterior) — toda evidência de dados veio dos handoffs fechados e do código-fonte.
7. **uniGUI runtime** não observado em execução; a descrição da sessão baseia-se na estrutura do código (`GetModuleInstance`), não em depuração.
8. O repositório do Dashboard não possui `Dashboard.Api` nem qualquer artefato de identidade — a integração partirá do zero.

## 10. CONCLUSÃO DA INVESTIGAÇÃO

1. A identidade do usuário autenticado **existe na sessão do SisacHTML5** (`dmSGA.QUsuario`; nome em `Main.pas.sUsuario`), e o contexto de empresa vem de `CONFIGEMPRESA` (`dmSGA.QEmpresa`, par GrupoEmp+Filial).
2. **Existe um transporte de identidade+filial já em uso** entre o surface uniGUI e o portal novo (`user_token` + `cod_fi`), e o próprio SisacHTML5 consome esse mesmo par para "logar" via URL.
3. O lado **receptor** (portal novo) desse transporte — que define como o Dashboard.Api validaria a identidade/sessão — **está fora da base de código acessível**.
4. O banco pode **corroborar** o cadastro e o contexto de empresa, mas **não pode, sozinho, autenticar a requisição** (cadastro ≠ autenticação).
5. Logo: **o mecanismo pelo qual o Dashboard.Api identificará e validará tecnicamente a requisição autenticada do SisacHTML5 é, neste momento, "mecanismo ainda não determinado".** A lacuna não é preenchida com suposição.

## 11. PRÓXIMA DECISÃO NECESSÁRIA

1. **Obter evidência do lado do portal novo** (`Home/PaginaPrincipal`): como `user_token`/`cod_fi` são validados e se há sessão/API reutilizável pelo Dashboard — é o pré-requisito para escolher a alternativa §8.
2. **Decidir** (com essa evidência) entre: (a) continuar usando `user_token`+`cod_fi`/sessão do portal; (b) expor/consumir validação de sessão do portal; (c) um mecanismo definido com o produto — respeitando ADR-009.
3. **Definir, em etapa futura**, como a autorização binária será confirmada (baseado na semântica de `PERFIL`/`AcessoW` e na relação identidade↔empresa — pendências do schema fechado), e o significado de "empresa" para o Dashboard (GrupoEmp? Filial? qual origem?).

> **Regras preservadas:** não reabrir `Login.pas`/`ElsoftProc.pas`/criptografia/`SenhaSistema`/mecanismo de suporte; não registrar segredos; não escolher implementar mecanismo; não alterar código/banco/PLAN/PRD/ADR.

---

## OBSERVAÇÃO FUTURA (IDEIA DE PRODUTO — registro apenas)

Fora da investigação de autenticação e **sem alteração de PLAN-001 ou código**: como comportamento desejado da primeira experiência do Dashboard, os indicadores deveriam, **por padrão**, apresentar as **últimas 24 horas**. Registrado apenas como observação para etapa de produto futura; nenhuma regra, código ou alteração documental foi produzida nesta tarefa por conta dessa ideia.

---

## REGISTRO DA SESSÃO (AO FINAL)

- **Arquivo criado:** `docs/handoffs/2026-09-22-investigacao-identidade-sessao-dashboard-api.md`
- **Arquivos alterados:** somente o handoff acima. **Nenhum** código, SQL, PLAN-001, PRD, ADR ou outro documento foi alterado.
- **Nenhum commit; nenhum push.**