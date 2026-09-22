using System.Globalization;

namespace Dashboard.Web.Dados;

public sealed class PontoDemonstracao
{
    public string Data { get; set; } = string.Empty;

    public decimal Valor { get; set; }
}

public sealed class IndicadorDemonstracao
{
    public string Id { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string Unidade { get; set; } = string.Empty;

    public bool Monetario { get; set; }

    public string Forma { get; set; } = "coluna";

    public decimal ValorReferencia { get; set; }

    public string Referencia { get; set; } = string.Empty;

    public List<PontoDemonstracao> Pontos { get; set; } = [];
}

public static class IndicadoresDemonstracao
{
    private const int DiasSerie = 400;
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.Today);

    private static readonly (string Id, string Nome, string Unidade, bool Monetario, string Forma, int Seed, decimal Base)[] Especificacoes =
    [
        ("atendimentos", "Atendimentos", "atendimentos", false, "coluna", 11, 62m),
        ("consultas", "Consultas", "consultas", false, "linha", 23, 38m),
        ("exames", "Exames", "exames", false, "coluna", 37, 26m),
        ("faturamento", "Faturamento", "R$", true, "linha", 41, 12400m),
        ("producao-medica", "Produção Médica", "produções", false, "linha", 53, 44m),
        ("despesas", "Despesas", "R$", true, "coluna", 67, 4700m),
        ("repasses", "Repasses Médicos", "R$", true, "linha", 79, 2400m)
    ];

    public static List<IndicadorDemonstracao> Obter()
    {
        var indicadores = new List<IndicadorDemonstracao>(Especificacoes.Length);

        foreach (var spec in Especificacoes)
        {
            var pontos = GerarSerie(spec.Seed, spec.Base, spec.Monetario);

            indicadores.Add(new IndicadorDemonstracao
            {
                Id = spec.Id,
                Nome = spec.Nome,
                Unidade = spec.Unidade,
                Monetario = spec.Monetario,
                Forma = spec.Forma,
                ValorReferencia = SomarMesCorrente(pontos),
                Referencia = Hoje.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                Pontos = pontos
            });
        }

        return indicadores;
    }

    private static List<PontoDemonstracao> GerarSerie(int seed, decimal baseDia, bool monetario)
    {
        var pontos = new List<PontoDemonstracao>(DiasSerie);
        var random = new Random(seed);

        for (var i = 0; i < DiasSerie; i++)
        {
            var data = Hoje.AddDays(i - (DiasSerie - 1));
            var fimDeSemana = data.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

            var onda = (decimal)(Math.Sin(i / 9.0) * 0.18 + Math.Sin(i / 31.0) * 0.12);
            var ruido = (decimal)(random.NextDouble() * 0.5 - 0.25);
            var fator = 1m + onda + ruido;

            var valor = baseDia * fator;
            if (valor < baseDia * 0.35m)
            {
                valor = baseDia * 0.35m;
            }

            if (fimDeSemana)
            {
                valor *= 0.55m;
            }

            if (monetario)
            {
                valor = Math.Max(0m, Math.Round(valor / 5m) * 5m);
            }
            else
            {
                valor = Math.Max(0m, Math.Round(valor));
            }

            pontos.Add(new PontoDemonstracao
            {
                Data = data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Valor = valor
            });
        }

        return pontos;
    }

    private static decimal SomarMesCorrente(IReadOnlyList<PontoDemonstracao> pontos)
    {
        var inicio = new DateOnly(Hoje.Year, Hoje.Month, 1);
        var soma = 0m;

        foreach (var ponto in pontos)
        {
            if (DateOnly.ParseExact(ponto.Data, "yyyy-MM-dd", CultureInfo.InvariantCulture) >= inicio)
            {
                soma += ponto.Valor;
            }
        }

        return Math.Round(soma, 2);
    }
}