# STATUS-002 — Estado do Workflow (transição Leanwork SDD + Atendimentos Reais)

> **Tipo:** estado do workflow — recuperação persistente entre sessões
> **Data:** 2026-09-28
> **Substitui:** `STATUS-001-checkpoint-kickoff.md` (2026-09-16) — **preservado,
> não apagado**; descreve o kick off, hoje superado
> **Regra:** ao encerrar uma sessão, ATUALIZE este arquivo. Não crie handoff novo
> por padrão.

## 1. Ordem de recuperação do contexto
1. `AGENTS.md` — regras permanentes, stack, comandos
2. Este arquivo — estado atual
3. `docs/plans/PLAN-001-dashboard-indicadores.md` — tarefas T-XX
4. `docs/reviews/` — findings R-XX
5. `docs/handoffs/` — histórico como evidência; não reabrir

## 2. Fase atual
| Fase | Artefato | Estado |
|---|---|---|
| Arquitetura | `docs/architecture/proposta-arquitetural.md` + ADR-007..010 | Aprovada |
| Requisitos | PRD-001 (Epic) / PRD-002 (Atendimentos) | Rascunho — revisão 2 |
| Especificação | SPEC-UI-001, SPEC-VIS-001 | Aguardando validação humana |
| Plano | PLAN-001 (T-01..T-21) | Execução em andamento |
| Revisão | REVIEW-T-08-2026-09-18 | Aprovado com ressalvas |

> **Nota de reconciliação (2026-10-02):** Reconciliação documental do estado do workflow realizada sem alterar documentos históricos.

## 3. Tarefas
| Tarefa | Status | Próxima ação |
|---|---|---|
| T-01..T-07 | Concluído | — |
| **T-08** | **Implementado e Validado (reconciliado 2026-10-02)** | Integrados CA-03/CA-09/CA-18 (estado reconciliado 2026-10-02) |
| T-09..T-13, T-19 | Pendente | Elegíveis por dependência — ver §6, tensão objeto de reconciliação documental em 2026-10-02 |
| T-14..T-18, T-20, T-21 | Pendente | Bloqueadas |

### 3.1 — PLAN-002 (Análise por Convênio) — registro aditivo (2026-10-05)

Novo plano, **independente** do grafo T-01..T-21 do PLAN-001. Não altera o estado da §3.

| Tarefa | Status | Próxima ação |
|---|---|---|
| **T-01** — Inventariar pontos de extensão por indicador | **Concluído** (2026-10-05) | — |
| **T-03** — Extender repositórios Data (SQL/consulta) para Convênio | **Concluído** (2026-10-05) | Entrega: 3 repositórios em `Dashboard.Data/Repositories` implementando `IIndicadorConvenioNominalRepository` (Atendimentos/Consultas/Exames); JOIN por chave composta, `C.DESCR` integral, filtros RN-07 e tipos 1/3; L2 e L3 resolvidas com evidência read-only; build 0 avisos/0 erros; 48 testes aprovados |
| **T-04** — Regra de ordenação + desempate (R5) | **Concluído** (2026-10-05) | Ordenação ajustada para `ORDER BY COUNT(1) DESC, C.DESCR ASC, C.CODCONVENIO ASC, C.GRUPOEMP ASC, C.FILIAL ASC` nos 3 repositórios nominais (desempate técnico determinístico). Preserva identidade da chave composta, não altera `Identificacao = C.DESCR`. R5 resolvida. Build 0 avisos/0 erros; 48 testes aprovados. |
| **T-05** — Integrar no backend Web (PageModel) filtros por Convênio | **Concluído** (2026-10-05) | Registrados os três repositórios nominais na DI do Web (`IIndicadorConvenioNominalRepository` → Atendimentos/Consultas/Exames). Sem alteração de UI, sem alteração de autenticação, sem impacto em repositórios/contratos existentes. Build 0 avisos/0 erros; 48 testes aprovados. |
| T-06..T-08 | Pendente | L2/L3 resolvidas; R5 resolvida na T-04 |

**T-02 — CONCLUÍDA em 2026-10-05 (contratos Core, RN-003-03/RN-003-04, valida CA-003-02).**
Criados `Dashboard.Core/DTOs/IndicadorConvenioNominal.cs` (`ConvenioNominalVolume`,
`IndicadorConvenioNominalVisao`) e `Dashboard.Core/Contratos/IIndicadorConvenioNominalRepository.cs`
(`ObterConvenioNominalAsync(IndicatorFilter, CancellationToken)`). **Contrato novo, não extensão**:
`IndicatorFilter`, `IndicadorVisao`, `DimensaoVisao`, `CoverageCategory`,
`IIndicadorVisaoRepository` e `IIndicatorRepository` **permanecem inalterados** — nenhum campo de
convênio nominal foi adicionado ao filtro, porque a análise nominal **não é um filtro** (DEC-003-05
proíbe drill-down/clique; RN-003-01..10 não exigem seleção de convênio). Nomenclatura
`ConvenioNominal` preserva a distinção entre cobertura (RN-26) e identificação nominal. Nenhuma
regra de ordenação no contrato — R5 permanece para T-04. Sem `MODOFAT` em Core. Sem SQL, sem
Web/UI, sem DI (T-03/T-05), sem banco, sem Git.

