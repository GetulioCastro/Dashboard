# Dicionário de Dados — Conhecimento do Legado SISAC/CASAMATER e Domínios Conceituais do Dashboard

**Tarefa:**
- T-01 (PLAN-001-dashboard-indicadores.md) — documento da engenharia reversa do legado
- T-03 (PLAN-001-dashboard-indicadores.md) — domínios conceituais do novo SisacHTML5 (§14)

**Finalidade:** Documentar, por indicador, o conhecimento extraído do banco legado e — a partir da **gate de negócio (2026-09-16; ADR-008)** — registrar os **domínios conceituais** do novo SisacHTML5 (§14), que passam a ser a base do contrato de dados do Dashboard. A proveniência de cada afirmação é declarada, distinguindo **fato encontrado**, **regra confirmada**, **hipótese** e **questão em aberto**.

> ⚠️ **Reposicionamento (ADR-008 — 2026-09-16):** o schema legado descrito neste documento é **evidência histórica e fonte de conhecimento** para compreender conceitos de negócio — **não é contrato de dados** do Dashboard. O Dashboard consulta exclusivamente o **banco do novo SisacHTML5** (única fonte operacional). Este documento **não** deve ser usado para presumir nomes de tabelas/colunas no novo banco; os nomes físicos serão definidos pelo modelo do SisacHTML5 (T-03) e registrados na seção 14.

**Status:** Aguardando validação pelo time que opera o legado (gate humana obrigatória)
**Data:** 2026-09-16
**Origem das descobertas (legado):** banco CASAMATER na instância local `DESENVHMSISAC0215\MSSQLSERVER2022` (SQL Server 2022 Developer; servidor `DESENVHMSISAC02`). Consultas de catálogo (`sys.tables`, `sys.columns`, `sys.partitions`), definição da proc `SP_ATUALIZA_DASHBOARD` (`OBJECT_DEFINITION`) e amostragem de domínios (`GROUP BY`) — todas somente leitura.

---

## 1. Contexto do banco

### 1.1. Instâncias e bancos

| Instância / servidor | Banco | Conteúdo observado | Evidência |
|---|---|---|---|
| `(local)` — SQL Server 2019 | (apenas `master`, `model`, `msdb`, `tempdb`) | Sem bancos de aplicação | `sys.databases` |
| `.\MSSQLSERVER2022` (`DESENVHMSISAC0215\MSSQLSERVER2022`) | `CASAMATER` | **Banco operacional do legado** — tabelas de atendimento, fatura, glosa, leitos, convênios, BI | `sys.databases` |
| `.\MSSQLSERVER2022` | `SISACLAB` | Módulo de laboratório (tabelas `TAB_*` de exames) | `sys.databases` |
| `.\MSSQLSERVER2022` | `SISACIMAGEM` | Módulo de imagem | `sys.databases` |
| `.\MSSQLSERVER2022` | `CONFIG` | Configuração (`TAB_Parametro`) | `sys.databases` |

> **Hipótese:** `CASAMATER` é o banco do sistema VCL/Delphi de gestão de saúde (suíte "SISAC"). `SISACLAB` e `SISACIMAGEM` são módulos complementares (laboratório e diagnóstico por imagem) e **não** são fontes primárias dos seis indicadores do PRD.
> **Questão em aberto:** confirmar a identidade oficial do banco e se `CASAMATER` é desenvolvimento ou espelho de produção (não há string de conexão nem documentação no repositório — repo não contém código Delphi nem `.sql`).

### 1.2. Volume e cobertura temporal das tabelas-chave (números de catálogo `sys.partitions`)

| Tabela | Linhas (aprox.) | Cobertura de data observada | Observação |
|---|---|---|---|
| `ENTRADA` | 1.913.675 | `DataHoraEnt` 1899-12-30* até **2026-08-12** | Mestre operacional, **atual** |
| `FATURA` | 8.211.958 | — | Itemizado de cobrança por guia |
| `GLOSA` | 4.972.819 | — | Glosa por item |
| `CONSULTA` | 2.957.770 | — | Consultas médicas (evolução) |
| `RECEBER` | (a medir) | `DataRef` usado no dashboard legado | Contas a receber |
| `dbo.BI_Atendimento` | 1.088.101 | `DataEntrada`/`DataSaida` até **2016-10** | BI antigo, desatualizado |
| `BI.BI_Atendimento` | 648.705 | `DataAtendimento` **2018+** | BI novo |
| `dbo.BI_Faturamento` | 7.494.798 | — | BI de faturamento por item |
| `BI_PACIENTEDIA` | 576.565 | `Data` 2010-01-01 até **2024-10-22** | Base diária de ocupação |
| `CONTLEITO` | 69.581 | `Data` 2007-10-25 até **2026-04-10** | Controle diário de leitos |
| `CADMEDICO` | 3.802 | — | Cadastro de médicos |
| `CADCONVENIO` | 597 | — | Cadastro de convênios |
| `DASH_RESUMO_MENSAL` | 6 | — | Dashboard legado (não atualizado) |
| `DASH_RESUMO_CONVENIO` | 1 | — | Dashboard legado (não atualizado) |

\* Valor `1899-12-30` = "zero" do TDateTime (Delphi); na prática representa ausência de data.

---

## 2. Fonte de autoridade das definições do dashboard legado

**Fato:** Existe a procedure `dbo.SP_ATUALIZA_DASHBOARD` que define as fórmulas do dashboard que o próprio legado usava. Sua origem é verificável (`OBJECT_DEFINITION`). Ela alimentava `DASH_RESUMO_MENSAL` e `DASH_RESUMO_CONVENIO` (hoje praticamente vazias — 6 e 1 linhas).

Fórmulas extraídas (fato, transcrição da proc):

| Métrica legada | Fonte | Fórmula | Condição |
|---|---|---|---|
| Faturamento | `ENTRADA` | `SUM(Total)` por mês (`Ano`/`Mes` de `DataHoraEnt`) | `Fechado IN ('F','E')` |
| Produção | `ENTRADA` | `SUM(Total)` por mês | `Fechado <> 'C'` |
| Movimento de pacientes | `ENTRADA` | `COUNT(CodMovimento)` por mês | `Fechado <> 'C'` |
| Taxa de ocupação | `BI_PACIENTEDIA` | `(SUM(PacienteDia)*100/SUM(LeitoDia))` por mês | `Data` no período |
| Glosas (valor e %) | `RECEBER` | `SUM(Glosa)`; `% = SUM(Glosa)*100/SUM(Valor)` | `DataRef` no período |
| Convênios | `ENTRADA` + `CADCONVENIO` | `COUNT(E.CodConvenio)` agrupado por `Left(Lower(C.Descr),10)` | janela 120 dias, `Fechado <> 'C'` |
| Tempo médio de fechamento | `ENTRADA` | `AVG(DATEDIFF(dd, alta, DataFech))` | `Fechado IN ('F','E')` |

