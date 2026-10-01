# SPEC-VIS-001 — Contrato de visão IndicadorVisao (Atendimentos)

> Contrato de visão · Leanwork SDD
> Origem: análise ATD-VIS-01 → decisão humana ATD-VIS-02 (2026-09-24) → correção documental ATD-VIS-03A (ValorAtual)
> Mapeia: `docs/architecture/adrs/ADR-010-adia-abstracao-de-provedor-de-visao-dos-indicadores.md`, `docs/prds/PRD-002-atendimentos-especificacao-funcional.md`
> Status: **aguardando validação humana**

## 1. Objetivo e escopo

Especificar, **sem implementar**, a evolução mínima do DTO `IndicadorVisao` (`Dashboard.Core`) para representar a visão do indicador **Atendimentos**, consumida pelo `Dashboard.Web`.

Fora de escopo desta especificação:

- Nenhuma alteração de código (`.cs`, `.cshtml`, JS, CSS), inclusive `IndicadorVisao.cs`, `dashboard.js`, `IndicadorDemonstracao`.
- Nenhum Provider/Service/Mapper/Factory/Strategy.
- Nenhum acesso a banco, execução de teste ou alteração de `PRD-002`, `ADR-010`, `PLAN-001`, `AGENTS.md`, `opencode.json` e T-08.

## 2. Contexto

O Dashboard possui dois níveis distintos de contrato:

| Camada | Contrato | Estado |
|---|---|---|
| **Evidência** (T-08) | `IIndicatorRepository → IndicatorData` (escalar) | Congelada (ADR-010), contrato de dados |
| **Visão** | `IndicadorVisao` em `Dashboard.Core` | Órfã (zero call-sites), reservada como "direção futura" (ADR-010) |

Decisão humana ATD-VIS-02: `IndicatorData` permanece como contrato de evidência; o `Dashboard.Web` deve consumir um contrato específico de visão; `IndicadorVisao` é o candidato a evoluir; **nenhuma** nova família de DTOs sem necessidade comprovada; **nenhum** Provider/Service/Mapper nesta etapa.

## 3. Especificação do contrato (forma final proposta)

### 3.1. Enum `DimensaoVisao` (novo)

```csharp
public enum DimensaoVisao
{
    Nenhuma,   // ausência de projeção dimensional — não é dimensão funcional
    Convenio,  // cobertura: Particular / Convênio / SUS (RN-26)
    Sexo
}
```

### 3.2. Classe `IndicadorVisao` evoluída

```csharp
public sealed class IndicadorVisao
{
    public string Id { get; set; }                    // NOVO — identificação no frontend
    public string Nome { get; set; }                  // mantido
    public string Unidade { get; set; }               // mantido
    public bool Monetario { get; set; }               // NOVO — formatação da apresentação
    public BusinessReferencePeriod Periodo { get; set; } // NOVO (substitui Data) — período de referência (RN-09)
    public decimal ValorTotal { get; set; }           // NOVO (substitui ValorAtual) — total do período, independente da dimensão ativa
    public GraficoForma Forma { get; set; }           // mantido — Linha | Coluna | Donut
    public DimensaoVisao DimensaoAtiva { get; set; }  // NOVO — dimensão projetada no gráfico
    public List<DimensaoVisao> DimensoesDisponiveis { get; set; } // NOVO — projeções analíticas suportadas
    public List<SerieGrafico> Series { get; set; }    // mantido
}
```

### 3.3. Tipos referenciais inalterados

- `GraficoForma` (Linha, Coluna, Donut) — mantido.
- `SerieGrafico` (`Nome`, `Cor`, `Pontos`) — mantido.
- `PontoGrafico` (`Data`, `Rotulo`, `Valor`) — mantido.
- `BusinessReferencePeriod` (`Start`, `End` — `DateOnly`) — reutilizado de `IndicatorData.cs` (Core).

### 3.4. Propriedade `Data` — remoção

A propriedade `Data` de `IndicadorVisao` **será removida** na implementação futura:

