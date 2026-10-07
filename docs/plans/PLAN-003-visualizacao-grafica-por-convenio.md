# PLAN-003 — Visualização Gráfica por Convênio

> **Tipo:** plano de execução — registro retroativo
> **Data de criação:** 2026-10-07
> **Natureza:** documento retroativo fiel ao executado na sessão de 07/10/2026.
> Nenhuma funcionalidade foi inventada após a fato; cada tarefa corresponde a
> entrega realmente implementada, validada e commitada.
> **Nomenclatura:** IDs `P3-TXX` próprios deste plano, para evitar colisão com
> as tarefas `T-XX` do PLAN-002 (Análise por Convênio) e do PLAN-001.

## Objetivo

Apresentar a análise nominal por convênio — já entregue pelo PLAN-002 na forma
de tabela — na forma de **gráfico de barras horizontal** nos três indicadores
assistenciais (Atendimentos, Consultas e Exames), com padrão visual e
estrutural idêntico entre eles e comportamento responsivo preservado.

## Referências

- PLAN-002 — Análise por Convênio (encerrado em 2026-10-06) — fornece a
  infraestrutura nominal consumida por este ciclo (seção, tabela, estados,
  repositórios)
- `Dashboard.Web/Pages/Atendimentos.cshtml` — página de referência do padrão
  aprovado
- PRD-003 / SPEC-UI-003 / DEC-003-01..06 — contexto do ciclo anterior
- AGENTS.md — convenções e restrições permanentes

## Escopo executado

- Gráfico horizontal SVG nativo antes da tabela, nos 3 indicadores
- Padrão único compartilhado (mesmo markup em Atendimentos, Consultas e Exames)
- Correção responsiva compartilhada em `site.css`
- Validação global técnica do ciclo (build + testes)

## Decisões reais do ciclo

- Barras horizontais com nome à esquerda, barra proporcional ao Volume, valor à direita
- SVG nativo (viewBox `0 0 860`, 24 un por item, área de nome 234 un, barra máx. 470 un) — **sem Chart.js, sem nova biblioteca**
- Gráfico posicionado **antes** da tabela nominal
- **Todos os convênios preservados** — sem Top N, sem "Outros", sem agregação adicional
- Ordem recebida do repositório preservada (já validada no PLAN-002)
- Truncamento visual do rótulo em 26 caracteres no SVG; **nome completo preservado na tabela**
- Scroll vertical da seção preservado (`.convenio-rolagem`, `max-height: 20rem`)
- `volumeMaximo = 0` tratado sem divisão por zero
- Responsividade: `.convenio-rolagem svg { min-width: 40rem; }` — impede escala ilegível em viewport estreito, com rolagem horizontal restrita à seção
- Atendimentos, Consultas e Exames com padrão estrutural consistente (verificado por diff: apenas título e aria-label diferem)

## Fora do escopo (não executadas neste ciclo)

- Top 10 + "Demais convênios" — **não implementado**
- Comparativo Consultas × Exames — **não implementado**
- Redesign / mudança de identidade visual — **não implementado**

## Tarefas

### P3-T01 — Preparação/definição do padrão gráfico por convênio
- **Status:** Concluído (2026-10-07)
- **Depende de:** Nenhuma
- **Evidência:** diagnóstico cirúrgico da estrutura existente (seção, tabela, `Model.ConvenioNominal`, Identificacao + Volume) e definição do padrão SVG a replicar a partir de Atendimentos; regra de negócio confirmada preservada (TIPO `'1'`/`'3'`, RN-07, período, exclusão de suspensos)

- [x] Confirmar checkpoint de repositório limpo
- [x] Confirmar infraestrutura nominal completa nas 3 páginas
- [x] Definir padrão visual alvo (título, aria-label, mesmas proporções)

### P3-T02 — Atendimentos por Convênio
- **Status:** Concluído (2026-10-07)
- **Depende de:** P3-T01
- **Evidência:** commit `0038c54`; validação visual humana concluída; build 0 erros / 0 avisos; 72/72 testes

- [x] Bloco SVG horizontal inserido antes da tabela em `Atendimentos.cshtml`
- [x] Título "Atendimentos por Convênio" e aria-label correspondente
- [x] Tabela, scroll e estados preservados

### P3-T03 — Consultas por Convênio
- **Status:** Concluído (2026-10-07)
- **Depende de:** P3-T02 (padrão aprovado como referência)
- **Evidência:** commit `6619782`; validação visual humana concluída; build 0 erros / 0 avisos; 72/72 testes

- [x] Mesmo bloco SVG replicado em `Consultas.cshtml` (single-file)
- [x] Título "Consultas por Convênio" e aria-label correspondente
- [x] Regra `TIPO = '1'` e demais filtros intactos (nenhum arquivo `.cs` alterado)

### P3-T04 — Exames por Convênio
- **Status:** Concluído (2026-10-07)
- **Depende de:** P3-T03 (padrão aprovado como referência)
- **Evidência:** commit `2c62fae`; validação visual humana concluída; build 0 erros / 0 avisos; 72/72 testes

- [x] Mesmo bloco SVG replicado em `Exames.cshtml` (single-file)
- [x] Título "Exames por Convênio" e aria-label correspondente
- [x] Regra `TIPO = '3'` e demais filtros intactos (nenhum arquivo `.cs` alterado)

### P3-T05 — Consistência/responsividade e validação global
- **Status:** Concluído (2026-10-07)
- **Depende de:** P3-T02, P3-T03, P3-T04
- **Evidência:** commit `e9a3917`; comparação estrutural 10/10 entre as 3 páginas (divergência apenas de título/aria-label); correção responsiva `min-width: 40rem` em `site.css` validada via DevTools mobile pelo humano; build Release 0 erros / 0 avisos; 72/72 testes; validação global T-06 sem bloqueador técnico

- [x] Consistência estrutural confirmada entre Atendimentos, Consultas e Exames
- [x] Regra responsiva compartilhada aplicada em `site.css` (arquivo único)
- [x] Overflow horizontal restrito à seção, sem overflow de página
- [x] Validação global: repositório limpo, build 0/0, testes 72/72
- [x] Regras de negócio reconfirmadas em código (RN-07, período, suspensos, TIPO)

## Histórico de execução

- **2026-10-07** — ciclo executado de ponta a ponta em sessão única:
  P3-T01 (diagnóstico) → P3-T02 (commit `0038c54`) → P3-T03 (commit `6619782`)
  → P3-T04 (commit `2c62fae`) → P3-T05 (commit `e9a3917`) → validação global
  T-06 (build 0/0, 72/72, nenhum bloqueador).
- **2026-10-07** — fechamento documental: criação retroativa deste plano e
  atualização aditiva do `STATUS-002-estado-workflow.md`, sob decisão humana.
