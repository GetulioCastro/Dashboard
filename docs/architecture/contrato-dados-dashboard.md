# Contrato de Dados do Dashboard — T-03

**Tarefa:** T-03 (PLAN-001-dashboard-indicadores.md)
**Status:** Concluído (2026-09-17) — investigação documental + complemento de validação no banco operacional; a **regra de negócio de "Despesa Provisionada"** permanece como **evidência/pendência (E/P)** para a etapa apropriada, sem bloquear a fundação estrutural
**Data:** 2026-09-17
**Autor:** Agente IA
**Restrição:** Esta tarefa é de **investigação e documentação**. Nenhum código, SQL de produção, banco ou schema foi alterado.

---

## 1. Resumo executivo

T-03 identifica as **estruturas físicas** que representam os conceitos de negócio do Dashboard no banco do SisacHTML5, usando como referência inicial a engenharia reversa do SghProg/CASAMATER (dicionário de dados, seções 1–13).

**Conclusão principal:** O banco CASAMATER do SghProg contém evidências estruturais para **9 dos 10 domínios** investigados via engenharia reversa. A **exceção foi superada pelo complemento do T-03 (2026-09-17)** com validação direta no banco operacional acessível pelo Dashboard (`CASAMATER`): o domínio **Despesas** foi identificado em **`dbo.PAGAR`** (títulos/parcelas financeiras — granularidade de pagamento/saldo) e **`dbo.PAGARC`** (obrigação financeira/fiscal complementar). A **regra de negócio da "Despesa Provisionada"** permanece em **evidência (E) / pendência (P)** — não foi inventada e não bloqueia a fundação estrutural.

**Classificação de tudo que foi encontrado:**

| Classificação | Quantidade | Significado |
|---|---|---|
| **D** (Confirmado no banco operacional — complemento 2026-09-17) | ~10 itens (domínio Despesas) | Estruturas `PAGAR`/`PAGARC`, chaves lógicas K4/K5, cardinalidade 1:N, risco de dupla contagem e presença de pagamento confirmados por consultas somente-leitura no banco `CASAMATER` |
| **E** (Evidência SghProg/CASAMATER) | ~80 itens | Estruturas documentadas na engenharia reversa do legado — não confirmadas no SisacHTML5 |
| **P** (Pendente) | ~25 itens + regra de provisionamento | Itens que exigem confirmação no banco do SisacHTML5 ou definição com o time; inclui a semântica formal de `DATAPREV`, a regra de inclusão/exclusão de situações e a classificação fixa/variável da "Despesa Provisionada" |

> **Regra de ouro aplicada:** com o acesso de leitura ao banco operacional (complemento 2026-09-17), itens do domínio Despesas foram **promovidos a D somente com evidência factual** (estrutura, chaves lógicas, cardinalidade, dupla contagem, presença de pagamento). Tudo que dependa de regra/semântica de negócio (provisionamento, fixa/variável, repasse) permaneceu **E/P** — preferiu-se pendências explícitas a conclusões incorretas.

---

## 2. Legenda e critérios de classificação

| Status | Definção |
|---|---|
| **D** | Confirmado no banco/modelo operacional do SisacHTML5 (exige acesso direto ao banco) |
| **E** | Evidência estrutural do SghProg/CASAMATER — estrutura documentada pela engenharia reversa, mas **não confirmada** como existente no SisacHTML5 |
| **P** | Pendente de confirmação — exige investigação adicional (banco SisacHTML5, configuração, time) |

**Origem da evidência (legado):** banco `CASAMATER` na instância `DESENVHMSISAC0215\MSSQLSERVER2022` (SQL Server 2022 Developer). Consultas de catálogo (`sys.tables`, `sys.columns`, `sys.partitions`), definição de procedures (`OBJECT_DEFINITION`) e amostragem de domínios (`GROUP BY`) — todas somente leitura. Referência: `docs/architecture/dicionario-de-dados.md`.

**Premissa estrutural (T-02):** o banco SQL padrão do SisacHTML5 é essencialmente o mesmo banco do SghProg/CASAMATER (poucas mudanças/exclusões de colunas). Portanto, estruturas encontradas no CASAMATER são **candidatas fortes** a existirem no SisacHTML5 — mas precisam de confirmação explícita.

---

## 3. Domínio — Atendimentos

### 3.1. Estrutura principal

| Atributo | Valor | Status | Observação |
|---|---|---|---|
| Tabela candidata | `dbo.ENTRADA` | **E** | Mestre de movimentos, ~1,9M linhas, cobertura 1899–2026-08-12 |
| Chave primária / identificador | `CODMOVIMENTO varchar(15)` | **E** | Identificador do atendimento/movimento |
| Tabela alternativa | `BI.BI_Atendimento` | **E** | BI novo (2018+), ~648K linhas; colunas amigáveis mas defasagem temporal |

### 3.2. Campos identificados

| Conceito | Campo(s) | Tipo | Status | Observação |
|---|---|---|---|---|
| Identificador do atendimento | `CODMOVIMENTO` | varchar(15) | **E** | |
| Código do paciente | `CODPACIENTE` | varchar(15) | **E** | |
| Matrícula | `MATRICULA` | varchar(70) | **E** | |
| Data/hora de entrada | `DATAHORAENT` | datetime | **E** | Usada como referência pela proc legada (RN-09) |
| Data/hora de saída | `DATAHORASAI` | datetime | **E** | |
| Data da alta | `ALTA` | datetime | **E** | Alta hospitalar |
| Local/unidade | `LOCAL` | varchar(20) | **E** | Código; referência `dbo.LOCAL` |
| Acomodação | `ACOMOD` | varchar(50) | **E** | |
| Código do convênio | `CODCONVENIO` | varchar(10) | **E** | Referência `dbo.CADCONVENIO` |
| Plano | `PLANO` | varchar(50) | **E** | |
| Médico executante | `CODMEDICO` | varchar(5) | **E** | Equipe: também `CODANEST`, `CODPRIM`, `CODSEG`, `CODINSTRUM`, `MEDRESP` |
| Especialidade | `CODESPEC` | varchar(20) | **E** | |
| Tipo de atendimento | `TIPO` | varchar(10) | **E** | Códigos 1–7, P, U (§13.2 do dicionário) |
| Tipo de atendimento (alternativo) | `TIPOATEND` | char(2) | **E** | Códigos 0–13, sem mapeamento em procs |
| Valor total | `TOTAL` | real | **E** | |
| Glosa | `GLOSA` | real | **E** | |
| Fatura | `FATURA` | real | **E** | |
| Estado de fechamento | `FECHADO` | varchar(1) | **E** | A/P/F/E/C (§13.1 do dicionário) |
| Número de fechamento | `NFECH` | varchar(10) | **E** | |
| Data de fechamento | `DATAFECH` | datetime | **E** | |
| Data referência | `DATAREF` | datetime | **E** | |
| Mês / Ano | `MES` int, `ANO` int | int | **E** | Pré-calculados |
| Grupo empresa | `GRUPOEMP` | char(2) | **E** | Multifilial |
| Filial | `FILIAL` | char(2) | **E** | Multifilial |

### 3.3. Regra de contagem (legada — evidência)

| Regra | Origem | Status |
|---|---|---|
| Contagem de atendimentos = `COUNT(CodMovimento)` com `Fechado <> 'C' AND LoteEnt <> 'INAT'` | `SP_TabIndicador` (modo `MOVPACIENTE`) | **E** |

> **Nota:** a regra de negócio atual (PRD revisão 2) define que **Atendimentos, Consultas e Exames são dimensões independentes** com contagens independentes. A regra legada de contagem única sobre `ENTRADA` pode não ser diretamente aplicável — a identificação de como cada tipo de atendimento é contabilizado no novo modelo depende do T-03 com acesso ao SisacHTML5.

### 3.4. Pendências

- [ ] **P1:** Confirmar se `ENTRADA` existe com a mesma estrutura no banco do SisacHTML5 (campos, tipos, tamanhos).
- [ ] **P2:** Confirmar se `ENTRADA.TIPO` (códigos 1–7) tem a mesma semântica no SisacHTML5, ou se há novos códigos/exclusões.
- [ ] **P3:** Confirmar se `ENTRADA.LoteEnt <> 'INAT'` continua sendo filtro válido no SisacHTML5.
- [ ] **P4:** Confirmar a fonte canônica de contagem de atendimentos para o novo dashboard (ENTRADA vs BI_Atendimento).