- `Periodo` representa o período de referência do indicador (RN-09);
- `PontoGrafico.Data` continua representando a data dos pontos da série;
- `IndicadorVisao` está órfão, sem call-sites;
- não existe consumidor atual que exija a propriedade `Data`.

### 3.5. Propriedade `ValorAtual` — remoção e substituição por `ValorTotal`

A propriedade `ValorAtual` de `IndicadorVisao` **será removida** na implementação futura:

- `ValorTotal` substitui semanticamente `ValorAtual`;
- `ValorTotal` representa explicitamente o total do período definido por `Periodo`;
- o contrato deixa de representar um valor pontual associado a `Data`;
- `Data` também será removida do nível de `IndicadorVisao` (§3.4);
- `PontoGrafico.Data` continua representando a data dos pontos das séries;
- a remoção é segura porque `IndicadorVisao` **não possui consumidores reais** (zero call-sites, verificado).

### 3.6. Variações do contrato (atual → proposta)

| Elemento | Estado atual | Estado após implementação |
|---|---|---|
| `Id` | inexistente | novo — `string` (inicialização técnica: string vazia) |
| `Nome` | `string` | **inalterado** |
| `Unidade` | `string` | **inalterado** |
| `Monetario` | inexistente | novo — `bool` (inicialização técnica: `false`) |
| `Data` | `DateOnly` | **removido** (§3.4) |
| `ValorAtual` | `decimal` | **removido** (§3.5), substituído por `ValorTotal` |
| `ValorTotal` | inexistente | **novo** — `decimal` (inicialização técnica: `0`) |
| `Periodo` | inexistente | novo — `BusinessReferencePeriod` (período de referência RN-09) |
| `Forma` | `GraficoForma` | **inalterado** (Linha | Coluna | Donut) |
| `DimensaoAtiva` | inexistente | novo — `DimensaoVisao` (inicialização técnica: `Nenhuma`) |
| `DimensoesDisponiveis` | inexistente | novo — `List<DimensaoVisao>` (inicialização técnica: lista vazia) |
| `Series` | `List<SerieGrafico>` | **inalterado** (inicialização técnica: lista vazia) |
| `DimensaoVisao` | inexistente | **novo** — enum `{ Nenhuma, Convenio, Sexo }` (§3.1) |

### 3.7. Defaults técnicos de inicialização

Valores-padrão de inicialização do DTO — **inicialização técnica, não regras de negócio**:

| Membro | Default de inicialização |
|---|---|
| `Id` | `string.Empty` |
| `Nome` | `string.Empty` |
| `Unidade` | `string.Empty` |
| `Monetario` | `false` |
| `ValorTotal` | `0` |
| `DimensaoAtiva` | `Nenhuma` |
| `DimensoesDisponiveis` | lista vazia |
| `Series` | lista vazia |

## 4. Semântica por visualização / dimensão

| Situação | `Forma` | `DimensaoAtiva` | `Series` |
|---|---|---|---|
| **Evolução temporal simples** (P1–P2) | Linha ou Coluna | `Nenhuma` | 1 série (`Nome` = "Atendimentos"), `Pontos` = `{Data, Valor}` por dia |
| **Distribuição por Convênio** (RN-26) — Donut | Donut | `Convenio` | 1 série por categoria (`Particular`, `Convênio`, `SUS`), cada uma com 1 ponto `{Rotulo, Valor}` (`Data` opcional) |
| **Convênio no tempo** | Coluna ou Linha | `Convenio` | 1 série por categoria, `Pontos` = `{Data, Valor}` sobre o tempo |
| **Distribuição por Sexo** | Donut ou Coluna | `Sexo` | 1 série por categoria (`Masculino`, `Feminino`), valor por categoria |
| **Sexo no tempo** | Coluna ou Linha | `Sexo` | 1 série por categoria, pontos por data |

Regras de interpretação do consumidor (apresentação):

