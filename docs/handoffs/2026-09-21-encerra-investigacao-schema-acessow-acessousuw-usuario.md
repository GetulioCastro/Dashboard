# Handoff — Encerramento da investigação SQL do schema ACESSOW / ACESSOUSUW / USUARIO (21/09/2026)

> **Data:** 2026-09-21
> **Modo:** MODO ENGENHEIRO
> **Escopo:** Investigação read-only (somente catálogo + agregados não sensíveis) do schema de `ACESSOW`, `ACESSOUSUW` e `USUARIO` no CASAMATER, para subsidiar a decisão arquitetural de autenticação/autorização do DashboardSB
> **Arquivos de referência:** `docs/handoffs/2026-09-21-fechamento-autenticacao.md`, `docs/handoffs/2026-09-21-encerra-investigacao-r02-r05.md`, `docs/architecture/t03-acesso-sql-readonly.md`
> **Execução:** consultas executadas **manualmente** via SSMS/sqlcmd pelo usuário (credencial fora do agente); resultados trazidos para a conversa
> **Decisão humana:** validação concedida por etapas (A–F autorizadas primeiro; G/H executadas manualmente após) — **nenhum dado inventado, nenhuma consulta reexecutada pelo agente**

---

## 1. OBJETIVO

Determinar se a estrutura existente do CASAMATER permite identificar um **usuário** e seu **perfil/autorização** de forma compatível com o DashboardSB — exclusivamente em modo leitura.

## 2. MÉTODO

1. Consultas de **catálogo/metadados** A–F (`sys.tables/partitions/columns/indexes/key_constraints/foreign_keys` + colunas homônimas) — primeira autorização.
2. Análise do resultado de A–F e, em etapa posterior executada manualmente pelo usuário, consultas de **agregados sobre colunas não sensíveis** G/G1 (relação `USUARIO` × `ACESSOW`) e H/H1/H2 (relação `ACESSOUSUW` × `ACESSOW`).
3. Nenhuma coluna de senha, hash, token ou chave foi selecionada, agregada ou exibida.

## 3. GUARDRAILS (respeitados)

- Somente leitura; nenhum objeto criado/alterado/removido.
- Nenhum `INSERT`/`UPDATE`/`DELETE`; nenhuma procedure executada.
- Nenhum valor de senha/hash/token/segredo registrado neste documento.
- Sem retorno ao código Delphi (`Login.pas`, `ElsoftProc.pas`, `EngeplusCript`) e sem investigar criptografia.
- Sem implementação de código C#; sem alteração de código, testes, banco, PLAN-001 ou documentação existente.

---

## 4. RESULTADOS A–F (metadados)

### A — Existência / linhas aproximadas

| Tabela | Registros |
|---|---|
| `dbo.ACESSOUSUW` | **15** |
| `dbo.AcessoW` | **2** |
| `dbo.USUARIO` | **3.245** |

### B — Colunas / tipos (metadados; nenhum valor)

**`ACESSOUSUW`** (7 colunas): `Usuario varchar(10)`, `CodigoA varchar(50)`, `Descr varchar(100)`, `Exclusivo char(1)`, `UsuSist varchar(10)`, `Data datetime`, `Condicao char(1)` — todas NULL.

**`AcessoW`** (19 colunas): `NOME varchar(20)`, `NOMECOMPLETO varchar(100)`, `SENHA varchar(200)` ⚠️, `DATAVALID datetime`, `FUNCAO varchar(30)`, `MODO varchar(50)`, `CODMEDICO varchar(5)`, `CODENF varchar(5)`, `CELULAR varchar(50)`, `EMAIL varchar(50)`, `IDACESSO int NOT NULL IDENTITY`, `SETORUSU varchar(50)`, `DATASIST datetime`, `USUARIO varchar(10)`, `CONDICAO char(1)`, `PERFIL varchar(20)`, `GrupoEmp char(2)`, `Filial char(2)`, `PERFILACESSO varchar(200)` — demais NULL.

> A coluna `SENHA` existe na estrutura (`AcessoW`) e **não teve nenhum valor lido, agregado ou registrado** — permanece fora de escopo de dados.