> **Regra confirmada (origem: proc legada):** "Faturamento" no legado = valor de movimentos com guia emitida (`Fechado IN ('F','E')`), coerente com a RN-12 do PRD (guia emitida, não recebimento). "Não cancelado" (`Fechado <> 'C'`) é o filtro usado para contagem/produção.
> **Hipótese:** `Fechado` usa codificação `F` = Faturado, `E` = Emitido, `A` = Aberto, `C` = Cancelado, `P` = Parcial. **Questão em aberto:** confirmar com o time.

Distribuição observada de `ENTRADA.Fechado`: `E`=1.602.045, `A`=181.246, `F`=45.873, `C`=42.624, `P`=40.883 (fonte: `GROUP BY`).

---

## 3. Indicador — Atendimentos (RN-07 a RN-09; Q1, Q2)

### 3.1. Tabela candidata canônica: `dbo.ENTRADA` (mestre de movimentos)

**Fato** (colunas via `sys.columns`):
- `CODMOVIMENTO varchar(15)` — identificador do atendimento/movimento
- `CODPACIENTE varchar(15)`, `MATRICULA varchar(70)`
- `DATAHORAENT datetime` — data/hora de entrada (usada como referência pela proc legada)
- `DATAHORASAI datetime`, `ALTA datetime` (alta hospitalar)
- `LOCAL varchar(20)` — unidade (ver §7), `ACOMOD varchar(50)`, `TAXAACOMOD`
- `CODCONVENIO varchar(10)`, `PLANO varchar(50)`
- `CODMEDICO varchar(5)` + colunas de equipe (`CODANEST`, `CODPRIM`, `CODSEG`, `CODINSTRUM`, `MEDRESP`)
- `TIPO varchar(10)` e `TIPOATEND char(2)` — **colunas candidatas a "tipo de atendimento" (Q2)**
- `TOTAL real` (valor), `GLOSA real`, `FATURA real`
- `FECHADO varchar(1)`, `NFECH varchar(10)`, `DATAFECH datetime`
- `TOTAL_TESTE_BI`, `DATAREF datetime`, `MES int`, `ANO int`
- `ELETIVA varchar(1)`, `OBITO varchar(5)`, `CIDOBITO varchar(10)`
- `GRUPOEMP char(2)`, `FILIAL char(2)`

**Fato** (domínios amostrados):
- `ENTRADA.TIPO`: códigos `1`(374K), `2`(148K), `3`(640K), `4`(604K), `5`(49K), `6`(78K), `7`(15K), `P`(3) — **códigos numéricos sem tabela de descrição no schema**
- `ENTRADA.TIPOATEND`: códigos `5`(575K), `4`(509K), `11`(305K), `2`(91K), `7`(79K), `1`(61K), `0`, `10`, `04`, `13`, outros — **códigos numéricos, sem descrição no schema**

### 3.2. Tabelas BI (já pré-agregadas pelo legado)

**Fato** — `BI.BI_Atendimento` (BI novo, 2018+) expõe os mesmos dados com colunas amigáveis:
- `DataAtendimento datetime`, `DataReferencia datetime`
- `TipoAtendimento varchar(50)` — **domínio em texto**: `Pequeno Procedimento`(237K), `Exame`(158K), `Consulta`(138K), `Retorno`(59K), `Cirurgia`(38K), `Clínico`(16K)
- `LocalAtendimento varchar(100)`, `TipoLocalAtendimento char(1)`, `OrigemAtendimento`
- `NomeConvenio`, `Plano`, `ModoCobranca`, `Situacao varchar(50)`, `ValorTotal numeric(18,2)`, `ValorCredenciado`, `ValorRecebido`
- `CodigoAtendimento varchar(15)`, `CodigoPaciente varchar(7)`
- `MedicoAssistente`, `MedicoSolicitante`, `Especialidade`
- UTI/internação: `DataAdmissaoUTI`, `DataAltaUTI`, `DataAltaMedica`, `DataAltaHospitalar`
- `DataInicioConsulta`, `DataFimConsulta`, `TempoConsulta int`

**Fato** — `dbo.BI_Atendimento` (BI antigo, parou em 2016-10): colunas `CodMovimento`, `Paciente`, `Telefone`, `DataEntrada`, `DataSaida`, `Situacao varchar(20)`, `Tipo varchar(20)`, `Glosa`, `ValorTotal`, `Convenio`, `Plano`, `ModoFatura`, `Local`, `MedicoExecutante`, `MedicoSolicitante`, `MedicoCirurgiao`, `Quantidade int`.
- `dbo.BI_Atendimento.Situacao`: `ENVIADO`(981K), `PARCIAL`(34K), `ABERTO`(31K), `CANCELADO`(26K), `FECHADO`(14K)
- `dbo.BI_Atendimento.Tipo`: `EXAME`(407K), `PQA`(319K), `CONSULTA`(206K), `RETORNO`(76K), `CIRÚRGICO`(31K), `CLÍNICO`(31K), `NULL`(15K)
- `dbo.BI_Atendimento.ModoFatura`: `CONVÊNIO`(620K), `PARTICULAR`(450K), `SUS`(17K)

### 3.3. Síntese / decisões para o indicador

| Tipo | Conteúdo |
|---|---|
| **Regra confirmada** | Trigês fontes distintas (ENTRADA, dbo.BI_Atendimento, BI.BI_Atendimento) carregam a contagem de movimentos por período com filtro por unidade (`LOCAL`/`LocalAtendimento`) e por convênio. |
| **Hipótese T-01** | Para **Atendimentos**, usar `ENTRADA` (mestre, atual, com `DATAHORAENT` = data de referência conforme RN-09) é o caminho principal; as tabelas `BI_*` servem como fonte alternativa/facilitadora, mas têm defasagem (dbo parou 2016; BI novo cobre 2018+) e podem não cobrir a totalidade dos movimentos. |
| **Questão em aberto (Q2)** | Não há descrição no schema para os códigos de `ENTRADA.TIPO` (1–7) e `ENTRADA.TIPOATEND` (0–13). O `BI.BI_Atendimento.TipoAtendimento` em texto (Consulta/Exame/Pequeno Procedimento/Retorno/Cirurgia/Clínico) ajuda, mas a **definição oficial** de "tipo de atendimento" precisa ser confirmada com o time (T-02). |

