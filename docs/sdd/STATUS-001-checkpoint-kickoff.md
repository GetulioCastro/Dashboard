# STATUS-001 — Checkpoint de Kick Off do DashboardSB

> **Tipo:** handoff documental para retomada em nova sessão do OpenCode
> **Criado em:** 2026-09-16
> **Estado geral:** Kick Off da fundação **concluído**. Implementação **NÃO iniciada**.
> **Instrução:** este documento substitui a memória de sessão. Leia-o integralmente antes de qualquer ação.

---

## 1. Identificação

- **Projeto:** DashboardSB / Dashboard
- **Tecnologia:** C# / .NET 10 / ASP.NET Core Razor Pages / Bootstrap 5
- **Estado:** Kick Off da fundação concluído. **Implementação ainda NÃO iniciada.**

---

## 2. Arquitetura aprovada

Fluxo de dados (de cima para baixo):

```
SisacHTML5
    ↓
Banco novo do SisacHTML5
    ↓
Dashboard.Data
    ↓
Dashboard.Core
    ↓
Dashboard.Web
```

### Responsabilidades

**Dashboard.Web**
- Apresentação;
- Razor Pages;
- Interação;
- Gráficos;
- Filtros;
- Experiência do usuário.

**Dashboard.Core**
- Regras de negócio;
- Cálculos;
- Classificações;
- Contratos;
- DTOs/records;
- Interfaces;
- **Nenhuma dependência externa** (ADR-005).

**Dashboard.Data**
- Acesso ao banco novo;
- Consultas;
- Joins;
- Filtros técnicos;
- Agregações;
- Otimização de acesso aos dados.

### Não criar

- Application layer;
- CQRS;
- Mediator;
- Camadas adicionais sem decisão humana.

---

## 3. Fonte de dados (decisão arquitetural — ADR-008)

**O banco do novo SisacHTML5 é a ÚNICA fonte operacional do Dashboard.**

O sistema legado/VCL e seu banco:

- **NÃO** serão consultados em runtime;
- **NÃO** são contrato de dados;
- servem apenas como **fonte de conhecimento**;
- servem para **engenharia reversa**;
- servem como **referência histórica** durante a construção.

O histórico de **2025 e anteriores** existente no **novo** banco será utilizado para **homologação e demonstração**.

---

## 4. Estado da documentação

Documentos existentes/revisados (criados ou atualizados na gate documental de 2026-09-16):

| Documento | Estado |
|---|---|
| `docs/architecture/adrs/ADR-008-banco-sisac-html5-fonte-unica-operacional.md` | **Criado** |
| `docs/prds/PRD-001-dashboard-indicadores.md` | **Reformulado** |
| `docs/plans/PLAN-001-dashboard-indicadores.md` | **Reformulado** |
| `docs/architecture/proposta-arquitetural.md` | **Revisado** |
| `docs/architecture/dicionario-de-dados.md` | **Reposicionado** |
| `docs/architecture/adrs/ADR-007-regras-de-negocio-em-core.md` | **Válido** (preservado) |

Mudanças de ADR:
- **ADR-008 criado** — banco do novo SisacHTML5 = única fonte operacional;
- **ADR-003 revisada** — alvo de leitura passa a ser o banco novo;
- **ADR-004 revisada** — contexto passa a ser o banco novo; consultas históricas pesadas exigem tratamento controlado de lentidão;
- **ADR-006 supersedida parcialmente** por ADR-008 (acesso ao legado eliminado; "legado = referência de conhecimento" permanece);
- **ADR-007 permanece válida** — regras de negócio residem no Dashboard.Core;
- **PRD-001 reformulado** — MVP 6 indicadores; RNs/CAs reclassificados (arquivados: RN-17..25, CA-06..08).
- **PLAN-001 reformulado** — tarefas T-01..T-21; Ocupação/Glosas/Convênios eliminadas; sem tarefa de Repasses; T-03 = contrato de dados do SisacHTML5.
- **Dicionário reposicionado** — legado tratado como **evidência, não contrato**; seção §14 com domínios conceituais (sem nomes físicos inventados).

---

## 5. Escopo conceitual

**7 indicadores conceituais:**
1. Atendimentos
2. Consultas
3. Exames
4. Faturamento
5. Produção Médica
6. Despesas
7. Repasses

**MVP implementável inicial (6):**
1. Atendimentos
2. Consultas
3. Exames
4. Faturamento
5. Produção Médica
6. Despesas

**Repasses** permanece como indicador previsto no produto, porém **FORA da implementação inicial**, pois depende da definição específica da regra de cada serviço de saúde (RN-40).

**Também fora do MVP:**
- Unidade;
- Internações;
- Cirurgias;
- Ocupação;
- Glosas;
- Convênios como indicador independente;
- outros indicadores futuros.

---

## 6. Decisões de negócio já tomadas

### ATENDIMENTOS / CONSULTAS / EXAMES

