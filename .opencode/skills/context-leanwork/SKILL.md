---
name: context-leanwork
description: Geração e atualização do contexto de projeto para agentes de IA, no padrão Leanwork — arquivos AGENTS.md (raiz e por módulo) e regras de permissão na seção `permission` do opencode.json. Use quando o usuário pedir explicitamente para "gerar AGENTS.md", "criar contexto do projeto", "atualizar AGENTS.md", "configurar permissões do agente", "configurar opencode.json", "documentar convenções do projeto para o agente", "criar AGENTS.md do módulo X" ou variações diretas. Também use quando o usuário aceitar uma sugestão de rodar a skill vinda de outra skill do pipeline SDD. A skill detecta stack e comandos reais inspecionando o repositório, lê os artefatos do pipeline (proposta arquitetural, ADRs, PRDs, planos) para extrair convenções e restrições, e produz AGENTS.md com seções Resumo, Stack, Comandos, Convenções, Restrições e índice de documentação, além de regras de permissão allow/ask/deny calibradas pela stack detectada. NUNCA sobrescreve conteúdo escrito por humanos — faz merge conservador em blocos delimitados e mostra diff antes de gravar. AGENTS.md de módulo é opt-in, nunca gerado em massa sem escolha do usuário. NÃO gera AGENTS.md automaticamente durante outras tarefas — precisa ser invocada explicitamente.
---

# Context Leanwork — Geração de AGENTS.md para Agentes de IA

Esta skill produz e mantém os arquivos `AGENTS.md` que dão contexto de projeto a agentes de IA. É a ponte entre os artefatos do pipeline SDD (que são escritos para humanos) e o que o agente precisa saber em cada sessão sem ler 400 linhas de proposta arquitetural.

## Princípios

- **Nunca automática.** Só roda por pedido direto do usuário ou quando ele aceita uma sugestão — nunca como efeito colateral de outra tarefa. Outras skills do pipeline sugerem; nenhuma gera por conta própria.
- **Nunca destrutiva.** Conteúdo escrito por humano é preservado. Merge acontece apenas dentro de blocos delimitados como auto-gerados. Sempre mostra diff antes de gravar.
- **Baseada em evidência do repositório.** Comandos de build/test vêm de inspeção real (`package.json`, `.csproj`, `Makefile`), não de suposição sobre a stack.
- **Sem duplicação entre raiz e módulo.** Módulo nunca repete stack nem comandos globais. Duplicação vira divergência em poucas semanas.
- **Enxuta.** `AGENTS.md` é lido em toda sessão do agente — cada linha custa contexto. Se a informação vive melhor num doc referenciado, referencie em vez de copiar.
- **Stack-agnóstica.** Como o resto do pipeline, não traz padrões pré-definidos. Lê do projeto.

## Fluxo geral

1. **Determinar o escopo** — raiz, módulo(s), ou ambos
2. **Coletar contexto** — artefatos do pipeline + inspeção do repositório
3. **Detectar drift** — divergência entre o que está documentado e o que o repo mostra
4. **Gerar ou fazer merge** — mostrando diff antes de gravar
5. **Reportar** — o que foi criado, atualizado, e o que ficou pendente

## Fase 1 — Determinar o escopo

Perguntar ao usuário **uma vez**, com opções claras, a menos que ele já tenha especificado:

> O que você quer gerar ou atualizar?
>
> **A.** `AGENTS.md` da raiz do projeto
> **B.** `AGENTS.md` de um ou mais módulos específicos
> **C.** Permissões — `opencode.json` com allow/ask/deny pela stack
> **D.** Raiz + permissões (o pacote completo de contexto do projeto)
> **E.** Auditar o que existe hoje e apontar o que está desatualizado (sem gravar nada)

A opção **E** é modo somente-leitura: útil quando o usuário quer saber se vale a pena rodar antes de mexer em arquivo.

**Por que permissões vivem aqui:** a detecção de stack e de comandos reais de build/teste é exatamente a mesma que alimenta a seção Comandos do `AGENTS.md`. Os comandos detectados ali são os que entram no `allow`. Separar em outro comando faria o mesmo trabalho duas vezes.

