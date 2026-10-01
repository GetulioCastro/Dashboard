using System.Globalization;
using Dashboard.Core.DTOs;

namespace Dashboard.Web.Dados;

public static class IndicadoresVisaoDemonstracao
{
    private const int Dias = 90;
    private const decimal BaseDia = 62m;
    private const int Seed = 11;
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.Today);

    private static readonly (string Nome, string Cor, decimal Fracao)[] CategoriasConvenio =
    [
        ("Particular", "#0d6efd", 0.1878m),
        ("Convenio", "#198754", 0.7320m),
        ("SUS", "#dc3545", 0.0802m),
    ];

    private static readonly (string Nome, string Cor, decimal Fracao)[] CategoriasSexo =
    [
        ("Masculino", "#6f42c1", 0.4500m),
        ("Feminino", "#fd7e14", 0.5500m),
    ];

    public static IndicadorVisao Obter(DimensaoVisao dimensao, GraficoForma forma)
    {
        if (dimensao == DimensaoVisao.Nenhuma && forma == GraficoForma.Donut)
        {
            forma = GraficoForma.Coluna;
        }

        var periodo = new BusinessReferencePeriod(Hoje.AddDays(-(Dias - 1)), Hoje);
        var diario = GerarSerieDiaria();

        var visao = new IndicadorVisao
        {
            Id = "atendimentos",
            Nome = "Atendimentos",
            Unidade = "atendimentos",
            Monetario = false,
            Periodo = periodo,
            ValorTotal = Math.Round(diario.Sum(p => p.Valor), 2),
            Forma = forma,
            DimensaoAtiva = dimensao,
            DimensoesDisponiveis = [DimensaoVisao.Convenio, DimensaoVisao.Sexo],
        };

        visao.Series = dimensao switch
        {
            DimensaoVisao.Convenio => SeriesPorCategoria(diario, CategoriasConvenio, forma),
            DimensaoVisao.Sexo => SeriesPorCategoria(diario, CategoriasSexo, forma),
            _ => [SerieTemporal("Atendimentos", "#0d6efd", diario)],
        };

        return visao;
    }

    private static List<PontoGrafico> GerarSerieDiaria()
    {
        var pontos = new List<PontoGrafico>(Dias);
        var random = new Random(Seed);

        for (var i = 0; i < Dias; i++)
        {
            var data = Hoje.AddDays(i - (Dias - 1));
            var fimDeSemana = data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

            var onda = (decimal)(Math.Sin(i / 9.0) * 0.18 + Math.Sin(i / 31.0) * 0.12);
            var ruido = (decimal)(random.NextDouble() * 0.5 - 0.25);
            var fator = 1m + onda + ruido;

            var valor = BaseDia * fator;
            if (valor < BaseDia * 0.35m)
            {
                valor = BaseDia * 0.35m;
            }

            if (fimDeSemana)
            {
                valor *= 0.55m;
            }

            valor = Math.Max(0m, Math.Round(valor));

            pontos.Add(new PontoGrafico
            {
                Data = data,
                Rotulo = data.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                Valor = valor,
            });
        }

        return pontos;
    }

    private static SerieGrafico SerieTemporal(string nome, string cor, IReadOnlyList<PontoGrafico> diario)
    {
        return new SerieGrafico
        {
            Nome = nome,
            Cor = cor,
            Pontos = [.. diario],
        };
    }

    private static List<SerieGrafico> SeriesPorCategoria(
        IReadOnlyList<PontoGrafico> diario,
        IReadOnlyList<(string Nome, string Cor, decimal Fracao)> categorias,
        GraficoForma forma)
    {
        var valoresPorData = new List<int[]>(diario.Count);

        for (var i = 0; i < diario.Count; i++)
        {
            var valoresProvisorios = new decimal[categorias.Count];
            for (var c = 0; c < categorias.Count; c++)
            {
                valoresProvisorios[c] = ValorCategoria(diario[i].Valor, categorias[c].Fracao, c, i);
            }

            valoresPorData.Add(ReconciliarCategorias(diario[i].Valor, valoresProvisorios, categorias));
        }

        var series = new List<SerieGrafico>(categorias.Count);
        for (var c = 0; c < categorias.Count; c++)
        {
            var (nome, cor, _) = categorias[c];

            if (forma == GraficoForma.Donut)
            {
                series.Add(new SerieGrafico
                {
                    Nome = nome,
                    Cor = cor,
                    Pontos =
                    [
                        new PontoGrafico
                        {
                            Data = Hoje,
                            Rotulo = nome,
                            Valor = valoresPorData.Sum(valores => valores[c]),
                        }
                    ],
                });
            }
            else
            {
                var pontos = new List<PontoGrafico>(diario.Count);
                for (var i = 0; i < diario.Count; i++)
                {
                    pontos.Add(new PontoGrafico
                    {
                        Data = diario[i].Data,
                        Rotulo = diario[i].Rotulo,
                        Valor = valoresPorData[i][c],
                    });
                }

                series.Add(new SerieGrafico { Nome = nome, Cor = cor, Pontos = pontos });
            }
        }

        return series;
    }

    private static int[] ReconciliarCategorias(
        decimal totalDia,
        IReadOnlyList<decimal> valoresProvisorios,
        IReadOnlyList<(string Nome, string Cor, decimal Fracao)> categorias)
    {
        var valores = new int[categorias.Count];
        if (totalDia == 0m)
        {
            return valores;
        }

        var quotas = ObterQuotas(totalDia, valoresProvisorios, categorias);
        var restos = new decimal[categorias.Count];
        for (var c = 0; c < categorias.Count; c++)
        {
            var piso = Math.Floor(quotas[c]);
            valores[c] = decimal.ToInt32(piso);
            restos[c] = quotas[c] - piso;
        }

        var unidadesRestantes = decimal.ToInt32(totalDia) - valores.Sum();
        for (var unidade = 0; unidade < unidadesRestantes; unidade++)
        {
            var indiceMaiorResto = 0;
            for (var c = 1; c < categorias.Count; c++)
            {
                if (restos[c] > restos[indiceMaiorResto])
                {
                    indiceMaiorResto = c;
                }
            }

            valores[indiceMaiorResto]++;
            restos[indiceMaiorResto] = -1m;
        }

        return valores;
    }

    private static decimal[] ObterQuotas(
        decimal totalDia,
        IReadOnlyList<decimal> valoresProvisorios,
        IReadOnlyList<(string Nome, string Cor, decimal Fracao)> categorias)
    {
        var quotas = new decimal[categorias.Count];
        if (totalDia == 0m)
        {
            return quotas;
        }

        var somaProvisoria = valoresProvisorios.Sum();
        for (var c = 0; c < categorias.Count; c++)
        {
            quotas[c] = somaProvisoria == 0m
                ? totalDia * categorias[c].Fracao
                : valoresProvisorios[c] * totalDia / somaProvisoria;
        }

        return quotas;
    }

    private static decimal ValorCategoria(decimal totalDia, decimal fracao, int categoria, int indice)
    {
        var desvio = (decimal)(Math.Sin(indice / 13.0 + categoria * 1.7));
        return Math.Max(0m, Math.Round(totalDia * fracao * (1m + 0.06m * desvio)));
    }
}