# Handoff — Encerramento da investigação ativa R-02/R-05 (21/09/2026)

> **Data:** 2026-09-21
> **Escopo:** Encerramento das tentativas de consulta ao CASAMATER nesta sessão; preservação do estado validado e dos próximos passos pendentes
> **Tarefa afetada:** T-08 — Implementar query de Atendimentos em `Dashboard.Data`
> **Arquivos de referência:** `docs/plans/PLAN-001-dashboard-indicadores.md`, `docs/reviews/REVIEW-T-08-2026-09-18.md`, `docs/handoffs/2026-09-18-t08-pausa.md`
> **Decisão humana:** validada nesta data (MODO ENGENHEIRO)

---

## 1. ESTADO GERAL DO T-08

- T-08 **implementado, ainda NÃO concluído/validado**.
- **NÃO foi feito commit da implementação.**
- **NÃO há alterações novas** de código, testes, banco ou PLAN-001 nesta etapa.
- T-09: **não iniciado**.

## 2. R-01 — CONCLUSÃO (validada)

- Regra literal: `FECHADO <> 'C' AND LoteEnt <> 'INAT'`.
- `LoteEnt` NULL **não** é contabilizado.
- Referência janela 2024: **82.118**.
- **Status: CONCLUÍDO.**

## 3. R-02 — JOIN INCOMPLETO (hipótese sustentada; NÃO concluído)

### Evidência estrutural
- `CADCONVENIO` possui índice **UNIQUE `CADCONVENIO0 (CODCONVENIO, GRUPOEMP, FILIAL)`**.
- O caso `CODCONVENIO 150` **não é conflito de classificação** ao considerar a chave completa:
  - `150 / GRUPOEMP 01 / FILIAL 01 → MODOFAT = P / ativo`
  - `150 / GRUPOEMP 01 / FILIAL 02 → MODOFAT = C / SUSPENSO`
- Entrada carrega `GRUPOEMP` e `FILIAL` (chave clusterizada `IDX_ENTRADA (CODMOVIMENTO, GRUPOEMP, FILIAL)`), viabilizando o JOIN composto por linha.

### Hipótese técnica
- O JOIN atual (`C.CODCONVENIO = E.CODCONVENIO`) multiplica linhas (277 códigos com 2 cadastros; 2024: 82.118 → 163.695 ≈ 1,99×).
- O JOIN pela chave composta (`CODCONVENIO`+`GRUPOEMP`+`FILIAL`) deve eliminar a multiplicação **por construção** (unicidade do índice).

### Status
- **PENDENTE exclusivamente da validação quantitativa** em ambiente responsivo:
  1. reexecutar medição `2024 = 82.118` (R-01);
  2. medir JOIN composto e categorias `C / P / S / Não classificado`;
  3. confirmar **soma categorial = 82.118**;
  4. então validar e corrigir o `Repository`;
  5. depois tratar R-04.
- **NÃO escolher `C`/`P`/Não classificado para o código 150** (em hipótese nenhuma; o caso é resolvido pela chave, não por seleção de valor).
- **NÃO corrigir código nem testes ainda.**

## 4. R-05 — DESEMPENHO/variabilidade (caracterizado; NÃO resolvido)

### Caracterização
- `ENTRADA` ≈ **1.912.675 registros** (approx. 1,9M).
- **Ausência de índice** adequado para `DATAHORAENT` / `CODCONVENIO` / `FECHADO` / `LoteEnt` (apenas clusterizada `IDX_ENTRADA`).
- Abertura de conexão **estável** (~70–84 ms), custo concentrado no **scan** (varia de ~1,5s até timeout 30s+).
- **`ENTRADA.ANO`/`MES` NÃO são ano/mês de referência** (histograma: `NULL` ≈ 1.778k, `-1` ≈ 58k, `0` ≈ 48k, `1` ≈ 27k) — **não usar `ANO`/`MES` como filtro de período**.
- Histograma `DATAREF`: cobertura observada 2005 → out/2024 (stats amostradas).
- **Degradação severa observada nesta sessão:** falhas progressivas até `SELECT 1` e abertura de conexão (pre/post-logon), caracterizando indisponibilidade **do servidor**, não do plano.
- **NÃO aumentar `CommandTimeout`.**
- **NÃO criar índice** (banco legado é leitura exclusiva; requer decisão/DBA).
- **NÃO reintroduzir consultas pesadas ao CASAMATER nesta sessão.**

### Tentativas registradas (timeout fixo 30s, sem aumento)
- Janela única 2024 + joins e, em seguida, segmentação trimestral T1–T4: **timeouts**.
- Em seguida, rejeição de conexão (handshake) até falha de `SELECT 1`/logon: **servidor indisponível**.

## 5. PRÓXIMOS PASSOS PRESERVADOS (ordem obrigatória)

1. **Medição R-02 em janela ociosa/responsiva** (mesma query única, timeout 30s; retry em degradação):
   - base 2024 (R-01) = 82.118;
   - linhas JOIN composto e categorias `C/P/S/Não classificado`;
   - soma categorial = 82.118.
2. Se a soma **confirmar → corrigir `Repository`** (JOIN composto) **e testes**; nova build + validação.
3. **R-04** — fortalecer testes com âncoras independentes da implementação.
4. Novo ciclo de testes de integração (R-05 responde melhor em janela ociosa).
5. Revisão completa + gate humano.
6. Atualizar PLAN-001 para Concluído **somente após validação efetiva**.
7. Commit da implementação (exibir `git status` e `git diff --stat` antes).

## 6. NÃO FAZER (restrições ativas)

- Não considerar R-02 concluído.
- Não considerar R-05 resolvido.
- Não criar índice.
- Não iniciar R-04.
- Não iniciar T-09.
- Não aumentar `CommandTimeout`.
- Não escolher `C/P/Não classificado` para código 150.
- Não fazer commit da implementação.