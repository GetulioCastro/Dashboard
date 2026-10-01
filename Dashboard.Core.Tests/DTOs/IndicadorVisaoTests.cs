using Dashboard.Core.DTOs;

namespace Dashboard.Core.Tests.DTOs;

public sealed class IndicadorVisaoTests
{
    [Fact]
    public void visao_nova_aplica_defaults_tecnicos_do_contrato_especificados()
    {
        var visao = new IndicadorVisao();

        Assert.Equal(string.Empty, visao.Id);
        Assert.Equal(string.Empty, visao.Nome);
        Assert.Equal(string.Empty, visao.Unidade);
        Assert.False(visao.Monetario);
        Assert.Equal(0m, visao.ValorTotal);
        Assert.Equal(DimensaoVisao.Nenhuma, visao.DimensaoAtiva);
        Assert.Empty(visao.DimensoesDisponiveis);
        Assert.Empty(visao.Series);
    }

    [Fact]
    public void visao_nao_expoe_propriedades_descontinuadas_data_e_valor_atual()
    {
        var propriedades = typeof(IndicadorVisao).GetProperties().Select(p => p.Name).ToHashSet();

        Assert.DoesNotContain("Data", propriedades);
        Assert.DoesNotContain("ValorAtual", propriedades);
    }

    [Fact]
    public void visao_exposta_suporta_periodo_e_total_do_periodo_de_referencia()
    {
        var inicio = new DateOnly(2024, 1, 1);
        var fim = new DateOnly(2024, 12, 31);

        var visao = new IndicadorVisao
        {
            Periodo = new BusinessReferencePeriod(inicio, fim),
            ValorTotal = 82118m,
        };

        Assert.Equal(inicio, visao.Periodo.Start);
        Assert.Equal(fim, visao.Periodo.End);
        Assert.Equal(82118m, visao.ValorTotal);
    }

    [Fact]
    public void dimensao_visao_expoe_apenas_nenhuma_convenio_e_sexo_na_ordem_do_contrato()
    {
        var valores = Enum.GetValues<DimensaoVisao>();

        Assert.Equal(
            new[] { DimensaoVisao.Nenhuma, DimensaoVisao.Convenio, DimensaoVisao.Sexo },
            valores);
    }

    [Fact]
    public void grafico_forma_expoe_apenas_linha_coluna_e_donut_na_ordem_do_contrato()
    {
        var valores = Enum.GetValues<GraficoForma>();

        Assert.Equal(
            new[] { GraficoForma.Linha, GraficoForma.Coluna, GraficoForma.Donut },
            valores);
    }

    [Fact]
    public void serie_nova_aplica_defaults_tecnicos_do_contrato_especificados()
    {
        var serie = new SerieGrafico();

        Assert.Equal(string.Empty, serie.Nome);
        Assert.Equal(string.Empty, serie.Cor);
        Assert.Empty(serie.Pontos);
    }

    [Fact]
    public void ponto_novo_aplica_defaults_tecnicos_do_contrato_especificados()
    {
        var ponto = new PontoGrafico();

        Assert.Equal(default, ponto.Data);
        Assert.Equal(string.Empty, ponto.Rotulo);
        Assert.Equal(0m, ponto.Valor);
    }

    [Fact]
    public void periodo_de_referencia_de_negocio_armazena_inicio_e_fim_como_date_only()
    {
        var periodo = new BusinessReferencePeriod(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));

        Assert.Equal(new DateOnly(2024, 1, 1), periodo.Start);
        Assert.Equal(new DateOnly(2024, 12, 31), periodo.End);
    }

    [Fact]
    public void visao_do_atendimento_por_convenio_preenchida_segundo_o_contrato_de_visao()
    {
        var visao = new IndicadorVisao
        {
            Id = "atendimentos",
            Nome = "Atendimentos",
            Unidade = "atendimentos",
            Monetario = false,
            Periodo = new BusinessReferencePeriod(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31)),
            ValorTotal = 82118m,
            Forma = GraficoForma.Donut,
            DimensaoAtiva = DimensaoVisao.Convenio,
            DimensoesDisponiveis =
            [
                DimensaoVisao.Convenio,
                DimensaoVisao.Sexo,
            ],
            Series =
            [
                new SerieGrafico
                {
                    Nome = "Particular",
                    Cor = "#0d6efd",
                    Pontos = [new PontoGrafico { Rotulo = "Particular", Valor = 15420m }],
                },
                new SerieGrafico
                {
                    Nome = "Convenio",
                    Cor = "#198754",
                    Pontos = [new PontoGrafico { Rotulo = "Convenio", Valor = 60110m }],
                },
                new SerieGrafico
                {
                    Nome = "SUS",
                    Cor = "#dc3545",
                    Pontos = [new PontoGrafico { Rotulo = "SUS", Valor = 6588m }],
                },
            ],
        };

        Assert.Equal("atendimentos", visao.Id);
        Assert.Equal("Atendimentos", visao.Nome);
        Assert.False(visao.Monetario);
        Assert.Equal(GraficoForma.Donut, visao.Forma);
        Assert.Equal(DimensaoVisao.Convenio, visao.DimensaoAtiva);
        Assert.Equal(2, visao.DimensoesDisponiveis.Count);
        Assert.Equal(3, visao.Series.Count);
        Assert.Equal("Particular", visao.Series[0].Nome);
        Assert.Equal("Particular", visao.Series[0].Pontos[0].Rotulo);
        Assert.Equal(15420m, visao.Series[0].Pontos[0].Valor);
        Assert.Equal(82118m, visao.ValorTotal);
    }
}