# Handoff — Fechamento da Vertical Slice Visual do Dashboard (22/09/2026)

> **Data:** 2026-09-22
> **Modo:** MODO ENGENHEIRO — FECHAMENTO DA SESSÃO (vertical slice visual)
> **Tema:** Primeira vertical slice visual do DashboardSB — painel com 7 indicadores demonstrativos.
> **Escopo:** alterações exclusivas em `Dashboard.Web/**`; nada de banco, login, `user_token` ou SisacHTML5.

---

## 1. Estado da vertical slice

A primeira vertical slice visual de `Dashboard.Web` foi implementada, validada e **aprovada para preservação** pela validação humana. Nenhum ajuste visual deve ser feito nesta sessão.

## 2. Arquivos alterados

| Arquivo | Tipo | Descrição |
|---|---|---|
| `Dashboard.Web/Dados/IndicadoresDemonstracao.cs` | novo | 7 indicadores demonstrativos, determinísticos, ancorados na data atual (séries de 400 dias) |
| `Dashboard.Web/Pages/Shared/_MenuLateral.cshtml` | novo | Partial do menu lateral (Dashboard + 7 indicadores) |
| `Dashboard.Web/wwwroot/js/dashboard.js` | novo | Renderização SVG (coluna/linha), agrupamento dia/semana/mês, filtro de período, seleção individual, maximização em modal |
| `Dashboard.Web/Pages/Shared/_Layout.cshtml` | editado | Sidebar responsiva (collapse no mobile, fixa no desktop) + carga de `dashboard.js` |
| `Dashboard.Web/Pages/Index.cshtml` | editado | Grid de 7 cards com gráficos ~200×200, filtros, seleção, modal maximizado |
| `Dashboard.Web/Pages/Index.cshtml.cs` | editado | Alimenta o modelo e o JSON (camelCase) embutido na página |
| `Dashboard.Web/wwwroot/css/site.css` | editado | Estilos do layout, cards, gráficos e destaque de seleção |
| `opencode.json` | editado | Permissão de edição `Dashboard.Web/**: allow` ativa |

## 3. Validações automatizadas

- `dotnet build Dashboard.slnx` — **0 avisos, 0 erros**
- Aplicação iniciou normalmente (ASPNETCORE_URLS `http://localhost:5140`)
- `GET /` — **HTTP 200**, `text/html`, stderr vazio
- `GET /js/dashboard.js` — **HTTP 200**
- JSON embutido (`#dados-indicadores`) validado via `ConvertFrom-Json`:
  - **7 indicadores** presentes (Atendimentos, Consultas, Exames, Faturamento, Produção Médica, Despesas, Repasses Médicos)
  - **400 pontos/dia** por indicador
  - Referência dos dados: **22/09/2026**
  - Valores do mês corrente calculados (ex.: Atendimentos 1.093; Faturamento R$ 199.885)
- **Sem acesso ao banco, sem login, sem `user_token`, sem integração com SisacHTML5**

## 4. Validação humana

- Menu lateral: OK
- Seleção individual dos indicadores: OK
- Filtro de período (Data Inicial / Data Final): OK
- Maximização do gráfico (modal) e retorno ao Dashboard: OK
- Responsividade desktop/tablet/mobile: OK

**Decisão:** a tela atual está **aprovada para preservação**. Não fazer ajustes visuais nesta sessão.

## 5. Decisões tomadas

- Gráficos nativos em **SVG** (sem biblioteca externa) — colunas/linha no card, agrupamento por período.
- Filtro padrão = **mês corrente** (semântica RN-04 aplicada à camada visual demonstrativa).
- Cards menores e resumidos; análise detalhada na **visualização maximizada**.
- **Repasses Médicos** aparece apenas como card demonstrativo — **sem regra de negócio** (RN-40 não implementada; nenhum cálculo/relação).
- Dados exclusivamente **demonstrativos determinísticos** — nenhuma query, nenhum repository na slice.

## 6. Evidências da tela VCL

Foi analisada a tela do **Gerenciador de Relatórios do SghProg** referente a **"Movimento de Entradas"**, com filtros/dimensões:

- Convênio, Plano, Médico, Especialidade, Local, Tipo, Origem do Paciente, Município, Paciente, Usuário, Grupo de Processo

**Leitura da evidência:**
- Convênio, Médico, Especialidade, Local e Tipo são **dimensões utilizadas pelo legado** para Movimento de Entradas.
- **Sexo NÃO foi comprovado por essa tela** — não há evidência de Sexo como dimensão.
- **Não transformar essa evidência em regra de negócio automaticamente.** É apenas evidência de que o legado usa essas dimensões.

## 7. Próximas etapas (não implementar agora)

Evolução dos indicadores demonstrativos para **indicadores analíticos**, começando por **ATENDIMENTOS**, com dimensões priorizadas:

1. Atendimento por **Convênio**
2. Atendimento por **Sexo**

Visualização prevista: escolha entre **Colunas** e **Donut**. Card resumido no Dashboard + análise detalhada na visualização maximizada.

Dimensões futuras (identificadas, **não** implementar): Médico, Especialidade, Tipo, Local.

**Antes da implementação**, comprovar no banco:
- origem do Convênio;
- origem do Sexo;
- relacionamento com Médico;
- relacionamento com Especialidade;
- significado dos códigos de Tipo.

## 8. Princípio para a próxima sessão

```
evidência → causa/regra → decisão → execução → validação → fechamento
```

Não implementar dimensões por plausibilidade. Primeiro comprovar dados e regras, depois decidir a visualização, só então implementar.

## 9. O que NÃO deve ser reaberto

- Investigação de `user_token` / `cod_fi` / `Home/PaginaPrincipal` / servidor `:8022`
- Investigação de `SghProg` / `SisacHTML5` em geral
- Fontes `SghProg`/`SisacHTML5` como contrato de dados — permanece ADR-008 (banco do novo SisacHTML5 como única fonte operacional)
- Vertical slice visual já implementada (aprovada; sem ajustes)

## 10. Pendências para amanhã

- Comprovar no banco as origens/relacionamentos de Convênio e Sexo (e demais dimensões de Atendimentos)
- Validar significado dos códigos de Tipo
- Especificar visualização Colunas/Donut para Atendimento por Convênio e por Sexo
- Evoluir a slice demonstrativa para dados analíticos de Atendimentos
- Revisar commits não publicados de trabalhos anteriores (T-07/T-08 e handoffs de investigação de identidade) que permanecem fora deste commit

---

## Registro da sessão

- **Arquivos alterados nesta etapa:** somente `Dashboard.Web/**` + `opencode.json` + este handoff.
- **Arquivos deliberadamente fora deste commit:** trabalhos anteriores do working tree (T-07/T-08, dicionário/PLAN, ADR-009, handoffs de investigação de identidade, artefatos `bin`/`obj`).
- **Nenhum push.**