---

## 4. Indicador — Faturamento (RN-10 a RN-13; Q1, Q6)

### 4.1. Fontes

**Fato** — `dbo.FATURA` (8.2M linhas) é o registro itemizado por guia (uma linha por procedimento/item do movimento):
- `CODPACIENTE varchar(15)`, `CODTAXA varchar(10)`, `CHAVE varchar(80)`, `DESCR varchar(200)`
- `DATA datetime` — **data do lançamento/guia** (candidata a "data de emissão da guia" — RN-13)
- `VALOR numeric(18,2)`, `QUANT numeric(18,2)`, `VTOTAL numeric(18,2)`, `VPROF`, `VHOSP`, `VANEST`, `SADT`, `DIARIA`, `TAXA`, `GAS`
- `CODMEDICO varchar(10)`, `CODBH`, `CODTUS`, `LOCAL varchar(20)`, `GLOSA numeric(18,0)`, `CUSTO real`
- `GRUPOIMP varchar(20)`, `MODOFAT`…, `GRUPOEMP`, `FILIAL`

**Fato** — `dbo.ENTRADA.Total` é a fonte da proc legada para "Faturamento" (`Fechado IN ('F','E')`); `DATAHORAENT` é a data de referência usada.

**Fato** — `dbo.RECEBER` é o contas a receber (pagamentos), **não** é faturamento por guia:
- `NFECH varchar(15)`, `NPARC int`, `CODCONVENIO varchar(3)`, `DATAFECH`, `DATAVENC`, `VALOR real`, `SALDO`, `VALORPAG`, `DATAPAG`, `GLOSA real`, `RECURSO`, `DATARECURSO`, `FATURADO real`, `PERDA`, `DATAREF` (referência usada pela proc legada em glosas)

**Fato** — `dbo.BI_Faturamento` (7.5M linhas) é a visão BI itemizada com colunas amigáveis: `CodMovimento`, `Procedimento`, `Valor`, `ValorAcrescimo`, `ValorTotal numeric(18,2)`, `Glosa numeric(18,2)`, `DataFatura datetime`, `DataReferencia`, `DataPrevisao`, `DataEntrega`, `LocalFatura`, `MedicoFatura`, `Fechamento`, `DataFechamento`.

### 4.2. Síntese / decisões para o indicador

| Tipo | Conteúdo |
|---|---|
| **Regra confirmada** (origem medata do legado) | Faturamento calculado com base na **guia emitida** (movimento `Fechado IN ('F','E')`), corroborando RN-12 (guia, não recebimento). |
| **Hipótese T-01** | Fonte canônica: `ENTRADA` (Total, por período via `DATAHORAENT`) ou, para detalhe, `FATURA`/`BI_Faturamento` (por item com `ValorTotal` e `DataFatura`). `RECEBER` deve ser usado apenas se a definição do negócio evoluir para "recebido" (contraria RN-12). |
| **Questão em aberto** | Confirmar o conceito exato de "data de emissão da guia" (coluna de `FATURA.DATA` vs `ENTRADA.DATAHORAENT`) e se o filtro por convênio deve usar `CODCONVENIO` (código) ou o nome (T-02). |

---

## 5. Indicador — Produtividade Médica (RN-14 a RN-16; Q3)

### 5.1. Fontes

**Fato** — `dbo.CADMEDICO` (3.802 médicos):
- `CODMEDICO varchar(10)`, `NOME varchar(50)`, `CRM`, `CPF`, `ESPECIALIDADE varchar(100)` (texto)
- `ATIV varchar(20)` — **em sua maioria vazio/NULL** (`''`=2.860, `NULL`=927, códigos 46/26/14/97/11 = 15 no total)
- `CONDICAO varchar(1)` — `''`=3.539, `D`=184, `C`=79
- `TIPO varchar(10)`, `DATANASC`, `CBO`, `CNES`, `CNS`, `EMERG`, `GRUPOEMP`, `FILIAL`

**Fato** — ligação médico ↔ especialidade:
- `dbo.ESPECIALIDADE`: `Codigo varchar(6)`, `DESCR varchar(50)`, `CBO varchar(10)`
- `dbo.CADMEDICOESP`: `SEQ int`, `CODMEDICO varchar(6)`, `AMBESP varchar(10)`, `GRUPOEMP`, `FILIAL`

**Fato** — `ENTRADA` tem a equipe do movimento: `CODMEDICO`, `CODANEST` (anestesista), `CODPRIM` (auxiliar?), `CODSEG`, `CODINSTRUM`, `MEDRESP varchar(20)`, `CODESPEC varchar(20)`.

**Fato** — `CADPROF` (profissionais gerais) está **vazia** (0 linhas).

### 5.2. Síntese / decisões para o indicador

| Tipo | Conteúdo |
|---|---|
| **Fato** | O cadastro de profissionais existe (`CADMEDICO`, 3.802 registros); não há tabela populada de profissionais não-médicos. |
| **Fato** | Não há flag confiável de "ativo": `CADMEDICO.ATIV` e `CONDICAO` estão majoritariamente vazias no ambiente observado. |
| **Hipótese T-01** | "Profissional ativo no período" provavelmente será derivado de movimentos (`ENTRADA.*MEDICO`) ou de cadastros não suspensos/desligados — **decidir não é possível só com o schema**. |
| **Questão em aberto (Q3)** | Definir "profissional ativo" com o Analista de Management (T-02). Confirmar se a especialidade filtrada é `CADMEDICO.ESPECIALIDADE` (texto) ou via `CADMEDICOESP`/`ESPECIALIDADE`. |

---

## 6. Indicador — Ocupação (RN-17 a RN-19; Q4)

### 6.1. Fontes

**Fato** — `dbo.BI_PACIENTEDIA` (576.565 linhas, `Data` 2010-01-01…2024-10-22) — linha **por paciente/dia** de internação:
- `Data datetime`, `CodMovimento varchar(100)`, `CodPaciente`, `Paciente`
- `Setor`, `Acomodacao`, `DataEntradaAcomod`, `DataSaidaAcomod`
- `Pacientedia int`, `Leitodia int`, `TaxaOcupacao real` (pré-calculada), `LeitoLivre int`
- `Local varchar(100)`, `LOCAL2 varchar(10)`, `TipoLeito varchar(5)`
- `Convenio`, `Plano`, `ModoFatura`, `MedicoAssistente`
- `GRUPOEMP`, `FILIAL`, `ID int`

