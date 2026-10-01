# Handoff — Encerramento da Frente de Investigação do Indicador Atendimentos por Sexo (23/09/2026)

> **Data:** 2026-09-23
> **Modo:** MODO ENGENHEIRO — FECHAMENTO DA SESSÃO (investigação somente leitura)
> **Tema:** Indicador **ATENDIMENTOS** — frente "Atendimentos por Sexo".
> **Escopo:** consultas somente leitura em `CASAMATER` (entidade `dashboard_readonly`). Nenhum código, banco, permissão ou documento foi alterado.

---

## 1. Estado da frente

Frente **encerrada por decisão humana** antes da coleta de distribuição/cobertura. Retomar em janela de menor carga ou após avaliação do DBA.

## 2. Registro da investigação

1. **Origem funcional de SEXO confirmada com o suporte:** `dbo.CADPACIENTE.SEXO`.
2. **Evidência técnica preliminar já existente:** `CADPACIENTE` possui `CODPACIENTE` e `SEXO`; aproximadamente **314 mil registros**; `CODPACIENTE` identificado anteriormente **sem duplicidade** (`U_CADPACIENTE`).
3. **Q1 (metadados/catálogo — `sys.columns`/`sys.indexes`/`sys.partitions`) autorizada mas excedeu 90s sem retorno.**
4. **Teste mínimo `SELECT DB_NAME(), GETDATE()` respondeu imediatamente**, confirmando conectividade com `CASAMATER`.
5. Portanto, a lentidão observada é **intermitente** e **não foi explicada por falha de login/rede**.
6. **Nenhuma consulta adicional foi executada após o teste de conectividade.**
7. **Nenhum dado, objeto, permissão ou código foi alterado.**
8. **Pendências:** distribuição real de `CADPACIENTE.SEXO` e cobertura `ENTRADA → CADPACIENTE` **não medidas**.
9. A investigação deverá ser retomada em **outra janela de menor carga** ou após **avaliação do DBA**.

## 3. Vedações registradas (decisão humana desta sessão)

- NÃO executar Q2, Q3, Q4, Q5 ou Q6 nesta janela.
- NÃO reexecutar Q1.
- NÃO aumentar timeout; NÃO contornar a lentidão do servidor.
- NÃO alterar permissões, banco, código.
- NÃO executar consultas adicionais de diagnóstico.

## 4. Próxima ação

Definida por **decisão humana** (nova janela ou avaliação do DBA). Nada pendente deste lado.