---

## 4. Domínio — Consultas

### 4.1. Estrutura principal

| Atributo | Valor | Status | Observação |
|---|---|---|---|
| Tabela candidata (movimentos) | `dbo.ENTRADA` com `TIPO = '1'` | **E** | `TIPO='1'` = CONSULTA (§13.2); 374K registros observados |
| Tabela de evolução | `dbo.CONSULTA` | **E** | ~2,96M linhas; `Tipo='CON'` = tempo médio de consulta |
| Tabela alternativa (BI) | `BI.BI_Atendimento` com `TipoAtendimento = 'Consulta'` | **E** | ~138K registros; coluna em texto |

### 4.2. Campos identificados

| Conceito | Campo(s) | Tabela | Status | Observação |
|---|---|---|---|---|
| Identificação da consulta | `CODMOVIMENTO` | ENTRADA | **E** | Mesmo identificador do atendimento |
| Data da consulta | `DATAHORAENT` | ENTRADA | **E** | Usada como data de referência |
| Profissional | `CODMEDICO` | ENTRADA | **E** | Médico executante |
| Paciente | `CODPACIENTE` | ENTRADA | **E** | |
| Tipo = Consulta | `TIPO = '1'` | ENTRADA | **E** | Mapeado em `SP_PacienteDia` |
| Texto tipo = Consulta | `TipoAtendimento = 'Consulta'` | BI.BI_Atendimento | **E** | Em texto; ~138K |
| Tempo de consulta | `TempoConsulta int` | BI.BI_Atendimento | **E** | Diferença DataFim - DataInicio |
| Data início | `DataInicioConsulta` | BI.BI_Atendimento | **E** | |
| Data fim | `DataFimConsulta` | BI.BI_Atendimento | **E** | |

### 4.3. Pendências

- [ ] **P5:** Confirmar se `ENTRADA.TIPO = '1'` continua representando "Consulta" no SisacHTML5.
- [ ] **P6:** Confirmar se existe tabela dedicada a consultas no SisacHTML5 (além de `ENTRADA`).
- [ ] **P7:** Confirmar se `CONSULTA` (evolução, ~2,96M linhas) é relevante para o indicador de Consultas no novo dashboard.

---

## 5. Domínio — Retorno

### 5.1. Estrutura principal

| Atributo | Valor | Status | Observação |
|---|---|---|---|
| Tabela candidata | `dbo.ENTRADA` com `TIPO = '2'` | **E** | `TIPO='2'` = RETORNO (§13.2); 148K registros observados |
| Tabela alternativa (BI) | `BI.BI_Atendimento` com `TipoAtendimento = 'Retorno'` | **E** | ~59K registros; coluna em texto |

### 5.2. Vínculo Consulta → Retorno

| Conceito | Status | Observação |
|---|---|---|
| Vínculo físico entre retorno e consulta original | **P** | Não há campo evidente em `ENTRADA` que ligue um retorno à consulta de origem. A regra de negócio (até 30 dias) é **semântica** — a identificação física do vínculo precisa ser confirmada no SisacHTML5. |
| Indicador explícito de "é retorno" | **E** | `ENTRADA.TIPO = '2'` no legado; `BI.BI_Atendimento.TipoAtendimento = 'Retorno'` |
| Data da consulta original | **P** | Não identificada no modelo legado; necessária para validar a regra de 30 dias. Pode ser derivada de `AGENDA` ou de movimento anterior — a confirmar. |

### 5.3. Regra de negócio (T-02)

> **Consulta** = atendimento realizado pelo profissional de saúde a um paciente.
> **Retorno** = consulta associada a uma consulta anterior, agendada previamente ou não, ocorrendo em até 30 dias da data da consulta.

### 5.4. Pendências

- [ ] **P8:** Confirmar se o SisacHTML5 mantém `TIPO = '2'` para Retorno.
- [ ] **P9:** Investigar se existe campo de vínculo (retorno → consulta original) no SisacHTML5 (ex.: `CODMOVORIG`, referência a movimento anterior).
- [ ] **P10:** Determinar como o Dashboard pode calcular "até 30 dias" sem um vínculo físico explícito — pode ser necessário derivar do agendamento ou de proximidade temporal.
- [ ] **P11:** Confirmar se a distinção Consulta/Retorno é feita por `TIPO` ou por outra coluna no SisacHTML5.

---

## 6. Domínio — Exames

### 6.1. Estrutura principal

| Atributo | Valor | Status | Observação |
|---|---|---|---|
| Tabela candidata (movimentos) | `dbo.ENTRADA` com `TIPO = '3'` | **E** | `TIPO='3'` = EXAME (§13.2); 640K registros observados |
| Tabela de itens | `dbo.EXAME` | **E** | Itens de exames/consultas; `Tipo='C'` (consulta) / `'E'` (exame) |
| Tabela alternativa (BI) | `BI.BI_Atendimento` com `TipoAtendimento = 'Exame'` | **E** | ~158K registros; coluna em texto |

### 6.2. Campos identificados

| Conceito | Campo(s) | Tabela | Status | Observação |
|---|---|---|---|---|
| Identificação do exame | `CODMOVIMENTO` | ENTRADA | **E** | Mesmo identificador do atendimento |
| Data do exame | `DATAHORAENT` | ENTRADA | **E** | |
| Profissional | `CODMEDICO` | ENTRADA | **E** | |
| Paciente | `CODPACIENTE` | ENTRADA | **E** | |
| Tipo = Exame | `TIPO = '3'` | ENTRADA | **E** | |
| Texto tipo = Exame | `TipoAtendimento = 'Exame'` | BI.BI_Atendimento | **E** | |
| Vínculo exame ↔ movimento | `CodPaciente = E.CodMovimento` | EXAME ↔ ENTRADA | **E** | Join por código do paciente/movimento |
| Grupo do exame | `GRUPOPROC` | EXAME | **E** | `MOVCONSULTA` / `MOVEXAME` |
| Tipo do item de exame | `Tipo` | EXAME | **E** | `'C'` = consulta, `'E'` = exame |

### 6.3. Independência vs Atendimento/Consulta

> **Evidência:** `ENTRADA.TIPO` distingue Consulta (1), Retorno (2) e Exame (3) como tipos distintos de movimento na **mesma tabela**. `EXAME` é uma tabela de **itens** que se vincula a `ENTRADA` por `CodPaciente = E.CodMovimento`. Não há hierarquia explícita entre atendimento, consulta e exame — são categorias de uma mesma tabela de movimentos.

### 6.4. Pendências

- [ ] **P12:** Confirmar se `ENTRADA.TIPO = '3'` continua representando "Exame" no SisacHTML5.
- [ ] **P13:** Confirmar se a tabela `EXAME` (itens) é relevante para o indicador de Exames no novo dashboard.
- [ ] **P14:** Determinar se exames podem ser contabilizados independentemente de atendimentos/consultas, ou se sempre pertencem a um movimento `ENTRADA`.

---

## 7. Domínio — Cobertura (Particular / Convênio / SUS)

### 7.1. Estrutura principal

| Atributo | Valor | Status | Observação |
|---|---|---|---|
| Tabela de convênios | `dbo.CADCONVENIO` | **E** | ~597 registros; `CODCONVENIO varchar(10)`, `DESCR varchar(250)`, `SUSPENSO`, `MODOFAT`, etc. |
| Campo de vínculo no atendimento | `ENTRADA.CODCONVENIO` | **E** | varchar(10); 315 códigos distintos em uso |
| Campo de cobertura (BI antigo) | `dbo.BI_Atendimento.ModoFatura` | **E** | Valores: `CONVÊNIO`(620K), `PARTICULAR`(450K), `SUS`(17K) |
| Campo de plano | `ENTRADA.PLANO` | **E** | varchar(50) |

### 7.2. Classificação das coberturas

