# Handoff — Fechamento da investigação de autenticação do SisacHTML5

> **Data:** 2026-09-21
> **Modo:** MODO ENGENHEIRO
> **Escopo:** Investigação de autenticação do SisacHTML5 **encerrada** para fins de decisão arquitetural
> **Arquivos de referência:** `C:\ProjetosSB\SisacHTML5\Main.pas`, `Login.pas`, `ElsoftProc.pas`, `Client_API_REST\uAuthService.pas`, `Client_API_REST\uAuthDTO.pas`, `Componentes\EngeplusCript_TLB.pas`

---

## 1. INVESTIGAÇÃO ENCERRADA — RESUMO DOCUMENTAL

Investigação da autenticação do SisacHTML5 concluída nesta data. **Nenhum segredo, senha, chave, token ou credencial literal é registrado neste documento.**

### Arquivos analisados (somente leitura)
- `Main.pas` — tela principal; usa o usuário já autenticado (`dmSGA.QUsuario.Nome`); autoriza cada módulo via `AcessoUsuario`.
- `Login.pas` — formulário de login (usuário/senha digitados, empresa/filial selecionada).
- `ElsoftProc.pas` — rotinas de autenticação e autorização (`AcessoUsuario`, `BuscaAcesso`, `Criptografar`, `SenhaSistema`, `SelecUsuario`, `EditaUsuario`, `PerfilUsu`).
- `Client_API_REST\uAuthService.pas` / `uAuthDTO.pas` — cliente REST da API de autenticação SAC.
- `Componentes\EngeplusCript_TLB.pas` — classe `TCriptografia` (cifra proprietária usada para senhas).

### Rotinas envolvidas
- **`AcessoUsuario(sUsuario, sCodigoA, sModo)`** — autorização por módulo; bypass total para `SISTEMA` e para perfil `Master`.
- **`BuscaAcesso(sNome)`** — lê a senha armazenada em `ACESSOW`.
- **`Criptografar(sTexto)`** — envolve `TCriptografia.Encriptar`.
- **`SenhaSistema`** — senha do usuário `SISTEMA`, gerada por rotina própria a partir da data do servidor (não reconstruída aqui).
- **`uAuthService.AutenticarSuporte`** — chamada REST para a API SAC (usuários de suporte).

### Diferença autenticação × autorização
- **Autenticação** (Login.pas): provar quem é o usuário comparando senha digitada com a armazenada (via `Criptografar` + `BuscaAcesso`), ou via serviço SAC para usuários de suporte.
- **Autorização** (Main.pas + `AcessoUsuario`): decidir se o usuário autenticado pode abrir cada módulo, pelo perfil (`Master`) ou permissão específica.

### Regra SISTEMA / Perfil='Master' — COMPROVADA
- Usuário `SISTEMA` ⇒ acesso total (bypass) em `AcessoUsuario` e `PerfilUsu`.
- Usuário com `Perfil='Master'` em `ACESSOW` ⇒ acesso total (bypass da checagem de permissão granular).
- Sem esses, a permissão é verificada por código de programa em `ACESSOUSUW`.

## 2. CLASSIFICAÇÃO DE EVIDÊNCIAS

### COMPROVADO
- Fluxo de entrada de usuário/senha no `Login.pas` e comparação com senha armazenada via `Criptografar`/`BuscaAcesso`.
- Regra de bypass `SISTEMA` e `Perfil='Master'` em `AcessoUsuario` (ElsoftProc).
- Leitura da senha armazenada a partir de `ACESSOW`; leitura do usuário autenticado a partir de `USUARIO`.
- Cliente REST `uAuthService` chamando a API SAC (usuários de suporte) e DTO de resposta (status/dados.Liberado).
- Existência da cifra proprietária `TCriptografia` com transformação determinística por caractere e saída hexadecimal.

### INDÍCIO
- Coluna de filtro em `AcessoUsuario`/`ACESSOUSUW` nomeada `Usuario`, enquanto `PerfilUsu` usa `Nome` — convenção de coluna real ainda não confirmada.
- Parâmetro fixo de matrícula no serviço SAC parece identificar a operação, sem confirmação.
- Comentário de "autenticar no banco local" como fallback do SAC, sem ramo de código implementado.

### NÃO DETERMINADO
- Schema (colunas, tipos, chaves) de `ACESSOW`, `ACESSOUSUW` e `USUARIO`.
- Semântica exata do parâmetro de matrícula no serviço SAC.
- Efeito real de `sModo` em `AcessoUsuario` para valores não tratados na função.
- Internos da rotina `SenhaSistema` e das funções auxiliares — **não reconstruídos, por regra de não-divulgação**.
- Análise de robustez criptográfica da cifra proprietária — fora de escopo.

## 3. PRÓXIMO FIO DE RETOMADA

1. **Investigar somente, em leitura, o schema de:**
   - `ACESSOW`
   - `ACESSOUSUW`
   - `USUARIO`
2. **Objetivo:** determinar se o DashboardSB poderá utilizar a identidade existente do SisacHTML5 para autenticação/autorização do gestor da clínica.
3. **Após essa investigação:**
   - decisão arquitetural de autenticação;
   - definição do papel MASTER no DashboardSB;
   - desenho do fluxo da API;
   - **somente depois** iniciar implementação.
4. **Estado atual:**
   - investigação de autenticação: **ENCERRADA**;
   - decisão arquitetural: **PENDENTE**;
   - schema das tabelas: **PENDENTE**;
   - T-08: implementado, porém **não validado/concluído**;
   - T-09: **não iniciado**;
   - implementação T-08: **não entra neste commit**.

### REGRA DE RETOMADA

> Na próxima sessão, não retomar a investigação do `Login.pas`. Retomar diretamente pela validação do schema de `ACESSOW`, `ACESSOUSUW` e `USUARIO`, em modo somente leitura.