**Fato** — `dbo.CONTLEITO` (69.581 linhas, `Data` 2007-10-25…2026-04-10) — censo diário de leitos por local:
- `Data datetime`, `LOCAL varchar(10)` (códigos ex.: `0100`, `011514`, `1110595`)
- `NumLeito int`, `NumBloq int`, `NumVazio int`, `PercOcup real`, `NUMOCUP int`, `NUMOCUPE int`, `CHAVE`

**Fato** — `dbo.TAXAOCUP` — taxas por `MESANO varchar(10)`:
- `MESANO`, `VALOR real`, `LOCAL varchar(10)`, `TIPO varchar(2)`, `NL int`, `CODCONVENIO`, `DESCR`, `DATAAUX`

**Regra confirmada** (proc legada): Taxa de ocupação = `SUM(PacienteDia)*100/SUM(LeitoDia)` por mês, com base em `BI_PACIENTEDIA` (base **diária**).

### 6.2. Síntese / decisões para o indicador

| Tipo | Conteúdo |
|---|---|
| **Fato** | A fonte tem granularidade diária (`BI_PACIENTEDIA` por `Data`/movimento; `CONTLEITO` censo por dia/local; `TAXAOCUP` mensal). |
| **Hipótese T-01** | O cálculo "média do período" é nativamente compatível (`SUM(PacienteDia)/SUM(LeitoDia)`), coerente com RN-19. Médias diárias (`CONTLEITO.PercOcup`) também existem. |
| **Atenção** | `BI_PACIENTEDIA` para em 2024-10-22; `CONTLEITO` chega a 2026-04-10. Verificar qual está em uso (T-03 / time do legado). |
| **Questão em aberto (Q4)** | Confirmar com o Gestor Clínico: média diária no período vs ponto no tempo; e se "leitos disponíveis" = `LeitoDia`/`NumLeito` (capacidade física) ou contratual. |

---

## 7. Indicador — Glosas (RN-20 a RN-22; Q5)

### 7.1. Fontes

**Fato** — `dbo.GLOSA` (4.972.819 linhas) — glosa por item/guia:
- `DATA datetime` — **data da glosa** (candidata a "data de notificação" — RN-22)
- `VALOR real`, `VALORPAG real`, `GLOSADO real`, `DATAPAG datetime`
- `CODCONVENIO varchar(3)`, `CODPACIENTE varchar(15)`, `CODMOVIMENTO varchar(15)`
- `CODMEDICO varchar(5)`, `CODTAXA varchar(10)`, `DESCR varchar(500)`, `LOCAL varchar(10)`
- `TIPOGLOSA varchar(5)` — **códigos numéricos** (maioria `NULL`/vazio; principais: `1714`(691K), `1705`(437K), `1426`(85K), `1706`(19K), `2001`, `2015`, …)
- `TIPO varchar(5)` — tipo de item: `MED`(51K), `MM`(30K), `MAT`(27K), `MSF`(22K), `F`(30K), `TT`, `OPM`, `TX`, `GAS`, …
- `CONDICAO varchar(1)`: `A`(4.177K), `B`(640K), `R`(154K) — **significado a validar**
- `RECURSO real`, `DATARECURSO datetime`, `Acatado bit`, `DataAcate datetime`, `MotivoAcate varchar(2000)`, `JUSTIFICA`, `MOTIVO varchar(150)`
- `BAIXA`, `DATABAIXA`, `GRUPOEMP`, `FILIAL`, `IDGLOSA int`, `SEQ`

**Fato** — a tabela `dbo.TissMotivoGlosa` (candidata a catálogo de motivos de glosa) **está vazia** no ambiente observado. O significado dos códigos de `TIPOGLOSA` **não está no banco** (vive no cliente Delphi ou em tabela de outra aplicação).

**Regra confirmada** (proc legada): O dashboard legado calculava glosas a partir de **`RECEBER.Glosa`** (agregado sobre contas a receber, por `DataRef`), **não** da tabela `GLOSA`. `% = SUM(Glosa)*100/SUM(Valor)`.

### 7.2. Síntese / decisões para o indicador

| Tipo | Conteúdo |
|---|---|
| **Fato** | Duas fontes concebívels: `GLOSA` (detalhe por item, com `DATA`, `VALOR`, convênio, motivo) e `RECEBER.Glosa` (agregado por conta/recebível, alinhado ao dashboard legado). |
| **Hipótese T-01** | Para o PRD (RN-20 colunas: data de notificação, valor, unidade, convênio) a tabela `GLOSA` é a fonte rica; `RECEBER.Glosa` é a fonte do indicador legado. **Escolha a validar com o time.** |
| **Questão em aberto (Q5)** | Não existe coluna "técnica/financeira". `TIPOGLOSA` são códigos (ex.: 1714, 1705, 2001) cuja descrição não está no banco (`TissMotivoGlosa` vazia). É preciso confirmar com o time se o legado distingue glosa técnica × financeira (provavelmente via códigos de motivo específicos) e qual conjunto usar. |

---

## 8. Indicador — Convênios (RN-23 a RN-25; Q6)

### 8.1. Fontes

**Fato** — `dbo.CADCONVENIO` (597 convênios):
- `CODCONVENIO varchar(10)`, `DESCR varchar(250)`, `RAZAO varchar(100)`, `CGC`, `CNES varchar(10)`, `ANS varchar(50)`, `URL`/`WS*` (webservices TISS), `DATAVIGOR`, `SUSPENSO`, `DATASUSP`, `MODOFAT`, `CONEMERG`
- `GRUPOEMP`, `FILIAL`

**Fato** — vínculo movimento ↔ convênio:
- `ENTRADA.CODCONVENIO varchar(10)` (referencia `CADCONVENIO`; 315 códigos distintos em uso)
- `FATURA.CODCONVENIO varchar(3)`; `BI.BI_Atendimento.NomeConvenio`/`Convenio` (texto); `dbo.BI_Atendimento.Convenio` (texto)

**Regra confirmada** (proc legada): O legado agrupava quantidade de movimentos **por convênio** (volume), usando `Left(Lower(Descr),10)` de `CADCONVENIO`, janela de 120 dias, `Fechado <> 'C'`.