| Cobertura | Identificação no legado | Status | Observação |
|---|---|---|---|
| **Particular** | `BI_Atendimento.ModoFatura = 'PARTICULAR'` ou `ENTRADA.CODCONVENIO` vazio/nulo | **E** | hipótese: se `CODCONVENIO` é vazio, é particular. Validar. |
| **Convênio** | `ENTRADA.CODCONVENIO` preenchido + `CADCONVENIO.DESCR` | **E** | 315 códigos em uso |
| **SUS** | `BI_Atendimento.ModoFatura = 'SUS'` | **E** | ~17K registros no BI antigo; 明らかに convênio SUS específico |
| **SUS só quando serviço atende SUS** | **P** | A regra de negócio (RN-26) exige que SUS só apareça quando o serviço efetivamente atende SUS. No legado, não foi encontrada configuração por serviço/.local que indique "atende SUS". **Pendente** de identificação no SisacHTML5. |

### 7.3. Pendências

- [ ] **P15:** Confirmar se `MODOFAT` (Particular/Convênio/SUS) existe no SisacHTML5 ou se é derivado de outra forma.
- [ ] **P16:** Investigar como o SisacHTML5 identifica que um serviço/local **atende SUS** (campo de configuração, cadastro do serviço, etc.).
- [ ] **P17:** Confirmar se `CADCONVENIO` mantém a mesma estrutura no SisacHTML5.
- [ ] **P18:** Determinar como o Dashboard distingue Particular de Convênio quando ambos usam `CODCONVENIO` — pode ser por campo `MODOFAT` ou por tipo de convênio.
- [ ] **P19:** Confirmar se `SUSPENSO`/`DATASUSP` de `CADCONVENIO` é suficiente para excluir convênios suspensos.

---

## 8. Domínio — Faturamento

### 8.1. Estrutura principal

| Atributo | Valor | Status | Observação |
|---|---|---|---|
| Tabela de guias | `dbo.FATURA` | **E** | ~8,2M linhas; itemizado por guia (uma linha por procedimento/item) |
| Tabela mestre de movimentos | `dbo.ENTRADA` | **E** | `TOTAL`, `FECHADO`, `DATAHORAENT`, `DATAFECH`, `ALTA` |
| Contas a receber | `dbo.RECEBER` | **E** | Pagamentos; **não** é faturamento (RN-12) |
| BI de faturamento | `dbo.BI_Faturamento` | **E** | ~7,5M linhas; `ValorTotal`, `DataFatura`, `DataReferencia`, `Fechamento`, `DataFechamento` |

### 8.2. Campos de faturamento

| Conceito | Campo(s) | Tabela | Status | Observação |
|---|---|---|---|---|
| Estado da conta/guia | `FECHADO` varchar(1) | ENTRADA | **E** | A=Aberto, P=Parcial, F=Fechado, E=Enviada, C=Cancelado |
| Data de referência (proc legada) | `DATAHORAENT` | ENTRADA | **E** | Usada pela proc para agrupar faturamento por período |
| Data de fechamento | `DATAFECH` | ENTRADA | **E** | |
| Número de fechamento | `NFECH` | ENTRADA | **E** | |
| Alta hospitalar | `ALTA` | ENTRADA | **E** | Gatilho para conta em processo (T-02) |
| Data do item de fatura | `DATA` | FATURA | **E** | Candidata a "data de emissão da guia" (RN-13) |
| Valor do item | `VALOR numeric(18,2)` | FATURA | **E** | |
| Quantidade | `QUANT numeric(18,2)` | FATURA | **E** | |
| Valor total do item | `VTOTAL numeric(18,2)` | FATURA | **E** | |
| Grupo/categoria do item | `GRUPO` / `GRUPOIMP` | FATURA | **E** | 1=honorários, 2-4=taxas, 5-7/M=materiais |
| Convênio | `CODCONVENIO varchar(3)` | FATURA | **E** | Nota: varchar(3) vs varchar(10) em ENTRADA |
| Médico | `CODMEDICO varchar(10)` | FATURA | **E** | |
| Local | `LOCAL varchar(20)` | FATURA | **E** | |
| Chave | `CHAVE varchar(80)` | FATURA | **E** | |
| Descrição | `DESCR varchar(200)` | FATURA | **E** | |

### 8.3. Faturamento legado (regra confirmada)

| Regra | Fonte | Status |
|---|---|---|
| Faturamento = `SUM(Total)` com `Fechado IN ('F','E')`, referência `DATAHORAENT` | `SP_TABINDICADOR` (modos `FATURAMM`/`FATURATX`/`FATURAHM`/`OPME`) | **E** |
| Categorias de item: `FATURA.Grupo` → 1=honorários, 2-4=taxas, 5-7/M=materiais, M=OPME | `SP_FATURAHM`/`SP_FATURATX`/`SP_FATURAMM`/`SP_OPME` | **E** |
| Fórmula item: `(Valor + Filme + CustoOP) * Quant` | Procs de faturamento | **E** |
| Contas a receber: `RECEBER.Condicao` → A=aberto, B=baixado | `SP_TABINDICADOR` | **E** |

### 8.4. Conta a faturar — representação

> **Regra de negócio (T-02):** Conta a faturar = contas de pacientes atendidos por Convênio que receberam alta e ainda não foram completamente faturadas.

> **Evidência legada:**

| Candidato a representação | Campo(s) | Status | Observação |
|---|---|---|---|
| Conta com alta e sem fechamento completo | `ENTRADA.ALTA IS NOT NULL AND FECHADO IN ('A','P')` | **E** | Hipótese: alta preenchida + estado Aberto ou Parcial = conta a faturar |
| Conta fechada (enviada) mas não paga | `ENTRADA.FECHADO = 'E'` | **E** | Enviada para auditoria/pagamento |
| Conta com fechamento parcial | `ENTRADA.FECHADO = 'P'` | **E** | Parcialmente faturada |
| Conta totalmente fechada | `ENTRADA.FECHADO = 'F'` | **E** | Faturamento completo |
| Faturamento por convênio | `RECEBER` com `Condicao IN ('A','B')` | **E** | Contas a receber; não é faturamento |

> **Pendente:** a correspondência exata entre os estados legados (`A/P/F/E/C`) e o fluxo conceitual do T-02 (Alta → Processo → Envio/auditoria → Faturamento completo) precisa ser confirmada no SisacHTML5. **Não se reutilizam automaticamente** os códigos do SghProg como contrato (T-02).

### 8.5. Pendências

- [ ] **P20:** Confirmar se `ENTRADA.FECHADO` (A/P/F/E/C) mantém a mesma semântica no SisacHTML5.
- [ ] **P21:** Confirmar se `FATURA` (8,2M linhas) existe no SisacHTML5 e mantém a mesma estrutura.
- [ ] **P22:** Determinar a "data de emissão da guia" (RN-13): `FATURA.DATA` ou `ENTRADA.DATAHORAENT`?
- [ ] **P23:** Confirmar como o SisacHTML5 representa a **alta** do paciente (campo `ALTA` em `ENTRADA`? Outra tabela?).
- [ ] **P24:** Confirmar o fluxo completo de faturamento no SisacHTML5: quais estados, quais campos, qual sequência.
- [ ] **P25:** Investigar se existe tabela de "processo de faturamento" ou "envio para auditoria" no SisacHTML5.
- [ ] **P26:** Confirmar se `RECEBER` (contas a receber) é relevante para o Dashboard (RN-12 diz que base = guia, não recebimento).

---

## 9. Domínio — Produção Médica / Profissional

### 9.1. Estrutura de profissional

| Atributo | Valor | Status | Observação |
|---|---|---|---|
| Tabela de médicos | `dbo.CADMEDICO` | **E** | ~3.802 registros |
| Chave | `CODMEDICO varchar(10)` | **E** | Referenciado por `ENTRADA.CODMEDICO` |
| Nome | `NOME varchar(50)` | **E** | |
| CRM | `CRM` | **E** | |
| CPF | `CPF` | **E** | |
| Especialidade (texto) | `ESPECIALIDADE varchar(100)` | **E** | Texto livre |
| Flag ativo | `ATIV varchar(20)` | **E** | Majoritariamente vazio/NULL (2.860 vazios, 927 NULL) |
| Condição | `CONDICAO varchar(1)` | **E** | Vazio (3.539), D (184), C (79) |
| Tipo | `TIPO varchar(10)` | **E** | |
| CBO | `CBO` | **E** | Classificação Brasileira de Ocupações |
| CNES | `CNES` | **E** | Cadastro Nacional de Estabelecimentos de Saúde |
| CNS | `CNS` | **E** | Cartão Nacional de Saúde |
| Filial | `GRUPOEMP`, `FILIAL` | **E** | Busca com `Filial IN ('99', ...)` |