- Permitir análise por:
  - **Particular**;
  - **Convênios cadastrados**;
  - **SUS**, caso o serviço de saúde atenda SUS (RN-26).
- Permitir:
  - contagens independentes;
  - contagens acumuladas.
- Visualização poderá utilizar **linhas**, **colunas** e **pizza**;
- O usuário escolhe a forma de visualização **quando semanticamente adequada** (RN-27);
- Relação entre Atendimento/Consulta/Exame é definida pelo **modelo do novo banco** — não presumida (RN-30).

### FATURAMENTO

- Deve distinguir **contas faturadas** e **contas a faturar** (RN-31);
- Deve permitir análise de **períodos anteriores** e **período atual** (RN-32);
- Base = guia/conta de atendimento, **não** recebimento (RN-12);
- Estados/emissão seguem o modelo do novo banco (RN-33).

### PRODUÇÃO MÉDICA

- Deve permitir:
  - produtividade por **profissional ativo** (RN-34);
  - produtividade **individual/profissional** por Particular/Convênio/SUS (RN-35);
  - produtividade **do serviço de saúde** por Particular/Convênio/SUS (RN-36).

### DESPESAS

- Deve permitir:
  - **despesas fixas** do período (RN-37);
  - **despesas variáveis** do período (RN-38);
  - **comparação gráfica** entre fixas e variáveis (RN-39).

### PERÍODO

- Deve permitir:
  - **data atual**;
  - **período futuro** com data inicial/final;
  - **período passado** com data inicial/final (RN-04, RN-42);
- Consultas históricas potencialmente lentas ou inviáveis terão **tratamento e comunicação adequada** ao usuário (RN-43).

### UNIDADE

- **Fora do MVP** e futura (RN-45) — não copiar hierarquia LOCAL do legado.

### REPASSES

- **Fora da implementação inicial** e futura (RN-40);
- **Não presumir qualquer fórmula** entre Faturamento, Despesas e Repasses (RN-41).

---

## 7. Decisões ainda NÃO definidas

As definições abaixo permanecem em aberto (serão tratadas em T-02 / T-03):

**Faturamento**
- Estados da conta que representam "faturado";
- Definição de "emissão";
- Regra de contas a faturar;
- Data de referência.

**Produção Médica**
- Fórmula/métrica exata de produtividade;
- Definição de "profissional ativo";
- Papel profissional considerado;
- Fonte de especialidade.

**Despesas**
- Definição de "fixa";
- Definição de "variável";
- Categorias;
- Data de referência;
- Competência versus pagamento;
- Origem no novo banco.

**Cobertura**
- Modelagem definitiva de Particular/Convênio/SUS no novo SisacHTML5.

**Período**
- Regras detalhadas de granularidade e comportamento.

> Regra transversal: **não presumir respostas**. O legado é evidência — não especificação a copiar (ADR-008, ADR-007).

---

## 8. Estado atual do SDD

- A arquitetura foi **aprovada**;
- O **PRD e PLAN foram reformulados documentalmente**;
- A **Gate de atualização documental foi concluída**;
- **Nenhum código foi implementado**;
- **Nenhuma query foi criada**;
- **Nenhum banco/schema foi alterado** (as descobertas do legado foram somente leitura);
- **Nenhum arquivo do legado foi alterado**;
- **Não houve commit/push após a última Gate documental** — conforme estado atual do Git: `docs/`, `AGENTS.md`, `.opencode/` e `opencode.json` estão **untracked** (nenhuma documentação commitada); `Dashboard.Web/bin` e `Dashboard.Web/obj` aparecem modificados por build local anterior (artefatos de compilação, sem relevância para as entregas).

---

## 9. Próximo passo exato

> ## PRÓXIMA TAREFA:
> **T-02 — validação/alinhamento das regras de negócio.**

- Antes de qualquer implementação, o **T-02** deve transformar as **questões de negócio ainda abertas** (seção 7) em uma **Gate objetiva** para validação humana;
- O próximo trabalho **NÃO** é criar código;
- O próximo trabalho **NÃO** é criar queries;
- O próximo trabalho **NÃO** é investigar novamente todo o legado;
- A próxima etapa é **validar as regras de negócio pendentes** e somente depois avançar para o **contrato de dados do novo SisacHTML5** (T-03).

---

## 10. Instrução para a nova sessão

> **AO RETOMAR ESTE PROJETO:**
>
> 1. Ler este checkpoint integralmente.
> 2. Ler AGENTS.md.
> 3. Ler ADRs relevantes (`docs/architecture/adrs/` — ADR-001 a ADR-008).
> 4. Ler PRD-001.
> 5. Ler PLAN-001.
> 6. Verificar o estado atual do Git.
> 7. Não assumir que a sessão anterior foi concluída além do que estiver documentado.
> 8. Não implementar nada automaticamente.
> 9. Apresentar o estado recuperado e aguardar instrução humana.