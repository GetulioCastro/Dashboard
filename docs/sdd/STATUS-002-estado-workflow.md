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

## 3. Tarefas
| Tarefa | Status | Próxima ação |
|---|---|---|
| T-01..T-07 | Concluído | — |
| **T-08** | **Implementado (validação pendente)** | Integrados CA-03/CA-09/CA-18 travados por R-05 |
| T-09..T-13, T-19 | Pendente | Elegíveis por dependência — ver §6, tensão não resolvida |
| T-14..T-18, T-20, T-21 | Pendente | Bloqueadas |

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
