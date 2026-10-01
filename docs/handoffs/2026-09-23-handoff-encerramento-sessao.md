# Handoff — 2026-09-23 (Encerramento da sessão — contexto e retomada)

> **Data:** 2026-09-23
> **Modo:** MODO ENGENHEIRO — FECHAMENTO DA SESSÃO
> **Propósito:** registrar o estado atual para retomada amanhã, **sem** novas investigações, **sem** alterar código e **sem** executar SQL nesta sessão.

---

## 1. Objetivo do handoff

Registrar o estado atual do projeto DashboardSB para retomada. **Nenhuma ação adicional deve ser executada agora — o handoff apenas registra contexto de retomada e encerra a sessão.**

## 2. Contexto principal

Construção do **Dashboard de ATENDIMENTOS** em C#/.NET 10 + Razor Pages.

**Conclusão central de hoje:** **ATENDIMENTOS é a medida analítica central.** A partir dela analisam-se diferentes dimensões:

- Data
- Hora
- Dia da semana
- Tipo
- Convênio/Cobertura
- Sexo
- Médico
- Usuário/Recepcionista

**Evidência funcional fornecida pelo usuário:** o Power BI utiliza uma medida de Atendimentos e permite agrupá-la por `UsuarioRegistro`, `Convênio`, `Sexo`, etc.

**Consulta SQL operacional apresentada, que demonstra:**
- `ENTRADA` como origem dos atendimentos;
- `COUNT(*)` como medida;
- `E.Recep` como Recepcionista;
- `CADCONVENIO` para Convênio;
- `E.Tipo` para TipoAtendimento;
- `CADMEDICO` para Médico;
- filtros de período, `GrupoEmp`, `Filial`, `Fechado` e `LoteEnt`;
- agrupamento por Recepcionista, Convênio, Tipo e Médico.

## 3. Decisão de engenharia

**Não** continuar criando investigações abstratas nem ficar em loop de documentação. A próxima etapa será **orientada à construção**:

`SISACHTML5` → código-fonte/rotina real → SQL/procedure real → regra de negócio → medida e dimensões → contrato C# → Repository → Razor Page → JSON → gráfico.

## 4. Próxima tarefa (retomada — investigação somente leitura)

Investigar no **código-fonte ATUAL do SisacHTML5** a origem real do relatório/consulta de atendimentos, identificando:

1. tela/rotina responsável;
2. SQL ou procedure utilizada;
3. parâmetros;
4. filtros;
5. campo que representa o usuário/recepcionista;
6. regra que define atendimento;
7. dimensões disponíveis;
8. estrutura do resultado.

## 5. Restrições (valem para a retomada)

- somente investigação;
- não alterar código;
- não alterar banco;
- não executar SQL no CASAMATER nesta etapa (ver item 7);
- excluir explicitamente qualquer diretório/arquivo cujo nome contenha o prefixo **"old"**;
- não investigar `old_SghProg`, `OLD_SisacHTML5` ou equivalentes;
- não iniciar novas frentes paralelas;
- não criar abstrações nem implementar o Dashboard ainda.

## 6. Estado das frentes e artefatos

- **T-ATD-02** — encerrado (handoff: `docs/handoffs/2026-09-23-t-atd-02-definicao-analitica-atendimento.md`).
- **Investigação Convênio** — encerrada (handoff: `docs/handoffs/2026-09-23-encerra-frente-investigacao-atendimento-convenio.md`).
- **Investigação Sexo** — encerrada (handoff: `docs/handoffs/2026-09-23-encerra-frente-investigacao-atendimento-sexo.md`).
- **PRD-002** — validado; não alterar sem autorização.
- **ADR-010** — validado; não alterar sem autorização.
- **T-ATD-01** — entregue como matriz de evidências no terminal; medição ativa de `ENTRADA` (janela 2024) registrada como **não comprovada por limitação de performance** nesta janela.
- Artefatos tocar hoje: `ADR-010-...md` (criado), `PRD-002-...md` (ajustado cirurgicamente — cobertura × convênio individual) e handoff T-ATD-02 (criado).

## 7. Próximo passo (amanhã)

**Primeiro** verificar o estado do SQL Server/CASAMATER após o trabalho de recuperação que ficará executando durante a noite. **Somente depois** disso retomar a investigação necessária.

## 8. Observação

Este handoff é **de contexto e retomada**. Nenhuma ação adicional foi executada além da gravação deste documento. Sessão encerrada.