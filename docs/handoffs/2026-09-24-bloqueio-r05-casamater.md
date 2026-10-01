# Handoff — Bloqueio R-05: indisponibilidade do CASAMATER na validação de integração do T-08

> **Data:** 2026-09-24
> **Modo:** MODO ENGENHEIRO — REGISTRO DE BLOQUEIO (diagnóstico somente leitura)
> **Tema:** T-08 — validação de integração bloqueada por falha de conexão com CASAMATER
> **Escopo:** diagnóstico de disponibilidade da conexão; nenhum código, banco, permissão ou documento foi alterado.

---

## 1. Contexto do T-08

Consulta de Atendimentos em `Dashboard.Data` (implementada), review concluído; R-05 = verificação pendente por não execução dos testes de integração. Correção R-02 (JOIN contextual) aplicada; matriz R-01..R-05 consolidada em 24/09/2026.

## 2. Evidência: configuração presente

`ConnectionStrings__SisacDatabase` **presente** no ambiente de execução (valor não exibido). Configuração resolvida pelo T-08 via `SqlConnectionFactory` (mesmo caminho do teste T-05 `factory_abre_conexao_com_banco_sisac_html5`).

## 3. Tentativa de abertura — resultado

Executado somente o teste de fábrica (validação mínima, `SELECT 1`). **Falha:** `Microsoft.Data.SqlClient.SqlException` — tempo limite de conexão expirado.

## 4. Sequência técnica observada

handshake concluído → autenticação concluída → **timeout na fase [Post-Login]** (≈ 14 s).

## 5. Confirmação: nenhum SELECT executado

Falha ocorreu em `SqlConnectionFactory.Create()` (antes da consulta); o `SELECT 1` nunca chegou a ser enviado. Nenhuma consulta exploratória executada.

## 6. Impacto

Testes de integração do T-08 **não podem ser executados** neste ambiente enquanto o CASAMATER permanecer indisponível/contido. R-05 permanece o bloqueio do fechamento do T-08.

## 7. Decisões preservadas

- Não aumentar timeout;
- não alterar permissões;
- não encerrar sessões;
- não alterar índices;
- não executar consultas exploratórias.

## 8. Estado

R-05 = **BLOQUEADO EXTERNAMENTE**.

## 9. Próximos passos

Aguardar disponibilidade/estabilidade do CASAMATER (janela sem contenção / verificação com DBA) e então retomar a validação de integração (CA-03, CA-09, CA-18) até verde.