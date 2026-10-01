# Handoff — Fechamento da investigação do mecanismo `user_token` / `cod_fi` (22/09/2026)

> **Data:** 2026-09-22
> **Modo:** MODO ENGENHEIRO — FECHAMENTO DA INVESTIGAÇÃO `user_token` / `cod_fi`
> **Escopo da leitura:** leitura cirúrgica, somente leitura, dos seguintes arquivos:
> - `C:\ProjetosSB\SisacHTML5\Suprimentos\Main.pas`
> - `C:\ProjetosSB\SisacHTML5\AplicacaoExterna.pas`
> - `C:\ProjetosSB\SisacHTML5\Main.pas`
> - `C:\ProjetosSB\SisacHTML5\Clinicas\Main.pas`
>
> Nada foi alterado: código, SQL, PLAN-001, PRD, dicionário de dados, ADR-009 ou testes. Nenhum commit/push.

---

## 1. Evidências registradas

- `Suprimentos\Main.pas` monta a URL da aplicação externa com `user_token` e `cod_fi` (`ChamaGerencRelat('AplicacaoExterna')` → `CarregarModulo` → `ShowModal`).
- O produtor ativo identificado utiliza **`user_token` hardcoded em Base64** no código-fonte — não é produzido dinamicamente a partir da sessão do usuário logado.
- `cod_fi` é **`'01'` hardcoded** nesse produtor.
- `AplicacaoExterna.pas` apenas **acrescenta um cache-breaker** (`&t=<hhmmss>`) e atribui a URL ao **`TUniURLFrame`** (iframe); nenhuma transformação, validação ou manipulação do token.
- `Main.pas` / `Clinicas\Main.pas` **recebem `user_token` e `cod_fi` por parâmetros da URL** (`UniApplication.Parameters.Values['user_token']` / `['cod_fi']`).
- `user_token` é decodificado com **`TNetEncoding.Base64.Decode`** e atribuído a `sUsuario`.
- Após a decodificação são executados, na ordem: `ConectaBD`, `TrocaEmpresa('01', cod_fi)` (se `cod_fi` não vazio), `EditaUsuario` e `SelecUsuario`.
- **Não foi identificada, nos trechos lidos, validação de assinatura, expiração, origem, nonce ou integridade do token.** O `try/except` presente trata falhas de execução (exibindo `Aviso`), não valida autenticidade.
- **O servidor de `Home/PaginaPrincipal` não está presente nos arquivos analisados** — o lado que atende `http://html5.sisacbrasil.com:8022/Home/PaginaPrincipal` não foi localizado.
- **DashboardSB atualmente não possui integração com esse mecanismo** (nenhum consumo de `user_token`/`cod_fi` em código; apenas documentação da investigação e protótipo de login simulado).

## 2. Conclusão técnica

O mecanismo identificado deve ser classificado, com base nos arquivos lidos, como **transporte de contexto com assunção de identidade**, e **não** como mecanismo comprovado de autenticação. Note também:

- Não é autorização (nenhuma checagem de permissão foi observada no trecho lido).
- É transporte de contexto parceado com a assunção de uma identidade (usuário + filial), cuja legitimidade não é verificada no lado analisado.

## 3. Decisão

**Não reutilizar o `user_token` atual como mecanismo de autenticação do `Dashboard.Api` sem compreender o lado servidor que atende `Home/PaginaPrincipal`.**

## 4. Lacuna principal

Não foi localizado o código do servidor responsável por:

```
http://html5.sisacbrasil.com:8022/Home/PaginaPrincipal
```

## 5. Próximo passo

Fazer posteriormente uma investigação **CIRÚRGICA** para tentar localizar, no ambiente atual, o projeto/serviço que implementa esse endpoint.

## 6. Limitações

- Não inferir como o servidor remoto funciona.
- Não afirmar que o token é inseguro no servidor sem examinar o servidor.
- Não afirmar que existe autenticação no servidor.
- Não reabrir a investigação de Login, `AcessoW`, `ACESSOUSUW` ou `USUARIO`.
- Não tratar a existência do "Sisac Relatórios" como fato técnico comprovado pelo código local; registrar apenas como contexto informado pelo responsável pelo projeto.