### 9.2. Tabelas de profissionais complementares

| Tabela | Status | Observação |
|---|---|---|
| `dbo.CADMEDICOESP` (especialidades do médico) | **E** | `SEQ int`, `CODMEDICO varchar(6)`, `AMBESP varchar(10)`, `GRUPOEMP`, `FILIAL` |
| `dbo.ESPECIALIDADE` (catálogo de especialidades) | **E** | `Codigo varchar(6)`, `DESCR varchar(50)`, `CBO varchar(10)` |
| `dbo.CADPROF` (profissionais gerais) | **E** | **Vazia** (0 linhas) — sem evidência de profissionais não-médicos |

### 9.3. Fontes de produção

| Fonte | Contexto | Status | Observação |
|---|---|---|---|
| `ENTRADA.CODMEDICO` | Médico executante do movimento | **E** | Modos `MOVCONSULTA`/`MOVEXAME` |
| `ENTRADA.MEDRESP` | Médico responsável | **E** | varchar(20) |
| `ENTRADA.CODANEST` | Anestesista | **E** | |
| `CONSULTA.CODMEDICO` | Médico da consulta (`Tipo='CON'`) | **E** | Tempo médio de consulta |
| `FATURA.CODMEDICO` | Médico no item de fatura | **E** | |

### 9.4. Unidade de produção — evidências

| Evidência | Observação | Status |
|---|---|---|
| Legado contava `SUM(Total)` por `CodMedico` (modo `MOVCONSULTA`/`MOVEXAME`) | Produção = valor financeiro por profissional | **E** |
| Legado contava `COUNT(CodMovimento)` por `CodMedico` (modo `MOVPACIENTE`) | Produção = quantidade de atendimentos por profissional | **E** |
| `CONSULTA.CODMEDICO` com `Tipo='CON'` | Tempo médio de consulta por médico | **E** |
| `FATURA.CODMEDICO` | Itens faturados por médico | **E** |

> **Regra de negócio (T-02):** produtividade = produção do profissional no período, comparável entre profissionais. Produção **não** é presumida como apenas consultas — consultas, exames solicitados e outras produções registradas podem compô-la.

> **Pendente:** a **unidade técnica de produção** (o que exatamente conta como "uma produção") precisa ser definida no SisacHTML5. Opções evidenciadas: (a) quantidade de movimentos por profissional; (b) valor financeiro dos movimentos; (c) tempo de consulta; (d) combinação. **Não se inventa fórmula.**

### 9.5. Critério de profissional ativo

| Candidato | Campo | Status | Observação |
|---|---|---|---|
| Flag de ativo no cadastro | `CADMEDICO.ATIV` | **E** | Majoritariamente vazio — **não confiável** |
| Condição do cadastro | `CADMEDICO.CONDICAO` | **E** | Vazio (93%), D (5%), C (2%) — significado não mapeado |
| Presença de movimentos no período | `ENTRADA.CODMEDICO` com `DATAHORAENT` no período | **E** | Hipótese T-01: profissional com ≥1 movimento = "ativo no período" |
| Exclusão por desligamento | **P** | Não há campo evidente de data de desligamento em `CADMEDICO` |

### 9.6. Pendências

- [ ] **P27:** Confirmar se `CADMONICO` existe no SisacHTML5 e mantém a mesma estrutura.
- [ ] **P28:** Determinar o critério de "profissional ativo" no SisacHTML5 (flag de cadastro? movimentos no período?).
- [ ] **P29:** Definir a **unidade técnica de produção** no SisacHTML5 (quantidade, valor, tempo, combinação).
- [ ] **P30:** Confirmar qual papel profissional conta para produtividade: executante (`CODMEDICO`), responsável (`MEDRESP`), ambos?
- [ ] **P31:** Confirmar a fonte de especialidade: `CADMEDICO.ESPECIALIDADE` (texto) vs `CADMEDICOESP`/`ESPECIALIDADE` (relacional).
- [ ] **P32:** Investigar se existem profissionais não-médicos no SisacHTML5 (`CADPROF` está vazia no legado).

---

## 10. Domínio — Despesas

> **Complemento do T-03 (2026-09-17) — validação direta no banco operacional acessível pelo Dashboard:** servidor/instância `DESENVHMSISAC02\MSSQLSERVER2022`, banco `CASAMATER`, usuário `dashboard_readonly` (somente leitura — credencial fornecida pelo time; senha não registrada). Consultas: catálogo/metadados (`sys.*`), `COUNT`/`SUM`/`GROUP BY` e pareamento agregado por chaves candidatas (K4/K5). **Nenhum dado, objeto, índice ou permissão do banco foi alterado.** Evidência inicial: **PAGAR / PAGARC** (evidência de negócio do time/Sisac Desktop VCL).

### 10.1. Consolidação do contrato — Despesas Provisionadas

| Item | Resultado | Status |
|---|---|---|
| Tabela principal (granularidade financeira) | `dbo.PAGAR` — granularidade de **títulos/parcelas financeiras**: `VALOR`, `NPARC`, `DATAVENC`, `DATAPAG`, `VALORPAG`, `SALDO`, `DATAPREV` | **D** |
| Tabela relacionada/complementar | `dbo.PAGARC` — estrutura relacionada à **obrigação financeira/fiscal**: `VALOR`, `DATAEMISSAO`, `DATAVENC`, `DATAPAG`, `NPARC`, `CCUSTO`, `NATOP`, `TIPOCUSTO`, `REPASSE` e outros campos de classificação | **D** |
| FK declarada entre PAGAR e PAGARC | **Não existe** chave estrangeira declarada | **D** |
| Chaves lógicas observadas | **K4** = `CODFORNECEDOR + NFISCAL + FILIAL + GRUPOEMP`; **K5** = K4 + `NPARC`. Correspondência lógica forte confirmada (resultados registrados conforme a saída SQL da investigação) | **D** |
| Cardinalidade observada | **1:N** entre `PAGARC` e `PAGAR` em diversos casos → PAGAR **não** deve ser tratado como cópia 1:1 de PAGARC | **D** |
| Risco de dupla contagem | `PAGARC.VALOR` × `SUM(PAGAR.VALOR)` por K4: **168.735 correspondências iguais** entre **170.062 pares** analisados → PAGAR e PAGARC **não devem ser somados como despesas independentes** | **D** |
| Acompanhamento financeiro (parcela/pagamento/saldo) | PAGAR possui registros **pagos e não pagos**; PAGARC apresentou `DATAPAG` **NULL em todos os registros analisados** → PAGAR é a estrutura com dados de parcela/pagamento/saldo observáveis na investigação | **D** |
| Evidência de provisionamento | `DATAPREV` preenchida em **todos os 10.087 registros PAGAR não pagos** — evidência forte de informação relacionada ao provisionamento | **E** |
| Semântica formal de `DATAPREV` | Semântica de `DATAPREV` como "data de provisionamento" **não confirmada** por documentação técnica ou código legado disponível | **P** |
| Regra de inclusão/exclusão (cancelamentos, estornos e outras situações) | Regra definitiva da "Despesa Provisionada" **não comprovada** documentalmente | **P** |

### 10.2. Despesas — registro consolidado