**`USUARIO`** (24 colunas): `USUARIO varchar(15)`, `CODMEDICO varchar(5)`, `NOME varchar(10)`, `CRM varchar(10)`, `DATAINI/HORAINI/DATAFIM/HORAFIM datetime`, `NIVEL varchar(10)`, `ACESSO varchar(50)`, `DATASIST datetime`, `CODEMP varchar(3)`, `CONTROLPR varchar(1)`, `SETOR varchar(50)`, `MEDAGD varchar(5)`, `GRUPOEMP char(2)`, `FILIAL char(2)`, `MODO varchar(50)`, `MEDESPERA varchar(5)`, `POSTO varchar(15)`, `ORIGEM varchar(15)`, `MAQUINA varchar(50)`, `CODPACIENTE varchar(15)`, `PSTRING varchar(100)` — todas NULL (sem NOT NULL ou Identity declarados).

### C — Índices

**`USUARIO`:**
- `_dta_index_USUARIO_7_59147256__K17_K16_K22` — NONCLUSTERED — `FILIAL, GRUPOEMP, MAQUINA`
- `_dta_index_USUARIO_c_7_59147256__K17_K16_K3` — CLUSTERED — `FILIAL, GRUPOEMP, NOME`
- `USUARIO0` — NONCLUSTERED — `UNIQUE = 1` — `NOME, FILIAL, GRUPOEMP`

**`AcessoW` e `ACESSOUSUW`:** nenhum índice retornado no resultado apresentado.

### D — PK / UNIQUE CONSTRAINTS

**0 linhas.** Não há `PRIMARY KEY` nem `UNIQUE CONSTRAINT` nas três tabelas. O índice `UNIQUE` `USUARIO0` continua existindo, mas **não é** `UNIQUE CONSTRAINT`.

### E — Foreign Keys

**0 linhas.** Não existem FKs declaradas envolvendo `ACESSOW`, `ACESSOUSUW` ou `USUARIO`.

### F — Colunas homônimas (pistas de vínculo)

| Coluna | Tabelas que a possuem |
|---|---|
| `CODMEDICO` | AcessoW, USUARIO |
| `Condicao` | AcessoW, ACESSOUSUW |
| `DATASIST` | USUARIO, AcessoW |
| `Filial` | AcessoW, USUARIO |
| `GrupoEmp` | AcessoW, USUARIO |
| `MODO` | USUARIO, AcessoW |
| `NOME` | USUARIO, AcessoW |
| `Usuario` | ACESSOUSUW, USUARIO, AcessoW |

---

## 5. RESULTADOS G/G1 e H/H1/H2 (agregados não sensíveis)

### G — Relacionamento `USUARIO` × `ACESSOW`

| Métrica | Valor |
|---|---|
| `TotalUSUARIO` | 3.245 |
| `USUARIO_ComAcessoW` | **0** |
| `USUARIO_SemAcessoW` | 3.245 |

### G1 — Decomposição do relacionamento

| Métrica | Valor |
|---|---|
| `TotalUSUARIO` | 3.245 |
| `Match_Usuario` | 0 |
| `Match_Usuario_GrupoEmp` | 0 |
| `Match_Usuario_Filial` | 0 |
| `Match_Completo` | 0 |

**Evidência:** `USUARIO.USUARIO` não encontrou correspondência em `AcessoW.USUARIO`, nem isoladamente, nem combinado com `GrupoEmp` e/ou `Filial`.

### H — `ACESSOUSUW` × `ACESSOW`

| Métrica | Valor |
|---|---|
| `TotalAcessoUSUW` | 15 |
| `Permissoes_ComAcessoW` | 9 |
| `Permissoes_SemAcessoW` | 6 |

### H1 — Usuários distintos

| Métrica | Valor |
|---|---|
| `TotalPermissoes` | 15 |
| `UsuariosDistintosPermissoes` | **3** |
| `UsuariosComAcessoW` | 1 |
| `UsuariosSemAcessoW` | 2 |

### H2 — Distribuição das permissões

| Com AcessoW | Condicao | Exclusivo | Qtd |
|---|---|---|---|
| SIM | NULL | NULL | 5 |
| SIM | NULL | S | 4 |
| NÃO | NULL | NULL | 2 |
| NÃO | N | NULL | 1 |
| NÃO | S | NULL | 3 |

**Total = 15.**

---