**L1 — RESOLVIDA em 2026-10-05 (reconstrução documental do PRD-003):**
`docs/prds/PRD-003-analise-por-convenio.md` foi **reconstruído (v0.2)** sob gate humano. Causa
diagnosticada como **falha de materialização original**, não truncamento: o artefato foi criado
já incompleto no commit `cfc5ecc` (2026-10-02), e a verificação de histórico Git somente leitura
confirmou **um único blob** em todo o objeto alcançável, **nenhum segundo commit, nenhum stash,
apenas `main`/`origin/main`** — não existia versão completa a restaurar. Materializados: §1–§5,
**RN-003-01..RN-003-10**, **CA-003-02..CA-003-07**, matriz RN↔CA↔DEC (§5.1), premissas abertas
(§5.2). **CA-003-01 preservada semanticamente; DEC-003-01..DEC-003-06 preservadas literalmente**
( apenas o cabeçalho de estado obsoleto "PENDENTE/VALIDAR" → aprovadas em 2026-10-02).
**Nada foi inventado:** §1–§5 originais, o documento de "autorização" citado na versão parcial e
personas/stakeholders/métricas/diagramas permanecem **não recuperados** (sem fonte no repositório).
**CA-003-08 e CA-003-09 nunca existiram** — família oficial **CA-003-01..CA-003-07**.

**L4 — RESOLVIDA em 2026-10-05 (decisão humana):** a análise nominal representa os **convênios
efetivamente presentes nos registros válidos** de cada indicador no período, **preservando RN-07,
P19 e demais regras aprovadas**. **NÃO** se introduz `MODOFAT = 'C'` como nova regra de produto,
e a análise **não depende** de a categoria de cobertura "Convênio" estar selecionada. Deixa de
bloquear T-03. Materializado em `PRD-003` §3.3 e RN-003-02.

**L2 — ABERTA:** P17 (`CADCONVENIO` no SisacHTML5) segue aberta em `contrato-dados-dashboard.md:223` — sem confirmação no banco alvo. Validar **somente com a credencial de leitura**, read-only, na etapa Data/SQL (T-03). Não bloqueia T-02.

**L3 — ABERTA:** cardinalidade de `CADCONVENIO` sob `(CODCONVENIO, GRUPOEMP, FILIAL)` sem medição registrada; premissa do volume por convênio. Verificação técnica read-only em T-03.

**R5 — ABERTA:** chave final de desempate quando `DESCR` for duplicado (ordenação pode não ser total). **Não inventada no PRD-003** (§5.2); decisão técnica atribuída a **T-04**, sem contradizer DEC-003-01/DEC-003-06.

**Colisão de nomenclatura:** `DimensaoVisao.Convenio` e `CoverageCategory.Convenio` já significam **cobertura** (Particular/Convênio/SUS), não convênio individual — não reutilizar. Materializado como RN-003-03 e CA-003-02.

**Reconciliação do PLAN-002 (2026-10-05):** o mapeamento `Decisões base:` foi corrigido de **posicional** (T-0*n* → DEC-003-0*n*) para **semântico** — T-04 passa a DEC-003-01 + DEC-003-06; T-06 passa a DEC-003-02..DEC-003-05 + SPEC-UI-003; T-01/T-02/T-03/T-05/T-07 sem DEC de apresentação. Nenhum checkbox de escopo funcional alterado. SPEC-UI-003, ADR, PRD-001, PRD-002, PLAN-001 e código **não alterados**.

## 4. R-05 — pendência aberta (bloqueio externo)

**Sequência registrada dos fatos (2026-09-28):**

1. Houve uma falha inicial de autenticação com `dashboard_readonly`.
2. Posteriormente, o login foi provisionado no SQL Server de São Luís.
3. Os testes seguintes apresentaram falha de conectividade/transporte para
   `172.16.2.138:1433`, antes de uma nova validação conclusiva de autenticação.
4. A conectividade TCP apresentou comportamento variável.

**Estado:** R-05 permanece bloqueado externamente, aguardando a infraestrutura.

**Causa:** não inferir causa de rede, ACL, firewall ou SQL sem evidência. Não
atribuir qualquer relação causal ao ZeroTier.

**Fatos que permanecem válidos:**
- SQL Server local anterior: `172.18.100.253`
- Novo SQL Server de São Luís: `172.16.2.138`
- Banco: `CASAMATER`
- Usuário da aplicação: `dashboard_readonly`
- Dashboard somente leitura (ADR-003, ADR-008)