### 8.2. Síntese / decisões para o indicador

| Tipo | Conteúdo |
|---|---|
| **Regra confirmada** | O legado usava **volume de atendimentos por convênio** no seu dashboard. |
| **Questão em aberto (Q6)** | O PRD permite volume, faturamento ou ambos (RN-25). A definição final com o Diretor Operacional é T-02; há dados para os três (volume via `ENTRADA`; faturamento via `FATURA`/`ENTRADA.Total`). |

---

## 9. Unidades de atendimento e empresa (filtros RN-08/11/15/18/21/24)

**Fato** — `dbo.LOCAL` é a árvore de unidades ("locais"):
- `LOCAL varchar(20)` (código), `DESCR varchar(100)`, `TIPO varchar(1)` (ex.: `G`=grupo, `T`=...), `LOCALPAI varchar(15)` (hierarquia), `COD_UNID char(3)`, `CNPJ`, `FINALID`, `PORTE`
- Exemplos observados: `0100` = ALIANÇA CASAMATER; `011` = HOSPITAL ALIANÇA CASAMATER; `01001` = [EM ESPERA]; níveis administrativos (financeiro, recepção, etc.).
- `ENTRADA.LOCAL`, `FATURA.LOCAL`, `CONTLEITO.LOCAL`, `GLOSA.LOCAL` referenciam esta árvore (tipos/códigos coincidem com exemplos).

**Fato** — `dbo.TAB_LOCAL` (LOC_ID, LOC_DESCR, LOC_TIPO, LOC_GRUPO, LOC_CODIGO) está **praticamente vazia** (3 registros com código NULL) — não é a tabela operativa de unidades.

**Fato** — Toda a suíte é **multifilial**: `GRUPOEMP char(2)` + `FILIAL char(2)` em todas as tabelas de dados; `CONFIGEMPRESA` é o mestre de empresas (fonte do cursor da proc legada).

> **Hipótese T-01:** O seletor "unidade" do dashboard deve usar a árvore `dbo.LOCAL` (filtrando por código em `LOCAL`). **Questão em aberto:** confirmar se o filtro é por nó da árvore (setor/unidade de atendimento) e como tratar grupos pai.

---

## 10. Considerations para a camada Data (recortes de leitura — ADR-003)

Registrados aqui como **fatos técnicos** para T-03 (performance) e Fase 3 (queries):

- **Datas Delphi-zero:** valores `1899-12-30` representam data não preenchida; queries de período devem filtrar essas linhas.
- **Tipos numéricos desiguais:** valores monetários em `real`/`float` na `ENTRADA`, `FATURA`, `GLOSA`, `RECEBER`; `numeric(18,2)` nas tabelas `BI_*`. Arredondamentos e formatação (RN-05) devem tratar isso na camada de leitura.
- **Volumes altos:** `FATURA` (~8,2M), `GLOSA` (~5M), `CONSULTA` (~3M), `ENTRADA` (~1,9M) — exigem índices adequados e testes de performance (T-03).
- **Duas famílias BI:** `dbo.BI_*` (até ~2016) e `BI.*` (2018+) — sempre qualificar o schema na query (`dbo.` vs `BI.`).

---

## 11. Questões em aberto (após T-01) — para a gate humana

Confirmações necessárias com o **time que opera o legado** antes de T-02/T-03 e de qualquer query (Fase 3):

| # | Questão | Evidência atual | O que falta |
|---|---|---|---|
| Q1 | Quais tabelas são a fonte canônica de cada indicador? | Candidatos mapeados (§3–§8), com fateos de volume/atualização | Validar a escolha de fonte por indicador (ENTRADA vs BI_*) |
| Q2 | Definição de "tipo de atendimento" | `ENTRADA.TIPO` (1–7) e `TIPOATEND` (0–13) sem descrição; `BI.BI_Atendimento.TipoAtendimento` em texto | Significado oficial dos códigos; qual coluna usar |
| Q3 | Definição de "profissional ativo" | `CADMEDICO.ATIV`/`CONDICAO` majoritariamente vazias | Definição do negócio (Analista de Management) |
| Q4 | Ocupação: média diária vs ponto no tempo | Fontes diárias (`BI_PACIENTEDIA`, `CONTLEITO`); proc legada usa média mensal | Decisão do Gestor Clínico |
| Q5 | Glosa técnica × financeira | Não há coluna dedicada; `TIPOGLOSA` são códigos sem catálogo no banco (`TissMotivoGlosa` vazia); legado usava `RECEBER.Glosa` | Se o legado distingue e como |
| Q6 | Convênios: volume, faturamento ou ambos | Legado usava volume | Decisão do Diretor Operacional |
| Q7 | Capacidade do SQL Server | Volumes medidos (§1.2) | Benchmark/validação com infra (T-03) |
| — | Qual banco/instância usar como oficial e se `CASAMATER` é dev ou espelho de produção | Instâncias listadas (§1.1) | Confirmação com infra |
| — | Estado de alimentação de `BI_PACIENTEDIA` (parou 2024-10-22) e se é mantido | Cobertura medida (§1.2) | Confirmação com infra |

---

## 12. Referências de origem (como reproduzir cada descoberta)

Todas as afirmações acima são reproduzíveis via consultas **somente leitura**:

- Lista de bancos: `SELECT name FROM sys.databases` (`sys.databases`)
- Lista de tabelas/colunas: `sys.tables JOIN sys.columns JOIN sys.types` (consultas de catálogo)
- Contagens/ordens de grandeza: `sys.partitions` (linhas aproximadas) ou `GROUP BY` em colunas de domínio
- Fórmulas do dashboard legado: `SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.SP_ATUALIZA_DASHBOARD'))`
- Volume da proc: `SP_ATUALIZA_DASHBOARD` itera `CONFIGEMPRESA` (por `GRUPOEMP`/`FILIAL`) e alimenta `DASH_RESUMO_MENSAL` e `DASH_RESUMO_CONVENIO`

> Consultas estruturadas de T-03 usarão estas mesmas origens, ampliadas com medições de execução em horário de baixa atividade.

---

## 13. T-02 — Definições de negócio (evidências levantadas; aguarda gate humana)

**Objetivo:** responder as questões do §12 usando a lógica interna das procedures de carga do legado (origem: `OBJECT_DEFINITION` de `SP_ATUALIZA_DASHBOARD`, `SP_TabIndicador`, `SP_PacienteDia`, `SP_TAXAOCUP`), diferenciando **Fato**, **Regra confirmada**, **Hipótese** e **Dependente do usuário**.