## 7. Contexto de produto (não técnico)

- A finalidade do **DashboardSB** é inspirada na ideia do **Sisac Relatórios**, porém o DashboardSB terá **indicadores gráficos** em vez de relatórios PDF.
- Essa informação é **contexto de produto**, não conclusão técnica da investigação.

---

## 8. Complemento — FECHAMENTO DA INVESTIGAÇÃO AMPLIADA (LOCALIZAÇÃO DO SERVIDOR)

> **Modo:** MODO ENGENHEIRO — FECHAMENTO DA INVESTIGAÇÃO (localização do servidor `Home/PaginaPrincipal`)
> **Data:** 2026-09-22 (mesma sessão)

### 8.1 Escopos autorizados (únicos investigados nesta etapa)

- `C:\ProjetosSB\SghProg`
- `C:\ProjetosSB\SisacHTML5`

### 8.2 Conclusão

O servidor responsável por:

```
http://html5.sisacbrasil.com:8022/Home/PaginaPrincipal
```

**NÃO foi localizado** em nenhum dos dois escopos autorizados.

### 8.3 Evidências da ampliação

- **`SisacHTML5`** contém apenas **referências de cliente à URL** (montagem da URL para a aplicação externa):
  - `Suprimentos\Main.pas:81-82`
  - `Main.pas:191`
  - `Clinicas\Main.pas:191`
- **`SghProg`** **não apresentou implementação nem referência textual relevante** ao endpoint:
  - `PaginaPrincipal`: zero ocorrências.
  - `html5.sisacbrasil` / `:8022`: zero ocorrências.
  - `sisacbrasil`: apenas `www.sisacbrasil.com.br` e `sisacbrasil.g4flex.com.br` (links, e-mails, SEO) — sem a porta 8022.
- **Falsos positivos de `8022` em `SghProg`:** GUID `{C6802233-...}`, erro CAPICOM `$80880220`, OS ticket `68022` e hashes hex — **não são referências ao endpoint**.
- **Não foram encontradas estruturas** `Home/`, `Controllers/`, `.csproj` ou `.sln` que indiquem outro servidor Web nesses escopos.
- **`ServerModule.pas` analisados** (raiz, Suprimentos, Clinicas, Sus, SAG, Financeiro) correspondem ao **próprio servidor uniGUI (Delphi)** de cada aplicação e **não implementam** `Home/PaginaPrincipal`.

### 8.4 Cadeia comprovada preservada

A cadeia já comprovada permanece válida e **não é removida** desta documentação:

```
Suprimentos\Main.pas
-> AplicacaoExterna.pas
-> TUniURLFrame
-> Home/PaginaPrincipal:8022
```

`AplicacaoExterna.pas` permanece documentado (container `TUniURLFrame` + cache-breaker apenas).

### 8.5 Lacuna

O **código-fonte do servidor `:8022`** permanece **fora dos escopos investigados**.

### 8.6 Decisão

**Não reutilizar o mecanismo `user_token`/`cod_fi` atual como autenticação do `Dashboard.Api` sem conhecer o servidor receptor.**

### 8.7 Escopo de investigação futura não autorizado

Nenhuma investigação adicional fora de `SghProg` e `SisacHTML5` foi autorizada nesta etapa. Não investigar:

- `Back-Ups`, `DelphiWeb*`, `SGE`, `SGCH`, `Componentes`, `files`, `HelloWorldUnigui`, arquivos compactados ou qualquer outro diretório fora dos dois escopos autorizados.
- Excluir também qualquer caminho com prefixo `old*`/`OLD*`.

---

## Registro da sessão

- **Arquivo criado:** `docs/handoffs/2026-09-22-fechamento-user-token-codfi.md`
- **Arquivos alterados:** somente o handoff acima. Nenhum código, SQL, PLAN-001, PRD, dicionário de dados, ADR-009 ou teste foi alterado.
- **Nenhum commit; nenhum push.**