- **Donut**: cada `SerieGrafico` = uma fatia; valor = único ponto categórico (ou soma dos pontos da série no período); `Cor` = cor da fatia; `Rotulo` do ponto = rótulo da fatia. **A interpretação de `SerieGrafico` como segmentos de Donut é responsabilidade da apresentação/renderização, não do domínio.**
- **Colunas/Linha com `Nenhuma`**: série única plotada por `Data`.
- **Colunas/Linha com dimensão**: uma barra/polilinha por série, agrupada por `Data`; legenda = `Nome` da série; `Cor` = cor da categoria.
- **Sem dados (CA-09)**: `Series` vazio ou todos os `Pontos` vazios → estado "Nenhum dado encontrado".
- **`ValorTotal`**: sempre o total do período, independentemente da dimensão ativa (as categorias podem não esgotá-lo — "Não classificado" — tratado na evidência, não no contrato).

## 5. Decisões humanas consolidadas

1. `ValorTotal` representa explicitamente o total do período, independentemente da dimensão ativa.
2. `DimensoesDisponiveis` representa as dimensões que o indicador suporta como projeção analítica; a presença da dimensão **não garante existência de dados** no período.
3. `DimensaoVisao.Nenhuma` representa ausência de projeção dimensional — **não é uma dimensão funcional**.
4. A propriedade `Data` de `IndicadorVisao` será removida na implementação futura (justificativa: §3.4).
5. A interpretação de `SerieGrafico` como segmentos de Donut permanece responsabilidade da apresentação/renderização, não do domínio.
6. `ValorAtual` será removido de `IndicadorVisao`; `ValorTotal` o substitui semanticamente e representa o total do período de referência (`Periodo`). Seguro porque `IndicadorVisao` não possui consumidores reais (justificativa: §3.5).

## 6. Mapeamento com a árvore desejada

```
Atendimentos
├── identificação      → Id, Nome, Unidade, Monetario
├── período            → Periodo {start, end}       (RN-09)
├── total do período   → ValorTotal                  (card)
├── evolução temporal  → Series[0].Pontos            (Data+Valor)
├── dimensões analíticas
│   ├── Convênio       → DimensaoAtiva/Disponiveis + série por categoria (RN-26)
│   └── Sexo           → idem
└── visualização       → Forma (Linha | Coluna | Donut)
```

## 7. Serialização

- JSON **camelCase** (padrão do `IndexModel`): enums `"linha"`, `"coluna"`, `"donut"` e `"nenhuma"`, `"convenio"`, `"sexo"`.
- `Periodo` serializa como `{ start: "yyyy-MM-dd", end: "yyyy-MM-dd" }` (`DateOnly`).
- A migração do JSON demo (`IndicadorDemonstracao`) para o JSON de visão, incluindo renderização Donut no JS, é **etapa de implementação futura** — fora desta especificação.

## 8. Fora de escopo / limitações

- Decisões de evidência/dados não resolvidas não são decididas por este contrato: P16 (SUS só quando serviço atende SUS), P19 (convênios suspensos `SUSPENSO`/`DATASUSP`), sexo "não informado" (PRD-002 §9), categoria "Não classificado".
- Ampliar dimensões além de `Convenio`/`Sexo` (Médico, Especialidade, Tipo, Local) fica como extensão futura do enum `DimensaoVisao`.
- Nenhuma regra funcional é inventada: o contrato apenas acomoda as categorias que vierem da evidência.

## 9. Rastreabilidade

| Artefato | Relação |
|---|---|
| `ADR-010` | Regra futura: definir o contrato definitivo na camada arquitetural adequada (Core, com base em `IndicadorVisao` evoluído), antes de abstrações/providers |
| `PRD-002` | RN-09 (período de referência), RN-26 (cobertura Particular/Convênio/SUS), CA-09 (sem dados) |
| `ATD-VIS-01` | Análise que fundamentou a escolha de `IndicadorVisao` como contrato de visão |
| `ATD-VIS-02` | Decisão humana consolidada (§5) |
| `ATD-VIS-03A` | Correção documental: remoção explícita de `ValorAtual` e substituição por `ValorTotal` (§3.5, §3.6) |
| `SPEC-UI-001` | Espécie-irmã (telas UI); este SPEC define o contrato de dados consumido por `dashboard.js` |