**Momento certo:** permissões só ficam boas **depois que existe código** — em repositório vazio não há stack para detectar. Se o usuário pedir permissões num repo sem código, gerar apenas o deny universal e as regras de git, e avisar que vale rodar de novo após o scaffolding.

**Nunca gerar `AGENTS.md` para todos os módulos sem escolha explícita.** Projeto LMA com 8 módulos rende 8 arquivos, e vários deles (módulos triviais de CRUD) só viram ruído. Detecte os módulos, liste, e deixe o usuário escolher.

## Fase 2 — Coletar contexto

### 2.1 Artefatos do pipeline SDD

Ler, quando existirem (ver `.opencode/templates/folder-conventions.md` para os caminhos):

| Artefato | O que extrair |
|----------|---------------|
| `docs/architecture/proposta-arquitetural.md` | Sumário executivo (→ Resumo), Restrições (seção 4), Stack (seção 6.2 Containers), Dívidas conscientes |
| ADRs (inline ou `docs/architecture/adrs/`) | Decisões que viram convenção obrigatória (ex.: "MediatR para todos os comandos") |
| `docs/prds/*.md` | Nomes das features para o índice de documentação; não copiar conteúdo |
| `docs/plans/*.md` | Convenções de código que apareceram nas tarefas; padrões de nomeação de teste |
| `docs/reviews/*.md` | Seção "Notas ao processo" → **padrões emergentes** que ainda não estão no AGENTS.md |

A seção "Padrão emergente" dos reviews é a fonte mais valiosa para atualizações: é exatamente onde o reviewer registrou "isso virou convenção mas não está documentado".

### 2.2 Inspeção do repositório

Para a seção **Comandos**, que não existe em nenhum artefato do pipeline, inspecionar o repositório. Ver `.opencode/skills/context-leanwork/references/command-detection.md` para a tabela completa por ecossistema.

Resumo da abordagem:

1. Procurar arquivos de build/task runner (`Makefile`, `justfile`, `Taskfile.yml`, `package.json` scripts, `*.csproj`/`*.sln`, `docker-compose.yml`, `pyproject.toml`, `Cargo.toml`, `go.mod`)
2. Extrair os comandos **realmente definidos** — não inventar comandos padrão da stack
3. Se um comando esperado não existir (ex.: não há script de teste), registrar como lacuna no relatório, não inventar

Para módulos, detectar a estrutura de pastas conforme o padrão arquitetural declarado. Em LMA, procurar as pastas de módulo conforme a proposta arquitetural define; não assumir convenção fixa.

### 2.3 Lacunas

Quando uma seção não tiver fonte:

- **Nunca inventar.** Marcar com placeholder explícito e reportar ao usuário.
- Formato do placeholder: `<!-- TODO: [o que falta] — sem fonte no repositório ou nos artefatos do pipeline -->`
- Se a lacuna for crítica (ex.: nenhum comando de teste detectado), perguntar ao usuário diretamente antes de gravar

## Fase 2.5 — Permissões (quando o escopo incluir)

Ler `.opencode/skills/context-leanwork/references/permission-catalog.md` e compor o `opencode.json`:

1. **Deny universal** — sempre, sem exceção. `.env`, chaves, certificados, credenciais de cloud, `terraform.tfstate`, tokens de registry, comandos destrutivos.
2. **Receita da stack detectada** na Fase 2 — allow, ask e deny específicos. Os comandos de build e teste já extraídos do repositório entram no `allow`.
3. **Regras de git** — transversais.
4. **Docker e infraestrutura** — apenas se detectados.

**Sintaxe:** as regras usam o formato `"padrão": "ação"` nativo do OpenCode — `"dotnet build*": "allow"`, não `Tool(especificador)`. A precedência é **last-match-wins**: a última regra que casar com a entrada decide. Regras mais específicas devem vir **depois** da regra genérica `"*": "ask"`.

**Ordem correta para `bash`:** `"*": "ask"` primeiro, regras específicas (allow, deny) depois. Com last-match-wins, as específicas no final vencem a genérica no início.

**Ordem correta para `read`:** `"**": "allow"` primeiro, deny de segredos depois. As regras de deny vencem por last-match-wins.

Duas decisões dividem opinião entre times e valem perguntar:

> **1.** `git commit*` — em `allow` (o agente commita direto, é local e reversível) ou em `ask`?
> **2.** Migrations — em `ask` (você confirma cada uma) ou `deny` (só na mão)?

Não perguntar mais do que isso. O resto do catálogo é calibragem defensável por padrão.

### Onde gravar

**Sempre em `opencode.json`** — política do time, versionada, viaja com o repositório. Nunca em `openrc.json`, que é território pessoal de cada dev.

### Merge com arquivo existente

Mesma regra do `AGENTS.md`: nunca destrutivo.

- Manter todas as regras existentes
- Adicionar apenas as que faltam
- **Nunca remover** regra que o usuário colocou
- Se uma regra existente está em balde diferente do sugerido (ex.: `git push*` em `allow` quando o catálogo sugere `ask`), **não mexer** — apontar a divergência no relatório e deixar a decisão com ele
- Mostrar o diff antes de gravar

Ao adicionar regras, manter a ordem: regra genérica `"*"` no início do objeto, específicas depois. Com last-match-wins, a ordem inversa (específica antes da genérica) faz a genérica vencer — comportamento oposto ao pretendido.

Como `opencode.json` é JSON e não aceita comentário delimitador, o merge é por comparação de listas de chaves, não por marcador. Regra que já existe não é duplicada.

### Verificar o `.gitignore`

Confirmar que `openrc.json` está ignorado. Se não estiver, mostrar a linha para o usuário adicionar — sem editar o `.gitignore` por conta própria.

## Fase 3 — Detectar drift

Quando já existe um `AGENTS.md`, comparar o conteúdo dele com a realidade coletada na Fase 2 e reportar divergências antes de propor mudança:

| Tipo de drift | Exemplo | Severidade |
|---------------|---------|------------|
| Stack divergente | AGENTS.md diz SQL Server, `.csproj` referencia Npgsql | Alta — agente vai gerar código errado |
| Comando quebrado | AGENTS.md manda `npm test`, `package.json` não tem script `test` | Alta — agente vai falhar ao validar |
| Convenção contrariada por ADR novo | AGENTS.md diz "repositórios expõem IQueryable", ADR-009 proibiu | Alta |
| Link morto | Índice aponta para `docs/prds/PRD-003.md` que não existe | Média |
| Padrão emergente não documentado | Reviews citam 3× um padrão que não está no AGENTS.md | Média |
| Módulo novo sem AGENTS.md | Pasta de módulo criada após a última execução | Baixa |
| Permissão para comando inexistente | `allow` tem `"npm run test*"` mas o script foi removido | Baixa |
| Stack nova sem permissões | Projeto ganhou frontend, `opencode.json` só cobre backend | Média |
| Segredo sem deny | Apareceu `terraform.tfstate` ou `.npmrc` no repo, sem regra de bloqueio | **Alta** |

Apresentar o drift ao usuário **antes** de propor a escrita. Em modo auditoria (opção D da Fase 1), o relatório de drift é o output final — não grava nada.

## Fase 4 — Gerar ou fazer merge

### Arquivo novo

Usar o template correspondente:

- `.opencode/skills/context-leanwork/references/agents-md-root-template.md` para a raiz
- `.opencode/skills/context-leanwork/references/agents-md-module-template.md` para módulos

Mostrar o conteúdo proposto e pedir confirmação antes de gravar.

### Arquivo existente — merge conservador

O `AGENTS.md` da raiz gerado por esta skill delimita as seções auto-geradas com marcadores HTML:

```markdown
<!-- leanwork-context:start -->
## Stack

- Backend: .NET 8 / ASP.NET Core / EF Core 8
- Banco: PostgreSQL 16
<!-- leanwork-context:end -->
```

Regras de merge:

1. **Dentro dos marcadores:** conteúdo pode ser atualizado pela skill
2. **Fora dos marcadores:** conteúdo é do humano, **nunca** modificado nem removido
3. **Arquivo sem marcadores** (escrito 100% à mão antes desta skill existir): **não sobrescrever**. Propor ao usuário: (a) mostrar diff sugerido para ele aplicar manualmente, (b) adicionar os marcadores em volta das seções que ele indicar, ou (c) criar seção nova no final com o que falta. Nunca escolher por ele.
4. **Sempre mostrar o diff** antes de gravar — mesmo quando o merge é trivial