Classificação usada neste anexo: `F` = fato encontrado; `R` = regra confirmada (lógica explícita em proc/ETL do legado); `H` = hipótese; `U` = depende de validação humana (stakeholder/time do legado).

### 13.1. Códigos `Fechado` (estado da guia/movimento) — **R** (origem: `SP_PacienteDia`, CASE explícito)

| Código | Significado | Evidência |
|---|---|---|
| `A` | ABERTO | `CASE E.Fechado WHEN 'A' THEN 'ABERTO'` |
| `P` | PARCIAL | `WHEN 'P' THEN 'PARCIAL'` |
| `F` | FECHADO | `WHEN 'F' THEN 'FECHADO'` |
| `E` | ENVIADA (guia enviada) | `WHEN 'E' THEN 'ENVIADA'` |
| `C` | CANCELADO | `WHEN 'C' THEN 'CANCELADO'` |

Distribuição atual em `ENTRADA`: `E`=1.602.045, `A`=181.246, `F`=45.873, `C`=42.624, `P`=40.883 (Fato).

**Revisão de hipótese da T-01:** o `E` era lido como "Emitido"; a proc legada define **ENVIADA**. Consequência para RN-12: "faturamento = Fechado IN ('F','E')" significa **guias fechadas e enviadas** — o PRD fala em "guia emitida". Confirmação de equivalência com o responsável pelo PRD → **U**.

### 13.2. Tipo de atendimento (Q2) — **R** parcial (origem: `SP_PacienteDia`)

Mapeamento explícito no CASE da proc `ENTRADA.Tipo`:

| Código | Significado |
|---|---|
| `1` | CONSULTA |
| `2` | RETORNO |
| `3` | EXAME |
| `4` | PEQUENO ATENDIMENTO (PQA) |
| `5` | CLINICO |
| `6` | CIRURGIA |

- `ENTRADA.TIPO` também aparece como `'7'` (15.788), `'P'` (3), `'U'` (1) — **não mapeados** na proc → **U** (significado).
- `ENTRADA.TIPOATEND` (códigos 0–13) **não possui** mapeamento encontrado em procs → **U** (se a fonte de tipos do novo dashboard é `TIPO` ou `TIPOATEND`).
- **Fonte canônica de contagem (regra legada):** `ENTRADA` com `Fechado <> 'C' AND LoteEnt <> 'INAT'` (`SP_TabIndicador`, modo `MOVPACIENTE`). O filtro **`LoteEnt <> 'INAT'`** (lote não-INAT) é novo e relevante (RN-07).
- Corrobora: `BI.BI_Atendimento.TipoAtendimento` (texto) usa as mesmas 6 classes (Consulta, Retorno, Exame, Clínico, Cirurgia, Pequeno Procedimento) — **H** de que deriva de `ENTRADA.Tipo`.

### 13.3. Faturamento — fonte, referência e categorias

- **R**: Faturamento por guia = `SUM(Valor)` sobre `ENTRADA` (`Fechado IN ('F','E')`), referência `DataHoraEnt` (`SP_TabIndicador` modos `FATURAMM`, `FATURATX`, `FATURAHM`, `OPME`).
- **R**: Por convênio, havia também (do recebível) `FATURADOCONV` = `SUM(Valor)` de `RECEBER` onde `Condicao IN ('A','B')`, por `DataFech`; `RECEBIDOCONV` = `Condicao = 'B'`; `RECEBERCONV` = `Condicao = 'A'` por `DataVenc`. Decorre a semântica de `RECEBER.Condicao`: **A = a receber (em aberto), B = recebido (baixado)** — **R**.
- **R (categorias de item em `FATURA.Grupo`)**: `1` = honorários médicos (proc `FATURAHM`); `LEFT(Grupo,1) IN ('2','3','4')` = taxas (proc `FATURATX`); `Grupo IN ('5','6','7','M')` = materiais (proc `FATURAMM`); `Grupo = 'M'` = OPME (proc `OPME`). Item faturado = `(Valor + Filme + CustoOP) * Quant` — **R**.
- **H**: "data de emissão da guia" (RN-13) == `ENTRADA.DataHoraEnt` (assim o legado referenciava). Candidatos alternativos: `ENTRADA.DATAFECH`/`NFECH`, `FATURA.DATA` → **U**.

### 13.4. Produtividade médica (Q3)

- **F**: O legado contava por `E.CodMedico` (modos `MOVCONSULTA`/`MOVEXAME`) e por `C.CodMedico` (TEMPOMEDIOCON, tabela `CONSULTA` com `Tipo='CON'`), **sem** filtro de status do profissional — não existia "profissional ativo" formalizado no legado.
- **F**: `CADMEDICO.ATIV`/`CONDICAO` majoritariamente vazias; `CADPROF` vazia.
- **H**: "ativo no período" = profissional com ≥1 movimento no período (derivável de `ENTRADA.CODMEDICO`), complementado por cadastro não-desligado.
- **U**: definição oficial de "profissional ativo" (Analista de Management); qual papel conta (executante `CODMEDICO` vs `MEDRESP`/anestesista); fonte de especialidade (`CADMEDICO.ESPECIALIDADE` texto vs `CADMEDICOESP`+`ESPECIALIDADE`).

### 13.5. Ocupação (Q4)

- **R (dashboard legado)**: `TAXAOCUP = SUM(PacienteDia)*100.0/SUM(LeitoDia)` por mês sobre `BI_PacienteDia` (`SP_TabIndicador`/`SP_ATUALIZA_DASHBOARD`).
- **R (construção de `BI_PacienteDia` — `SP_PacienteDia`)**: censo diário composto por uma linha de leito **ocupado** (`Modo='S'`, `Pacientedia=1`, `Leitodia=1`, `LeitoLivre=0`) e uma de leito **livre** (`Modo='N'`, `Pacientedia=0`, `Leitodia=1`, `LeitoLivre=1`) por dia. Exclui `ACOMOD` com `Tipo NOT IN ('A','E','U','F','V')` e `Condicao IN ('[Bloqueado]','C')`. `ACOMOD.CodPaciente` = `ENTRADA.CodMovimento`.
- **R/F**: capacidade = **física** (leitos cadastrados; `LeitoDia` = ocupados + livres, com bloqueados/cancelados excluídos). Sem evidência de capacidade contratual.
- **R/F**: `TipoLeito='U'` = UTI (modo `PACDIA` filtra `TipoLeito='U'`). Filtro por tipo de leito disponível em `BI_PACIENTEDIA.TipoLeito`.
- **F**: método alternativo existe (`SP_TAXAOCUP` por `CONTACOMOD`, horas ponderadas, inclui ambulatório `LOCAL.TIPO IN ('B','P','H','C','D','I')`, grava em `TAXAOCUPLOC`) — é método de custo, **não** o do dashboard.
- **U**: confirmação (Gestor Clínico) de que a regra é o método legado (média do período agregada) — compatível com RN-19; e a **fonte ativa** de leitos (`BI_PacienteDia` parou em 2024-10-22 vs `CONTLEITO` até 2026-04) a validar com infra.