**ZeroTier:** é uma VPN pessoal utilizada para acesso remoto ao notebook de
trabalho. Não faz parte do caminho de acesso ao SQL Server e permanece
formalmente excluído desse caminho.

**Origem desta seção:** itens da sequência 1–4 e a caracterização de
comportamento variável foram fornecidos pelo responsável do projeto em
2026-09-28 e **não foram reconfirmados por teste neste ambiente**. Tratá-los
como declaração registrada, não como medição.

## 5. Divergência documental do R-05

`docs/handoffs/2026-09-24-bloqueio-r05-casamater.md` contém uma descrição da
sequência da falha que **não corresponde à investigação efetivamente realizada**.

O histórico desse arquivo **não é alterado**. A divergência fica registrada
como divergência, e a §4 é a **versão corrigida** do estado atual do R-05.

Não tratar como estado atual nenhuma das afirmações que aquele documento faz e
que a §4 substitui, em particular:
- sequência "handshake OK → autenticação OK → timeout pós-login";
- `SELECT 1` como evidência de sucesso;
- qualquer conclusão sobre firewall, ACL, rota ou SQL sem evidência
  correspondente.

Não inventar causa alternativa para substituir a descrição registrada na §4.

**Segunda divergência, mesma natureza:** `docs/architecture/t03-acesso-sql-readonly.md`
registra, em 2026-09-17, TCP/1433 OK com logon aceito para `172.18.100.253`,
tratado ali como o SQL alcançável. O estado atual registra `172.18.100.253` como
o SQL Server local **anterior** e `172.16.2.138` como o novo alvo. As duas
informações **não são conciliadas por inferência**; a reconciliação depende da
resposta da infraestrutura. O documento histórico não é reescrito.

## 6. Tensão a resolver (decisão humana)
O grafo de dependências marca T-09..T-13 e T-19 como elegíveis, mas o
`REVIEW-T-08-2026-09-18` orienta explicitamente: *"não avançar a T-09"* enquanto
R-01..R-05 estiverem abertos. As duas leituras não concordam. Decidir antes de
retomar a execução.

Em 2026-10-02, por reconciliação documental autorizada, diferenciou-se ESTADO HISTÓRICO (documentos antigos, inalterados, refletindo bloqueios das respectivas datas) de ESTADO RECONCILIADO (2026-10-02): R-01, R-02, R-03/P19 e R-05 encontram-se RESOLVIDOS posteriormente; R-04 encontra-se SUPERADO pelas validações/testes posteriores. As divergências documentais registradas em §5 não foram alteradas. Os handoffs históricos permanecem inalterados.

## 7. Investigações encerradas — não reabrir
| Encerramento | Fonte |
|---|---|
| ATD-VIS-04 e ATD-VIS-05 (gate visual) | `2026-09-25-fechamento-formal-atd-vis-04-atd-vis-05.md` |
| Schema `ACESSOW`/`ACESSOUSUW`/`USUARIO` | `2026-09-21-encerra-investigacao-schema-...md` |
| Autenticação Delphi (`Login.pas`, `ElsoftProc.pas`) | `2026-09-21-fechamento-autenticacao.md` |
| `user_token` / `cod_fi` / servidor `:8022` | `2026-09-22-fechamento-user-token-codfi.md` |
| Identidade e sessão do Dashboard API | `2026-09-22-investigacao-identidade-sessao-dashboard-api.md` |
| Frente Atendimentos por Convênio | `2026-09-23-encerra-frente-investigacao-atendimento-convenio.md` |
| Frente Atendimentos por Sexo | `2026-09-23-encerra-frente-investigacao-atendimento-sexo.md` |
| T-ATD-02 (análise documental) | `2026-09-23-t-atd-02-definicao-analitica-atendimento.md` |
| Diretórios com prefixo `old`/`OLD` | `2026-09-23-handoff-encerramento-sessao.md` |
| SghProg / SisacHTML5 como contrato de dados | `2026-09-22-fechamento-vertical-slice-dashboard.md` |
| Gate de conexão T-05 | `2026-09-17-checkpoint.md` |

## 8. Itens sem lastro documental no repositório
- **`INFRA-01`** — não há registro de ticket em `docs/`
- **ZeroTier** — não há menção a ZeroTier em `docs/`; o fato é declaração do
  responsável, registrada na §4

Nenhum dos dois foi encerrado por documento anterior. Se devem constar como
investigações concluídas, precisam de um documento que os registre.

## 9. Regras de conduta ao retomar
- Não repetir diagnóstico de conectividade enquanto R-05 estiver bloqueado
- Não inventar resultado de teste ausente — a lacuna é o achado
- Não criar handoff por encerramento rotineiro de sessão; atualizar este arquivo
- Não alterar `Dashboard.Web` fora do escopo da tarefa
- Não alterar documentos históricos para "corrigir" divergência — registrar a
  divergência
