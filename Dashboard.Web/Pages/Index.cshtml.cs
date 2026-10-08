using System.Text.Json;
using Dashboard.Core.Contratos;
using Dashboard.Core.DTOs;
using Dashboard.Data.Repositories;
using Dashboard.Web.Dados;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dashboard.Web.Pages;

public class IndexModel : PageModel
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web);

    private readonly IIndicadorVisaoRepository _atendimentosVisaoRepository;
    private readonly ConsultasVisaoRepository _consultasVisaoRepository;
    private readonly ExamesVisaoRepository _examesVisaoRepository;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IIndicadorVisaoRepository atendimentosVisaoRepository,
        ConsultasVisaoRepository consultasVisaoRepository,
        ExamesVisaoRepository examesVisaoRepository,
        ILogger<IndexModel> logger)
    {
        _atendimentosVisaoRepository = atendimentosVisaoRepository;
        _consultasVisaoRepository = consultasVisaoRepository;
        _examesVisaoRepository = examesVisaoRepository;
        _logger = logger;
    }

    public List<IndicadorDemonstracao> Indicadores { get; private set; } = [];

    public string DadosJson { get; private set; } = "[]";

    public void OnGet()
    {
        Indicadores = IndicadoresDemonstracao.Obter();

        DadosJson = JsonSerializer.Serialize(Indicadores, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public async Task<IActionResult> OnGetIndicadoresReaisAsync(
        DateOnly? dataInicial,
        DateOnly? dataFinal,
        CancellationToken cancellationToken = default)
    {
        bool periodoValido = dataInicial.HasValue
            && dataFinal.HasValue
            && dataInicial.Value <= dataFinal.Value;

        if (!periodoValido)
        {
            return RespostaIndicadoresReais(null, null, [
                IndicadorIndisponivel("atendimentos", "Período inválido."),
                IndicadorIndisponivel("consultas", "Período inválido."),
                IndicadorIndisponivel("exames", "Período inválido."),
            ]);
        }

        IndicatorFilter filtro = new()
        {
            Period = PeriodType.Past,
            StartDate = dataInicial!.Value,
            EndDate = dataFinal!.Value,
        };

        Task<IndicadorRealResposta> tarefaAtendimentos =
            ConsultarIndicadorAsync("atendimentos", _atendimentosVisaoRepository, filtro, cancellationToken);
        Task<IndicadorRealResposta> tarefaConsultas =
            ConsultarIndicadorAsync("consultas", _consultasVisaoRepository, filtro, cancellationToken);
        Task<IndicadorRealResposta> tarefaExames =
            ConsultarIndicadorAsync("exames", _examesVisaoRepository, filtro, cancellationToken);

        await Task.WhenAll(tarefaAtendimentos, tarefaConsultas, tarefaExames);

        return RespostaIndicadoresReais(
            dataInicial.Value,
            dataFinal.Value,
            [tarefaAtendimentos.Result, tarefaConsultas.Result, tarefaExames.Result]);
    }

    private async Task<IndicadorRealResposta> ConsultarIndicadorAsync(
        string id,
        IIndicadorVisaoRepository repositorio,
        IndicatorFilter filtro,
        CancellationToken cancellationToken)
    {
        try
        {
            IndicadorVisao visao = await repositorio.ObterVisaoAsync(
                filtro,
                DimensaoVisao.Nenhuma,
                GraficoForma.Coluna,
                cancellationToken);

            return ConstruirRespostaReal(id, visao);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception erro)
        {
            _logger.LogError(
                erro,
                "Falha ao carregar o indicador real {Indicador} na Home. Inicio={Inicio} Fim={Fim}",
                id,
                filtro.StartDate,
                filtro.EndDate);

            return new IndicadorRealResposta(id, false, null, string.Empty, "Indicador indisponível no momento.");
        }
    }

    private static IndicadorRealResposta IndicadorIndisponivel(string id, string mensagem) =>
        new(id, false, null, string.Empty, mensagem);

    private static IndicadorRealResposta ConstruirRespostaReal(string id, IndicadorVisao visao)
    {
        SerieGrafico? serie = visao.Series.FirstOrDefault(s => s.Pontos is { Count: > 0 });

        if (serie is null)
        {
            return new IndicadorRealResposta(id, true, visao.ValorTotal, visao.Unidade, string.Empty);
        }

        List<PontoRealResposta> pontos =
        [.. serie.Pontos.Select(ponto => new PontoRealResposta(ponto.Data, ponto.Valor))];

        return new IndicadorRealResposta(
            id,
            true,
            visao.ValorTotal,
            visao.Unidade,
            string.Empty,
            serie.Cor,
            pontos);
    }

    private static ContentResult RespostaIndicadoresReais(
        DateOnly? dataInicial,
        DateOnly? dataFinal,
        List<IndicadorRealResposta> indicadores)
    {
        string json = JsonSerializer.Serialize(
            new RespostaIndicadoresReaisPayload(dataInicial, dataFinal, indicadores),
            OpcoesJson);

        return new ContentResult
        {
            Content = json,
            ContentType = "application/json",
            StatusCode = 200,
        };
    }

    private sealed record IndicadorRealResposta(
        string Id,
        bool Disponivel,
        decimal? Valor,
        string Unidade,
        string Mensagem,
        string? Cor = null,
        List<PontoRealResposta>? Pontos = null);

    private sealed record PontoRealResposta(DateOnly Data, decimal Valor);

    private sealed record RespostaIndicadoresReaisPayload(
        DateOnly? DataInicial,
        DateOnly? DataFinal,
        List<IndicadorRealResposta> Indicadores);
}