### 13.6. Glosas (Q5)

- **R (dashboard legado)**: índice de glosa = `SUM(Glosa)*100/SUM(Valor)` de `RECEBER` com `Valor > 0`, por `DataFech`; valor de glosas = `SUM(Glosa)` — **nível de conta/recebível**, não item.
- **F**: `TissMotivoGlosa` vazia (0 linhas) — catálogo de motivos TISS não está populado.
- **F**: `TabGlosa` (223 linhas) contém catálogo de **motivos de glosa** (códigos 001–042, descrições textuais) vinculado a convênio (ex.: `GGG`). Não confere com `TIPOGLOSA` (4 dígitos, ex.: 1714) → **U** se há relação entre catálogo, `TIPOGLOSA` e `GLOSA.MOTIVO`.
- **F/H**: `GLOSA.CONDICAO` (`A`/`B`/`R`) **sem** mapeamento encontrado (atenção: não confundir com `RECEBER.Condicao`, que tem outra semântica) → **U**. `GLOSA.TIPO` (MED/MM/MAT/MSF/F/TT/OPM/TX/GAS/UCO) = categorias de item; coerente com `FATURA.Grupo`, porém correspondência exata é **H**.
- **U**: distinção técnica × financeira não existe no legado (Q5) — validar com o time; **fonte** do indicador (tabela `GLOSA` por item com `DATA` de notificação vs `RECEBER.Glosa` por conta) a definir.

### 13.7. Convênios (Q6)

- **R (legado)**: indicador de convênios = **volume** (`COUNT`) por convênio (`SP_ATUALIZA_DASHBOARD`); existia também faturamento por convênio (`FATURADOCONV`). Agrupamento do legado usava `Left(Lower(Descr),10)` — **F** (artefato do legado; novo dashboard deve agrupar por `CODCONVENIO`).
- **U**: métrica oficial (volume/faturamento/ambos — Q6, Diretor Operacional).

### 13.8. Unidades (mapeamento)

- **F/R**: `ENTRADA.Local` = código (varchar 20) e junta-se por `L.Local = E.Local` exato. Em `BI_PacienteDia`, `Local` (varchar 100) = **nome** e `Local2` = **código** — para filtrar BI, usar `Local2`.
- **F**: `LOCAL.TIPO` tem papel de agrupador: grupo (`G`), setores administrativos/de custo e tipos `B/P/H/C/D/I` (ambulatório/consultório/etc. p/ ocupação-custo). Nível selecionável de "unidade" (não administrativo) → **U**.
- **F**: `ACOMOD.Condicao` com `[Bloqueado]` é usado para excluir leitos — sinal que a cadeia leito reserva o código de unidade como **`ACOMOD.LocalE`** → `LOCAL.Local`.
- **F**: códigos de unidade em formatos distintos entre tabelas (`ENTRADA.LOCAL` varchar 20; `GLOSA.LOCAL`/`CONTLEITO.LOCAL` varchar 10; `BI.Local2`) — consolidação única na camada Data (T-07) → **U** para a regra de qual nível expor.

### 13.9. Empresa / filial

- **R (legado)**: toda a carga filtra `GrupoEmp = '01' AND Filial = '01'` (ambiente atual); parâmetros globais usam `Filial IN ('01','99')`; `CADMEDICO` é buscado com `Filial IN ('99', ...)`. Nome da empresa vem de `SGHINI` (`Filial + ' - ' + Empresa`) e `CONFIGEMPRESA` (fonte do cursor de `SP_ATUALIZA_DASHBOARD`).
- **U**: se o novo dashboard expõe seletor de empresa/filial ou fixa a empresa atual — decisão de escopo/UI.

### 13.10. Fontes operacionais adicionais (Fato)

| Tabela | Contexto |
|---|---|
| `EXAME` | Itens de exames/consultas; `Tipo='C'` (consulta) / `'E'` (exame); join `X.CodPaciente = E.CodMovimento`; `GRUPOPROC` para grupo do exame (`MOVCONSULTA`/`MOVEXAME`) |
| `CONSULTA` | Lançamentos de consulta com `Tipo='CON'` (tempo médio de consulta) |
| `AGENDA` | Agendamentos (`MOVAGENDA`, `MOVAGENDACONF` com `ConfAtd='S'`) — fora dos 6 indicadores do PRD |
| `LOTEENT` | `ENTRADA.LoteEnt <> 'INAT'` filtra lotes inativos na contagem de movimentos |
| `PROCEDUTI`/`MONITORAMENTO`/`PROCEDCC` | Indicadores de UTI/infecção (TAXAITU, TAXACVC, TAXAICS, CVC/VM) — fora do escopo PRD; utilizam parâmetros de `SGHPARAM` |

### 13.11. Síntese — o que muda em relação à T-01

1. **`Fechado` mapeado** (Aberto/Parcial/Fechado/Enviada/Cancelado) — revisa intepretação anterior do `E`.
2. **`ENTRADA.Tipo` mapeado** (1=Consulta, 2=Retorno, 3=Exame, 4=PQA, 5=Clínico, 6=Cirurgia); `7`, `P`, `U` e `TIPOATEND` seguem em aberto.
3. **Ocupação formalizada** como agregação sobre censo diário (`PacienteDia`/`LeitoDia`), capacidade física, `TipoLeito='U'` = UTI.
4. **Glosas do dashboard legado** = nível de recebível (`RECEBER`), não item; catálogo de motivos em `TabGlosa`, não em `TissMotivoGlosa`.
5. **Convênios**: agrupar por `CODCONVENIO`; métrica a confirmar.
6. **Faturamento por convênio** também existia no legado (`FATURADOCONV`) — suporte a "ambos" em Q6.