- **Evidência inicial:** `PAGAR` / `PAGARC` (evidência de negócio do time).
- **Resultado da investigação:** estruturas **confirmadas** no banco `CASAMATER` (§10.1). Granularidade financeira em `PAGAR`; `PAGARC` complementar/fiscal (detalhe por nota fiscal, classificação). Sem FK declarada; correspondência lógica por K4/K5. Cardinalidade 1:N. **Risco de dupla contagem confirmado** — não somar as duas tabelas como despesas independentes.
- **Classificação:** estruturas e relacionamento = **D**; evidência de provisionamento = **E**; regras de negócio da "Despesa Provisionada" = **P**.
- **Evidências:** consultas de leitura (catálogo + agregados + pareamento K4/K5, só SELECT/metadados) em `CASAMATER` via `dashboard_readonly` (2026-09-17).
- **Regra identificada:** **não comprovada** — nenhuma fórmula de provisionamento foi inventada.
- **Pendências:** P38–P40 (§10.4) — a regra específica da "Despesa Provisionada" permanece pendente para a etapa apropriada (Fase 3 — Core/Data, T-13).

### 10.3. Histórico preservado — investigação documental anterior (2026-09-17, superada pelo complemento)

#### 10.3.1. Estrutura identificada (documental)

| Atributo | Valor | Status | Observação |
|---|---|---|---|
| Tabela de despesas | **Não encontrada** | **P** | Não existe tabela identificada no CASAMATER que represente despesas fixas/variáveis (energia, água, aluguel, condomínio, salários, repasses, equipamentos, móveis). *(Superado: origem identificada em `PAGAR`/`PAGARC` — §10.1.)* |

#### 10.3.2. O que foi investigado (documental)

| Fonte investigada | Resultado | Status |
|---|---|---|
| Tabelas do CASAMATER (§1.2 do dicionário) | Nenhuma tabela com nome sugestivo de despesa (`DESPESA`, `DESP`, `CUSTO`, `FIXO`, `VARIAVEL`) foi encontrada | **E** |
| `dbo.FATURA` | É faturamento (guia de cobrança), não despesa | **E** |
| `dbo.RECEBER` | É contas a receber (pagamentos), não despesa | **E** |
| Procedures do legado | `SP_ATUALIZA_DASHBOARD` não calcula despesas | **E** |

#### 10.3.3. Hipóteses anteriores (documental — superadas parcialmente)

- **H1:** Despesas podem estar em módulo contábil/financeiro separado do CASAMATER (ex.: módulo de compras, patrimônio, folha de pagamento). *(Parcialmente respondido: `PAGAR`/`PAGARC` no próprio CASAMATER — financeiro/contas a pagar; classificação contábil a validar.)*
- **H2:** Despesas podem estar em outro banco (ex.: `CONFIG` ou banco contábil) — não acessado. *(Mantida como não confirmada para outras origens.)*
- **H3:** Despesas podem ser importadas de sistema externo para o SisacHTML5 — a confirmar. *(Mantida.)*

### 10.4. Pendências do domínio Despesas (pós-complemento)

**Original (P33–P37) — status atual após o complemento:**

- [ ] **P33 (parcialmente resolvido):** investigar módulo de despesas/integração contábil no SisacHTML5 → origem estrutural identificada em `PAGAR`/`PAGARC` (**D**); validar com o produto se essas tabelas permanecem a fonte única do indicador no SisacHTML5. → **E/P**
- [ ] **P34 (resolvido na estrutura):** origem dos dados de despesas no SisacHTML5 → confirmada em **`PAGAR`** (principal) e **`PAGARC`** (complementar) no banco `CASAMATER` (**D**); validação funcional com o produto pendente. → **D/E**
- [ ] **P35 (mantida):** como o SisacHTML5 classifica despesas como **fixas/variáveis** (RN-37/RN-38) → campos candidatos em `PAGARC` (`TIPOCUSTO`, `REPASSE`, `NATOP`, `CCUSTO`, `PROVREP`) existem, mas **sem semântica confirmada**. → **P**
- [ ] **P36 (parcialmente resolvida):** data de referência das despesas (competência × pagamento) → candidatas confirmadas na estrutura (`DATAVENC`, `DATAPREV`, `DATAPAG`, `DATAEMISSAO`, `DataEmissao`), mas a **data-chave do indicador permanece P**. → **E/P**
- [ ] **P37 (parcialmente resolvida):** repasse médico → campos presentes em `PAGAR` (`NFREPASSE`, `CODFORNREP`, `CODMEDICO`) e `PAGARC` (`REPASSE`, `PROVREP`, `CODFORNREP`, `CODMEDICO`); semântica e classificação fixa/variável pendentes. → **E/P**

**Novas pendências (complemento 2026-09-17):**

- [ ] **P38:** Semântica formal de `DATAPREV` como "data de provisionamento" — **E** (100% dos 10.087 não pagos preenchida), **P** para confirmação documental/técnica.
- [ ] **P39:** Regra definitiva de **inclusão/exclusão de cancelamentos, estornos e outras situações** da "Despesa Provisionada" — **P**, não comprovada.
- [ ] **P40:** Definição de grupo **fixa/variável** e **classificação do repasse médico** na "Despesa Provisionada" — **P**, campo candidato `TIPOCUSTO`/`REPASSE` sem semântica confirmada.

> **Pendência específica da regra de "Despesa Provisionada":** a regra de negócio que define **quando uma despesa está provisionada** (quais campos/filtros/situações) permanece **P** para a etapa apropriada (Fase 3 — definição em Core/Data, T-13). **Não bloqueia a fundação estrutural** (T-04 já concluído e validado).

---

## 11. Mapeamento de datas por domínio

| Domínio | Data candidata(s) | Campo(s) | Tabela | Status | Observação |
|---|---|---|---|---|---|
| **Atendimentos** | Data do atendimento | `DATAHORAENT` | ENTRADA | **E** | Referência usada pela proc legada (RN-09) |
| **Atendimentos** | Data de saída | `DATAHORASAI` | ENTRADA | **E** | Alternativa; não usada pela proc |
| **Atendimentos** | Data da alta | `ALTA` | ENTRADA | **E** | Alta hospitalar |
| **Consultas** | Data da consulta | `DATAHORAENT` | ENTRADA (TIPO=1) | **E** | |
| **Consultas** | Data início/fim | `DataInicioConsulta`, `DataFimConsulta` | BI.BI_Atendimento | **E** | BI pré-agregado |
| **Retorno** | Data do retorno | `DATAHORAENT` | ENTRADA (TIPO=2) | **E** | |
| **Retorno** | Data da consulta original | **Não identificada** | — | **P** | Necessária para regra de 30 dias |
| **Exames** | Data do exame | `DATAHORAENT` | ENTRADA (TIPO=3) | **E** | |
| **Faturamento** | Data de referência (proc) | `DATAHORAENT` | ENTRADA | **E** | Usada para agrupar por período |
| **Faturamento** | Data de emissão da guia | `DATA` | FATURA | **E** | Candidata a "data de emissão" (RN-13) |
| **Faturamento** | Data de fechamento | `DATAFECH` | ENTRADA | **E** | |
| **Faturamento** | Data de fechamento (BI) | `DataFechamento` | BI_Faturamento | **E** | |
| **Faturamento** | Data do item de fatura | `DataFatura` | BI_Faturamento | **E** | |
| **Produção** | Data do movimento | `DATAHORAENT` | ENTRADA | **E** | |
| **Produção** | Data da consulta | `DATAHORAENT` | CONSULTA | **E** | Para `Tipo='CON'` |
| **Despesas** | Vencimento / pagamento / previsão / emissão | `DATAVENC`, `DATAPAG`, `DATAPREV`, `DataEmissao` (PAGAR); `DATAEMISSAO`, `DATAVENC`, `DATAPAG` (PAGARC) | PAGAR/PAGARC | **E** | Estruturas confirmadas (D); **data-chave** do indicador (competência × pagamento) = **P** (P36) |
| **Cobertura** | Data de vigência do convênio | `DATAVIGOR` | CADCONVENIO | **E** | Para convênios com vigência |
| **Cobertura** | Data de suspensão | `DATASUSP` | CADCONVENIO | **E** | Para convênios suspensos |

> **Observação transversal:** a data `DATAHORAENT` é a **data de referência dominante** no legado — usada pela proc para atendimentos, consultas, exames e faturamento. No SisacHTML5, pode haver datas mais específicas por domínio. **Pendente** de confirmação.

---

## 12. Mapa de relacionamentos (evidência legada)