## 6. INTERPRETAÇÃO / HIPÓTESES DESCARTADAS

1. **Hipótese descartada — `USUARIO` como vínculo de autenticação com `AcessoW`:** o relacionamento `USUARIO → AcessoW` por `USUARIO` **não foi confirmado** — os testes quantitativos retornaram **zero correspondências** (G/G1). Não há vínculo direto por essa coluna.
2. **Hipótese descartada — relacionamento formal via PK/FK:** não existem PK nem FK entre as três tabelas (D/E). Qualquer relacionamento é **lógico/aplicacional** até evidência em contrário.
3. **Hipótese descartada — causa dos 6 registros sem `AcessoW`:** não é possível concluir que sejam inativos, exclusivos ou relacionados a causa específica — H2 não demonstra isso.
4. **Hipótese NÃO adotada — `USUARIO` como tabela de autenticação:** não registrar `USUARIO` como tabela de autenticação. A investigação do código Delphi (handoff de fechamento de autenticação) já identificou **`AcessoW`** na autenticação/autorização e **`ACESSOUSUW`** na autorização granular — coerente com os dados.
5. Existe correspondência **`ACESSOUSUW.Usuario` → `AcessoW.USUARIO`** em **9 de 15** registros de permissão; as 15 permissões pertencem a **3 usuários distintos**; somente **1** desses usuários possui correspondência em `AcessoW`.

---

## 7. LIMITAÇÕES

- Amostra pequena em `AcessoW` (2) e `ACESSOUSUW` (15); `USUARIO` com 3.245.
- Sem PK/FK formais — vínculos lógicos não formalizados no schema.
- Vínculo lógico alternativo **por `NOME`** (presente em `USUARIO` e `AcessoW`; `USUARIO.NOME` é `varchar(10)` vs `AcessoW.NOME` `varchar(20)`) **não foi avaliado nesta etapa** — caso a decisão arquitetural exija, é o próximo ponto de verificação candidato, sem reabrir a investigação de autenticação.
- Não foi avaliada a semântica de `PERFIL`/`PERFILACESSO` em `AcessoW` (valores não coletados por conterem domínio sensível adjacente) — a verificação do perfil `Master`/bypass citada no handoff de autenticação permanece como **indício** (coluna existe), não como valor confirmado.
- `SENHA varchar(200)` existe em `AcessoW` — **apenas metadado; nenhum valor lido**.

---

## 8. DECISÃO — ENCERRAMENTO DA INVESTIGAÇÃO SQL NESTE PONTO

A investigação read-only do schema das três tabelas é **encerrada nesta data**. As evidências coletadas são suficientes para o **processo de decisão arquitetural**, sem necessidade de novas consultas agora. **Nenhuma consulta será reexecutada pelo agente.**

## 9. PRÓXIMO PASSO

**Consolidar a decisão arquitetural de autenticação/autorização do DashboardSB** (decisão arquitetural: **PENDENTE**):

1. Decidir se/recomo o DashboardSB reutiliza a identidade existente do SisacHTML5 (`AcessoW` como base de autenticação; `ACESSOUSUW` como autorização granular; vínculo lógico por `Usuario` e, se necessário, avaliar `NOME`).
2. Definir o papel **MASTER** no DashboardSB e sua correspondência com `AcessoW.PERFIL`/bypass.
3. Desenhar o fluxo da API/identidade.
4. **Somente depois** iniciar qualquer implementação.

## 10. NÃO FAZER

- Não retornar à investigação de `Login.pas`, `ElsoftProc.pas` ou criptografia.
- Não investigar novos mecanismos de autenticação.
- Não registrar senhas, hashes, tokens ou segredos.
- Não executar novas consultas (salvo autorização humana explícita).
- Não alterar código, testes, banco, PLAN-001 ou documentação existente.

---

**Estado acumulado do projeto (para a próxima sessão):**
- T-08: implementado, **não validado/concluído, não commitado** (R-02/R-04/R-05 pendentes).
- T-09..T-13: não iniciados.
- Investigação de autenticação (código Delphi): **encerrada**.
- Investigação SQL do schema de autenticação (A–F, G/G1, H/H1/H2): **encerrada**.
- Decisão arquitetural de autenticação/autorização: **PENDENTE** (próximo passo em aberto).