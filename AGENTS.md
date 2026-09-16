# DashboardSB — Instruções para agentes de IA

<!-- leanwork-context:start -->

## Resumo

Repositório de testes do pipeline SDD Leanwork (porte para OpenCode). Projeto ASP.NET Core Razor Pages mínimo (net10.0), usado como scaffold para validar estrutura de diretórios, convenções e resolução de paths entre skills, templates e referências.

## Stack

- **Backend:** ASP.NET Core (Razor Pages) — .NET 10.0
- **Frontend:** Razor Pages (renderização server-side)
- **Banco:** nenhum projeto de dados configurado neste repositório
- **Infra:** local (dotnet CLI)
- **Testes:** nenhum projeto de testes presente

## Comandos

```bash
# Build
dotnet build Dashboard.slnx

# Rodar local
dotnet run --project Dashboard.Web

# Testes
<!-- TODO: nenhum comando de teste detectado no repositório -->
```

## Convenções

- Arquitetura do pipeline SDD com IDs `ADR-XX`, `RN-XX`, `CA-XX`, `UI-XX`, `T-XX`, `R-XX`
- Testes de cenário nomeados `CA_XX_descricao_do_cenario` (quando existirem)
- Arquivos do pipeline em `docs/`: `architecture/`, `prds/`, `prototype/`, `plans/`, `reviews/`, `traceability/`
- Validação humana explícita antes de cada entrega

## Restrições

- SQL Server legado é leitura exclusiva (se aplicável a dados do pipeline)
- Não alterar `Dashboard.Web` ou comportamento existente sem necessidade explícita
- Não criar `Dashboard.Core` nem `Dashboard.Data` neste estágio (virá com proposta arquitetural futura)
- Evitar migrations até que o banco seja integrado via camada de dados

## Documentação

- **Arquitetura:** `docs/architecture/proposta-arquitetural.md`
- **ADRs:** `docs/architecture/adrs/`
- **PRDs:** `docs/prds/`
- **Planos:** `docs/plans/`
- **Reviews:** `docs/reviews/`

## Como trabalhar neste projeto

Este projeto usa o pipeline SDD Leanwork. Antes de implementar qualquer feature:

1. Verifique se existe um plano em `docs/plans/PLAN-XXX-*.md`
2. Identifique a próxima tarefa pendente sem bloqueio: `Status: Pendente` e todas as tarefas de `Depende de:` com `Status: Concluído`
3. Leia a tarefa inteira, incluindo os campos `Implementa:`, `Valida:` e `Decisões base:`
4. Abra os artefatos referenciados
5. Respeite os pontos de validação humana marcados no plano
6. Nomeie os testes conforme a convenção `CA_XX_*`
7. Atualize o campo `Status:` e o histórico de execução ao concluir
8. Execute uma tarefa por vez e peça review antes de seguir

<!-- leanwork-context:end -->