```
CADCONVENIO (cadastro de convênios)
    ↑ CODCONVENIO
ENTRADA (mestre de movimentos/atendimentos)
    ├── CODMEDICO → CADMEDICO (cadastro de médicos)
    │       └── → CADMEDICOESP → ESPECIALIDADE
    ├── CODCONVENIO → CADCONVENIO
    ├── LOCAL → dbo.LOCAL (árvore de unidades)
    ├── TIPO = '1' → CONSULTA (movimento do tipo consulta)
    ├── TIPO = '2' → RETORNO (movimento do tipo retorno)
    ├── TIPO = '3' → EXAME (movimento do tipo exame)
    ├── CodMovimento → EXAME.CodPaciente (itens de exame)
    ├── CodMovimento → CONSULTA (evolução/tempo)
    ├── CodMovimento → FATURA.Chave? (itens de fatura)
    ├── CodMovimento → ACOMOD.CodPaciente (leitos)
    ├── FECHADO → estados: A/P/F/E/C
    └── CodMovimento → BI_PACIENTEDIA.CodMovimento (ocupação)

FATURA (itens de fatura/guia)
    ├── CODCONVENIO (varchar(3))
    ├── CODMEDICO
    ├── LOCAL
    └── GRUPO → categorias (1=honorários, 2-4=taxas, 5-7/M=materiais)

RECEBER (contas a receber)
    ├── NFECH → ENTRADA.NFECH
    ├── CODCONVENIO
    └── Condicao: A=aberto, B=baixado

BI_PACIENTEDIA (censo diário de leitos)
    ├── Data, CodMovimento, CodPaciente
    ├── Local (nome), LOCAL2 (código)
    ├── PacienteDia, LeitoDia, LeitoLivre
    └── TipoLeito (U=UTI)

dbo.LOCAL (árvore de unidades)
    ├── LOCAL (código), DESCR, TIPO
    └── LOCALPAI (hierarquia)
```

> **Nota:** este mapa é baseado em **evidências do SghProg/CASAMATER**. Os relacionamentos reais no SisacHTML5 podem diferir. **Não assumir** que este mapa é válido sem confirmação.

---

## 13. Matriz consolidada do contrato de dados

| Domínio | Conceito | Tabela/Estrutura | Campo(s) | Tipo | Relacionamento | Evidência | Status | Observação |
|---|---|---|---|---|---|---|---|---|
| Atendimento | Tabela principal | `ENTRADA` | — | table | — | sys.tables | **E** | ~1,9M linhas |
| Atendimento | Chave | `ENTRADA` | `CODMOVIMENTO` | varchar(15) | PK | sys.columns | **E** | |
| Atendimento | Data do atendimento | `ENTRADA` | `DATAHORAENT` | datetime | — | sys.columns | **E** | Referência da proc legada |
| Atendimento | Data saída | `ENTRADA` | `DATAHORASAI` | datetime | — | sys.columns | **E** | |
| Atendimento | Alta | `ENTRADA` | `ALTA` | datetime | — | sys.columns | **E** | |
| Atendimento | Paciente | `ENTRADA` | `CODPACIENTE` | varchar(15) | — | sys.columns | **E** | |
| Atendimento | Profissional | `ENTRADA` | `CODMEDICO` | varchar(5) | → CADMEDICO | sys.columns | **E** | |
| Atendimento | Tipo | `ENTRADA` | `TIPO` | varchar(10) | — | sys.columns + SP | **E** | 1=Cons, 2=Ret, 3=Exa, 4=PQA, 5=Cli, 6=Cir |
| Atendimento | Tipo (alt) | `ENTRADA` | `TIPOATEND` | char(2) | — | sys.columns | **E** | Códigos 0–13, sem mapeamento |
| Atendimento | Convênio | `ENTRADA` | `CODCONVENIO` | varchar(10) | → CADCONVENIO | sys.columns | **E** | |
| Atendimento | Plano | `ENTRADA` | `PLANO` | varchar(50) | — | sys.columns | **E** | |
| Atendimento | Local | `ENTRADA` | `LOCAL` | varchar(20) | → dbo.LOCAL | sys.columns | **E** | |
| Atendimento | Estado | `ENTRADA` | `FECHADO` | varchar(1) | — | sys.columns + SP | **E** | A/P/F/E/C |
| Atendimento | Valor | `ENTRADA` | `TOTAL` | real | — | sys.columns | **E** | |
| Atendimento | Contagem legada | `ENTRADA` | — | COUNT | `Fechado<>'C' AND LoteEnt<>'INAT'` | SP_TabIndicador | **E** | Filtro LoteEnt |
| Consulta | Tipo=1 | `ENTRADA` | `TIPO='1'` | varchar(10) | — | SP_PacienteDia | **E** | 374K registros |
| Consulta | Texto | `BI.BI_Atendimento` | `TipoAtendimento='Consulta'` | varchar(50) | — | sys.columns | **E** | ~138K |
| Consulta | Tempo | `BI.BI_Atendimento` | `TempoConsulta` | int | — | sys.columns | **E** | Diferença DataFim - DataInicio |
| Retorno | Tipo=2 | `ENTRADA` | `TIPO='2'` | varchar(10) | — | SP_PacienteDia | **E** | 148K registros |
| Retorno | Texto | `BI.BI_Atendimento` | `TipoAtendimento='Retorno'` | varchar(50) | — | sys.columns | **E** | ~59K |
| Retorno | Vínculo c/ consulta | — | — | — | — | — | **P** | Não identificado no legado |
| Exame | Tipo=3 | `ENTRADA` | `TIPO='3'` | varchar(10) | — | SP_PacienteDia | **E** | 640K registros |
| Exame | Texto | `BI.BI_Atendimento` | `TipoAtendimento='Exame'` | varchar(50) | — | sys.columns | **E** | ~158K |
| Exame | Itens | `dbo.EXAME` | — | table | CodPaciente=CodMovimento | sys.tables | **E** | |
| Cobertura | Cadastro convênio | `dbo.CADCONVENIO` | — | table | — | sys.tables | **E** | ~597 registros |
| Cobertura | Código convênio | `CADCONVENIO` | `CODCONVENIO` | varchar(10) | — | sys.columns | **E** | |
| Cobertura | Descrição | `CADCONVENIO` | `DESCR` | varchar(250) | — | sys.columns | **E** | |
| Cobertura | Suspenso | `CADCONVENIO` | `SUSPENSO` | — | — | sys.columns | **E** | |
| Cobertura | Modo fatura | `CADCONVENIO` | `MODOFAT` | — | — | sys.columns | **E** | |
| Cobertura | Particular | `BI_Atendimento` | `ModoFatura='PARTICULAR'` | varchar(50) | — | sys.columns | **E** | ~450K no BI antigo |
| Cobertura | Convênio | `BI_Atendimento` | `ModoFatura='CONVÊNIO'` | varchar(50) | — | sys.columns | **E** | ~620K no BI antigo |
| Cobertura | SUS | `BI_Atendimento` | `ModoFatura='SUS'` | varchar(50) | — | sys.columns | **E** | ~17K no BI antigo |
| Cobertura | SUS=svc atende | — | — | — | — | — | **P** | Configuração não encontrada |
| Faturamento | Itens de fatura | `dbo.FATURA` | — | table | — | sys.tables | **E** | ~8,2M linhas |
| Faturamento | Data item | `FATURA` | `DATA` | datetime | — | sys.columns | **E** | |
| Faturamento | Valor | `FATURA` | `VALOR` | numeric(18,2) | — | sys.columns | **E** | |
| Faturamento | Quantidade | `FATURA` | `QUANT` | numeric(18,2) | — | sys.columns | **E** | |
| Faturamento | Valor total | `FATURA` | `VTOTAL` | numeric(18,2) | — | sys.columns | **E** | |
| Faturamento | Grupo | `FATURA` | `GRUPO`/`GRUPOIMP` | varchar(20) | — | sys.columns | **E** | 1=hon, 2-4=tax, 5-7/M=mat |
| Faturamento | Convênio | `FATURA` | `CODCONVENIO` | varchar(3) | — | sys.columns | **E** | Nota: varchar(3) |
| Faturamento | Estado | `ENTRADA` | `FECHADO` | varchar(1) | — | SP | **E** | F=E, E=Env, A=Ab, P=Par, C=Can |
| Faturamento | Fechamento | `ENTRADA` | `DATAFECH` | datetime | — | sys.columns | **E** | |
| Faturamento | Núm fechamento | `ENTRADA` | `NFECH` | varchar(10) | — | sys.columns | **E** | |
| Faturamento | Receber | `dbo.RECEBER` | — | table | NFECH→ENTRADA | sys.tables | **E** | Contas a receber |
| Faturamento | Condicao(Receber) | `RECEBER` | `CONDICAO` | varchar(1) | — | SP | **E** | A=aberto, B=baixado |
| Produção | Médico | `dbo.CADMEDICO` | — | table | — | sys.tables | **E** | ~3.802 registros |
| Produção | Chave médico | `CADMEDICO` | `CODMEDICO` | varchar(10) | — | sys.columns | **E** | |
| Produção | Nome | `CADMEDICO` | `NOME` | varchar(50) | — | sys.columns | **E** | |
| Produção | Especialidade | `CADMEDICO` | `ESPECIALIDADE` | varchar(100) | — | sys.columns | **E** | Texto livre |
| Produção | Ativo | `CADMEDICO` | `ATIV` | varchar(20) | — | sys.columns | **E** | Majoritariamente vazio |
| Produção | Condição | `CADMEDICO` | `CONDICAO` | varchar(1) | — | sys.columns | **E** | Vazio/D/C |
| Produção | Fonte contagem | `ENTRADA` | `CODMEDICO` | varchar(5) | → CADMEDICO | SP | **E** | Modos MOVCONSULTA/MOVEXAME |
| Produção | Especialidade rel. | `dbo.CADMEDICOESP` | `CODMEDICO` | varchar(6) | → CADMEDICO | sys.tables | **E** | |
| Produção | Cat. especialidade | `dbo.ESPECIALIDADE` | `Codigo` | varchar(6) | — | sys.tables | **E** | |
| Produção | Prof. gerais | `dbo.CADPROF` | — | table | — | sys.tables | **E** | **Vazia** |
| Despesas | Tabela principal (títulos/parcelas) | `dbo.PAGAR` | — | table | — | sys + dados | **D** | Confirmada em CASAMATER (complemento 2026-09-17) |
| Despesas | Valor do título | `PAGAR` | `VALOR` | numeric(15,2) | — | sys.columns | **D** | Granularidade parcela |
| Despesas | Nº parcela | `PAGAR` | `NPARC` | int | parte de K5 | sys.columns | **D** | |
| Despesas | Vencimento | `PAGAR` | `DATAVENC` | datetime | — | sys.columns | **D** | |
| Despesas | Pagamento | `PAGAR` | `DATAPAG` / `VALORPAG` | datetime / numeric(15,2) | — | sys.columns | **D** | Pago × não pago observado |
| Despesas | Saldo | `PAGAR` | `SALDO` | numeric(15,2) | — | sys.columns | **D** | |
| Despesas | Previsão (provisionamento) | `PAGAR` | `DATAPREV` | datetime | — | sys.columns | **E** | 100% dos não pagos preenchida; semântica = **P** (P38) |
| Despesas | Fornecedor | `PAGAR`/`PAGARC` | `CODFORNECEDOR` | varchar | K4 | sys.columns | **D** | |
| Despesas | NF/título | `PAGAR`/`PAGARC` | `NFISCAL` | varchar | K4 | sys.columns | **D** | |
| Despesas | Filial / grupo empresa | `PAGAR`/`PAGARC` | `FILIAL` / `GRUPOEMP` | char(2) | K4 | sys.columns | **D** | |
| Despesas | Tabela complementar (fiscal) | `dbo.PAGARC` | — | table | — | sys + dados | **D** | Confirmada em CASAMATER (complemento 2026-09-17) |
| Despesas | Emissão NF | `PAGARC` | `DATAEMISSAO` | datetime | — | sys.columns | **D** | |
| Despesas | Centro de custo | `PAGARC` | `CCUSTO` | varchar(30) | — | sys.columns | **D** | Semântica de classificação = **P** |
| Despesas | Natureza operação | `PAGARC` | `NATOP` | varchar(1) | — | sys.columns | **D** | Semântica = **P** |
| Despesas | Tipo custo | `PAGARC` | `TIPOCUSTO` | char(1) | — | sys.columns | **D** | Candidato fixa/var — semântica = **P** (P35/P40) |
| Despesas | Repasse médico | `PAGARC`/`PAGAR` | `REPASSE`, `PROVREP`; `NFREPASSE`, `CODFORNREP`, `CODMEDICO` | — | — | sys.columns | **E** | Semântica/classificação = **P** (P37) |
| Despesas | Dupla contagem | `PAGAR` × `PAGARC` | K4/K5 | — | 1:N lógico | dados | **D** | 168.735/170.062 NFs iguais por K4 — **não somar** |
| Despesas | Classificação fixa/var | — | — | — | — | — | **P** | Não identificada (P35/P40) |
| Despesas | Data referência (indicador) | — | — | — | — | — | **P** | Data-chave competência × pagamento (P36) |
| Período | Data ref. atendimentos | `ENTRADA` | `DATAHORAENT` | datetime | — | sys.columns | **E** | |
| Período | Data ref. faturamento | `ENTRADA`/`FATURA` | `DATAHORAENT`/`DATA` | datetime | — | sys.columns | **E** | 2 candidatas |
| Período | Data ref. produção | `ENTRADA` | `DATAHORAENT` | datetime | — | sys.columns | **E** | |
| Período | Datas Delphi-zero | `ENTRADA` | `DATAHORAENT` = 1899-12-30 | datetime | — | sys.columns | **E** | Filtrar em queries |