### 13.12. Perguntas que permanecem dependentes de stakeholders (para aprovação da gate)

- [ ] **Time do legado:** significado de `TIPO='7'`, `'P'`, `'U'` e de `TIPOATEND`; se a fonte de tipos do novo dashboard é `TIPO` ou `TIPOATEND` (Q2).
- [ ] **Time do legado:** relação entre `TIPOGLOSA` (4 díg.), `TabGlosa` (motivos 3 díg.) e `GLOSA.MOTIVO`; semântica de `GLOSA.CONDICAO` (A/B/R); se existe distinção técnica × financeira (Q5).
- [ ] **Time do legado/responsável PRD:** "guia emitida" (RN-12) equivale a `Fechado IN ('F','E')` (Fechada + Enviada)? (13.1)
- [ ] **Analista de Management:** definição de "profissional ativo"; papel a contar; fonte de especialidade (Q3).
- [ ] **Gestor Clínico:** método de ocupação (agregado do período) e capacidade (física) (Q4); fonte ativa de leitos.
- [ ] **Diretor Operacional:** métrica de Convênios — volume, faturamento ou ambos (Q6).
- [ ] **Responsável PRD/UI:** exposição de empresa/filial; nível de unidade selecionável.

---

## 14. Domínios conceituais do Dashboard (novo SisacHTML5 — T-03)

> **Finalidade:** registrar os **domínios conceituais** que o Dashboard precisa consumir do **banco do novo SisacHTML5** (única fonte operacional — ADR-008). Esta seção **não** inventa nomes físicos de tabelas/colunas: o contrato de dados (entidades, atributos, estados, relacionamentos) será definido e validado com o produto na tarefa **T-03** (PLAN-001). O conhecimento descrito nas seções 1–13 é **evidência histórica**, não contrato.
>
> Legenda de origem: **C** = conceito definido na gate de negócio / PRD-001 v1.0 · **R** = regra a formalizar · **U** = questão em aberto.

### 14.1. Atendimento

- **C:** registra a movimentação assistencial (atendimentos realizados). Indicador: **Atendimentos** (RN-07..RN-09).
- **C:** campos mínimos conceituais: data de referência (**data do atendimento** — RN-09), tipo de atendimento, estado, categoria de cobertura (RN-26) e convênio/plano (quando aplicável), profissional(ais) envolvido(s).
- **R/U:** classificação do tipo de atendimento no novo modelo (categorias e valores) — modelagem definida pelo SisacHTML5 (RN-30); não se presume hierarquia com Consulta/Exame.

### 14.2. Consulta

- **C:** registra consultas realizadas. Indicador: **Consultas** (RN-28).
- **C:** distinção **primeira consulta / retorno** conforme definição do produto (RN-28).
- **R/U:** relação com Atendimento definida pelo novo modelo (RN-30) — entidade própria ou categoria.

### 14.3. Exame

- **C:** registra exames realizados. Indicador: **Exames** (RN-29).
- **C:** tipo/grupo de exame quando aplicável (RN-29).
- **R/U:** relação com Atendimento definida pelo novo modelo (RN-30).

### 14.4. Cobertura (categoria de atendimento)

- **C:** **Particular**, **Convênio** (efetivamente cadastrado no SisacHTML5) e **SUS** (somente quando o serviço atender SUS) (RN-26).
- **U:** como o novo modelo identifica a cobertura SUS no cadastro (RN-26, Q10, T-02/T-03).

### 14.5. Guia / Conta e Faturamento

- **C:** base do indicador **Faturamento** (RN-10..RN-13). Dados conceituais mínimos: estado da conta, data de emissão, convênio, valor(es) e itens (valor, quantidade, categoria), unidade.
- **C:** distinção entre **contas faturadas** e **contas a faturar** (RN-31); comparação **período anterior × atual** (RN-32).
- **R/U:** estados efetivos no novo banco, definição de "emissão" e fórmula de "a faturar" (RN-33, Q4 — T-02). Base = guia/conta de atendimento, **não** recebimento (RN-12).

### 14.6. Produção Médica / Profissional

- **C:** base do indicador **Produção Médica** (RN-14..RN-16, RN-34..RN-36).
- **C:** produção por **profissional ativo**; análise individual/profissional e por categoria de serviço, ambas por cobertura (RN-26, RN-35, RN-36).
- **R/U:** definição exata de produtividade, "profissional ativo", papel a contar, métrica e fonte de especialidade (RN-16/RN-34, Q3 — T-02). Especialidade é informação futura de formalização (RN-15).

### 14.7. Despesas

- **C:** base do indicador **Despesas** — grupos **fixas** e **variáveis** (RN-37, RN-38) comparáveis graficamente (RN-39).
- **R/U:** quais despesas em cada grupo, categorias, **data de referência** (competência × pagamento) e origem no novo banco (Q5 — T-02). Nada derivado do legado como regra.

### 14.8. Repasses (roadmap)

- **C:** indicador do **escopo conceitual/roadmap — bloqueado para implementação no MVP** (RN-40). Regra específica por serviço de saúde, a definir em Core quando solicitado. Não se assume relação com Faturamento, percentuais ou honorários.

### 14.9. Período de referência dos indicadores

- **C:** mecanismo uniforme de filtro por período — **atual**, **passado** e **futuro**, este e aquele com data inicial e final (RN-04, RN-42, CA-15).
- **C:** cada indicador possui sua **data de referência de negócio própria** em Core (RN-42): Atendimentos/Consultas/Exames = data do evento; Faturamento = data de emissão da conta; Produção e Despesas = data definida pelas regras formalizadas (T-02).
- **C:** histórico de **2025 e anteriores** disponível no banco novo para **homologação/demonstração** (RN-44).

### 14.10. Relações e independência financeira

- **C:** relação Atendimento × Consulta × Exame é definida pelo novo modelo (RN-30) — não presumida.
- **C:** **Faturamento, Despesas e Repasses são independentes** — nenhum Resultado/Margem derivado automático (RN-41).

### 14.11. Observação sobre nomes físicos

> Os nomes físicos de tabelas/colunas/estados do novo banco serão preenchidos nesta seção **somente após a validação do contrato de dados em T-03**, junto ao produto. Até lá, mantêm-se apenas os domínios conceituais acima, para evitar a criação de um novo "contrato inventado" (mesma regra aplicada ao schema legado pela ADR-008).