### Ordem das seções

Manter a ordem canônica (Resumo, Stack, Comandos, Convenções, Restrições, Documentação) para consistência entre projetos. Se o arquivo existente usa outra ordem escrita por humano, **preservar a ordem dele** — consistência interna do projeto ganha da convenção do plugin.

## Fase 5 — Reportar

Ao final, mostrar ao usuário:

- Arquivos criados / atualizados / inalterados (`AGENTS.md` e/ou `opencode.json`)
- Drift detectado e resolvido
- Lacunas que ficaram como TODO e por quê
- Módulos detectados que **não** receberam AGENTS.md (com o motivo: usuário não selecionou)
- Sugestão de próximo passo, quando aplicável

## Divisão de conteúdo — raiz vs módulo

Regra que evita duplicação e divergência:

| Seção | Raiz | Módulo |
|-------|------|--------|
| Resumo | Produto inteiro, 2-4 linhas | Responsabilidade do módulo, 1-2 linhas |
| Stack | Completa | **Nunca** — herda da raiz |
| Comandos | Globais (build, test, run, migrations) | Apenas os específicos do módulo, se existirem |
| Convenções | Gerais do projeto | Apenas as que **divergem** do padrão da raiz |
| Restrições | Do projeto | Dependências permitidas / proibidas (boundaries) |
| Documentação | Índice completo do `docs/` | ADRs e PRDs que afetam este módulo |
| Domínio | — | Entidades e agregados principais do módulo |

Se o `AGENTS.md` de um módulo ficar com menos de ~15 linhas úteis depois de aplicar essa divisão, provavelmente ele não se justifica. Avisar o usuário e sugerir não criar.

## O que NÃO incluir no AGENTS.md

- **Conteúdo copiado dos artefatos do pipeline.** Referencie o caminho; não duplique regras de negócio, ADRs completos ou critérios de aceite.
- **Histórico ou changelog.** Isso é papel do git e do `PROGRESS`/histórico de execução do plano.
- **Instruções genéricas de boa prática** ("escreva código limpo", "use nomes descritivos"). O agente já sabe; só ocupa contexto.
- **Permissões afrouxadas a pedido.** Se o usuário pedir para remover um item do deny universal, explicar o risco específico antes. `terraform.tfstate` e `.npmrc` guardam segredo em texto puro e são os mais esquecidos.
- **Credenciais, connection strings, tokens.** Nunca, em nenhuma circunstância. Se encontrar algum no arquivo existente, **alertar o usuário** em vez de propagar.
- **Detalhes que mudam toda semana.** Se a informação tem meia-vida curta, ela pertence a um doc referenciado, não ao AGENTS.md.

## Quando sugerir que o usuário rode esta skill

Esta skill nunca roda como efeito colateral de outra tarefa — exige pedido direto ou aceite de sugestão. As outras skills e comandos do pipeline devem sugeri-la nestes momentos:

| Momento | Quem sugere |
|---------|-------------|
| Proposta arquitetural concluída | `architect-leanwork` (checklist final) |
| Review detectou padrão emergente | `reviewer-leanwork` (Notas ao processo) |
| Review rodou em modo degradado por falta de AGENTS.md | `reviewer-leanwork` |
| Projeto tem artefatos do pipeline mas nenhum AGENTS.md | `/leanwork-next` |
| Módulo novo apareceu no repositório | `/leanwork-next` |
| Projeto tem código mas nenhum `opencode.json` | `/leanwork-next` |
| Primeiro scaffolding concluído (comandos de build passam a existir) | `/leanwork-next` |

A sugestão é sempre **um convite, não uma etapa obrigatória**. Se o usuário ignorar, o pipeline segue normalmente.

## Recursos auxiliares

- `.opencode/skills/context-leanwork/references/agents-md-root-template.md` — template do AGENTS.md da raiz
- `.opencode/skills/context-leanwork/references/agents-md-module-template.md` — template do AGENTS.md de módulo
- `.opencode/skills/context-leanwork/references/command-detection.md` — como detectar comandos reais por ecossistema
- `.opencode/skills/context-leanwork/references/permission-catalog.md` — receitas de `allow`/`ask`/`deny` por ecossistema, com deny universal de segredos