---

## 14. Riscos de performance e duplicidade

| Risco | Tabela(s) | Volume | Tipo | Observação |
|---|---|---|---|---|
| Tabela muito volumosa | `FATURA` | ~8,2M linhas | Join/Agregação | Exige índices adequados em `DATA`, `CODCONVENIO`, `LOCAL` |
| Tabela muito volumosa | `GLOSA` | ~5,0M linhas | Join/Agregação | Fora do MVP, mas Volume alto |
| Tabela volumosa | `CONSULTA` | ~2,96M linhas | Join/Agregação | |
| Tabela volumosa | `ENTRADA` | ~1,9M linhas | Filtro/Agregação | Tabela central; toda query passa por ela |
| Datas Delphi-zero | `ENTRADA.DATAHORAENT` | ~valores 1899-12-30 | Filtro | Queries de período devem excluir essas linhas |
| Tipos numéricos desiguais | `ENTRADA.TOTAL` (real), `FATURA.VALOR` (numeric(18,2)) | — | Arredondamento | Tratar na camada de leitura (RN-05) |
| Duas famílias BI | `dbo.BI_*` vs `BI.*` | — | Qualificação | Sempre qualificar schema (`dbo.` vs `BI.`) |
| Tamanhos de colunas inconsistentes | `FATURA.CODCONVENIO` (varchar(3)) vs `ENTRADA.CODCONVENIO` (varchar(10)) | — | Join | Unificar na camada Data (T-07) |
| LOCAL com códigos em formatos distintos | `ENTRADA.LOCAL` (varchar 20), `GLOSA.LOCAL` (varchar 10), `BI.Local2` | — | Join | Consolidação na camada Data |
| Risco de duplicidade | `ENTRADA` com múltiplos registros por paciente/dia | — | Agregação | Filtro `Fechado<>'C' AND LoteEnt<>'INAT'` evita duplicidade legada; validar no SisacHTML5 |
| Risco de duplicidade | `FATURA` com múltiplos itens por guia | — | Agregação | Cada linha = 1 item; `SUM(VALOR)` ou `SUM(VTOTAL)` por movimento |
| Períodos muito grandes | Consultas históricas 2025+ | — | Performance | Tratamento controlado de lentidão (RN-43) |
| BI defasado | `BI_PACIENTEDIA` parou em 2024-10-22 | — | Cobertura | `CONTLEITO` vai até 2026-04; verificar qual está ativo |
| Risco de dupla contagem | `PAGAR` × `PAGARC` | — | Join/Agregação | PAGAR e PAGARC representam a mesma despesa em níveis diferentes; por K4, **168.735 de 170.062 NFs** com `PAGARC.VALOR` = Σ(`PAGAR.VALOR`) → **não somar as duas tabelas como despesas independentes**; usar K4/K5 como chave de pareamento |

---

## 15. Itens que exigem acesso ao banco do SisacHTML5

> Os itens abaixo **não podem ser respondidos** apenas com documentação. Exigem acesso de leitura ao banco do SisacHTML5.

| # | Item | Domínio | Tipo de consulta necessária |
|---|---|---|---|
| 1 | Existência de `ENTRADA` e compatibilidade de estrutura | Atendimento | `sys.tables`/`sys.columns` |
| 2 | Semântica de `ENTRADA.TIPO` no SisacHTML5 | Atendimento/Consulta/Retorno/Exame | `GROUP BY TIPO` + amostragem |
| 3 | Existência de `FATURA` e compatibilidade | Faturamento | `sys.tables`/`sys.columns` |
| 4 | Semântica de `ENTRADA.FECHADO` no SisacHTML5 | Faturamento | `GROUP BY FECHADO` |
| 5 | Existência de `CADMONICO` e campos de "ativo" | Profissional | `sys.tables`/`sys.columns` |
| 6 (resolvido) | Existência de tabela de despesas → **`PAGAR`/`PAGARC` confirmadas** no banco operacional (complemento 2026-09-17) | Despesas | `sys.tables`/`sys.columns` + agregações e pareamento K4/K5 — **concluído** |
| 7 | Campo de cobertura (Particular/Convênio/SUS) | Cobertura | `sys.columns` + amostragem |
| 8 | Configuração "serviço atende SUS" | Cobertura | Tabelas de configuração/cadastro |
| 9 | Campo de vínculo retorno→consulta | Retorno | `sys.columns` (busca por nomes sugestivos) |
| 10 | Estrutura de alta do paciente | Faturamento | `sys.columns` em `ENTRADA` ou outra tabela |

---

## 16. Critérios de aceite — verificação

| Critério | Status | Observação |
|---|---|---|
| Atendimento identificado ou marcado Pendente | ✅ | Identificado como evidência (E) em `ENTRADA` |
| Consulta identificada ou marcada Pendente | ✅ | Identificada como evidência (E): `ENTRADA.TIPO='1'` |
| Retorno identificado ou marcado Pendente | ✅ | Identificado como evidência (E): `ENTRADA.TIPO='2'`; vínculo = P |
| Exame identificado ou marcado Pendente | ✅ | Identificado como evidência (E): `ENTRADA.TIPO='3'` + `EXAME` |
| Particular identificado ou marcada Pendente | ✅ | Identificada como evidência (E): `BI_Atendimento.ModoFatura` |
| Convênio identificado ou marcada Pendente | ✅ | Identificado como evidência (E): `CADCONVENIO` + `ENTRADA.CODCONVENIO` |
| SUS identificado ou marcada Pendente | ✅ | Identificado como evidência (E): `ModoFatura='SUS'`; regra "svc atende SUS" = P |
| Alta identificada ou marcada Pendente | ✅ | Identificada como evidência (E): `ENTRADA.ALTA` |
| Conta a faturar identificada ou marcada Pendente | ✅ | Hipótese como evidência (E): `FECHADO IN ('A','P')` + `ALTA IS NOT NULL` |
| Conta faturada identificada ou marcada Pendente | ✅ | Identificada como evidência (E): `FECHADO IN ('F','E')` |
| Produção médica identificada ou marcada Pendente | ✅ | Identificada como evidência (E): `ENTRADA.CODMEDICO`; unidade de produção = P |
| Profissional identificado | ✅ | Identificado como evidência (E): `CADMONICO` |
| Critério físico de profissional ativo identificado ou marcado Pendente | ✅ | Marcado Pendente (P): `ATIV`/`CONDICAO` não confiáveis |
| Despesas identificadas | ✅ | Confirmado (D): `PAGAR` (títulos/parcelas) e `PAGARC` (complementar/fiscal) — complemento 2026-09-17 |
| Estruturas de classificação fixa/variável identificadas ou marcadas Pendente | ✅ | Marcado Pendente (P): candidatos `TIPOCUSTO`/`REPASSE`/`NATOP`/`CCUSTO` sem semântica confirmada |
| Datas relevantes identificadas por domínio | ✅ | Mapeadas (§11); múltiplas candidatas por domínio |
| Relacionamentos principais documentados | ✅ | Mapa lógico documentado (§12) |
| Evidência SghProg separada de confirmação SisacHTML5 | ✅ | Todos os itens marcados E (evidência) ou P (pendente); zero D |
| Nenhuma regra de negócio inventada | ✅ | |
| Nenhum código implementado | ✅ | |
| Nenhum SQL de produção criado | ✅ | |
| Nenhuma alteração de banco realizada | ✅ | |

---

## 17. Recomendação de fechamento

### T-03 CONCLUÍDO (2026-09-17) — com ressalvas.

**Motivo:** a investigação documental produziu a base de evidências (E/P). O **complemento do T-03 (2026-09-17)** executou **validação direta no banco operacional acessível pelo Dashboard** (`CASAMATER`, usuário `dashboard_readonly`, somente leitura), permitindo promover o domínio **Despesas** a **D** e superar o bloqueio de origem das despesas. A **regra de negócio da "Despesa Provisionada"** permanece registrada como **E/P** para a etapa apropriada (Fase 3 — definição em Core/Data, T-13) e **não bloqueia a fundação estrutural** (T-04 — já concluído e validado).

### O que foi entregue:

1. **Base de evidências** para os domínios — estruturas do SghProg/CASAMATER mapeadas e classificadas (**E**).
2. **Domínio Despesas confirmado (D):** `PAGAR` (títulos/parcelas — principal) e `PAGARC` (complementar/fiscal); chaves lógicas **K4/K5**; cardinalidade **1:N**; **risco de dupla contagem** (168.735/170.062 NFs iguais por K4 — não somar as duas tabelas).
3. **37 pendências originais (P1–P37)** preservadas no histórico; P33/P34/P36/P37 **parcialmente respondidas** pela estrutura; novas **P38–P40** para a regra de provisionamento.
4. **Matriz consolidada do contrato** (§13) — itens do domínio Despesas promovidos/sinalizados.
5. **Mapa de relacionamentos** (§12) — baseado em evidências legadas.
6. **Riscos de performance** (§14) — incluído o **risco de dupla contagem PAGAR × PAGARC**.

### O que permanece em aberto (não bloqueia a fundação estrutural):

1. **Regra de "Despesa Provisionada"** — semântica formal de `DATAPREV` (**E/P** — P38); regra de inclusão/exclusão de cancelamentos/estornos (**P** — P39); classificação fixa/variável e repasse médico (**P** — P35/P37/P40). Definida na Fase 3 (Core/Data, T-13), conforme PRD RN-37/RN-38/RN-39.
2. **Validação com o time/produto** — itens P que dependem de configuração, modelo ou decisão (P16, P24, P28, P29) e confirmação funcional de `PAGAR`/`PAGARC` como fonte do indicador no SisacHTML5 (P33/P34).
3. **Data de referência (competência × pagamento)** do indicador de Despesas — **P** (P36).

### Gate de negócio/dados:

- **Despesas:** origem estrutural **resolvida (D)**. Regra de negócio provisionada → **E/P**, decidida na Fase 3. **Não bloqueia a fundação estrutural.**
- **Retorno:** o vínculo Consulta→Retorno (regra de 30 dias) não tem representação física identificada — pode exigir cálculo derivado ou campo não mapeado.
- **Produção médica:** a unidade técnica de produção não foi definida — sem ela, não é possível implementar o indicador.

---

*Documento produzido por investigação documental (SghProg/CASAMATER) e complementado por validação somente-leitura no banco operacional acessível pelo Dashboard (2026-09-17). Nenhum dado, objeto, índice ou permissão do banco foi alterado.*
