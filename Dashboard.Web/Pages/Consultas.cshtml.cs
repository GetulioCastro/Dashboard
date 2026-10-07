using System.Text.Json;
using System.Text.Json.Serialization;
using Dashboard.Core.Contratos;
using Dashboard.Core.DTOs;
using Dashboard.Core.Regras;
using Dashboard.Data.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dashboard.Web.Pages;

public class ConsultasModel : PageModel
{
    private const int DiasPadraoRetroativos = 89;

    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private readonly ConsultasVisaoRepository _visaoRepository;
    private readonly ConsultasConvenioNominalRepository _convenioNominalRepository;
    private readonly ILogger<ConsultasModel> _logger;

    public ConsultasModel(
        ConsultasVisaoRepository visaoRepository,
        ConsultasConvenioNominalRepository convenioNominalRepository,
        ILogger<ConsultasModel> logger)
    {
        _visaoRepository = visaoRepository;
        _convenioNominalRepository = convenioNominalRepository;
        _logger = logger;
    }

    public IndicadorVisao? Visao { get; private set; }

    public BusinessReferencePeriod? Periodo { get; private set; }

    public string DadosJson { get; private set; } = "null";

    public bool DadosIndisponiveis { get; private set; }

    public string MensagemIndisponibilidade { get; private set; } = string.Empty;

    public IndicadorConvenioNominalVisao? ConvenioNominal { get; private set; }

    public bool ConvenioNominalIndisponivel { get; private set; }

    public string MensagemConvenioNominal { get; private set; } = string.Empty;

    public async Task OnGetAsync(
        string? dimensao = null,
        string? forma = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        DateOnly? dataInicial = null,
        DateOnly? dataFinal = null,
        CancellationToken cancellationToken = default)
    {
        inicio ??= dataInicial;
        fim ??= dataFinal;

        IndicatorFilter filtro = MontarFiltro(inicio, fim);
        Periodo = PeriodoResolutor.Resolver(filtro);

        try
        {
            Visao = await _visaoRepository.ObterVisaoAsync(
                filtro,
                ParseDimensao(dimensao),
                ParseForma(forma),
                cancellationToken);

            DadosJson = JsonSerializer.Serialize(Visao, OpcoesJson);
        }
        catch (Exception erro) when (erro is not OperationCanceledException)
        {
            _logger.LogError(
                erro,
                "Falha ao consultar a visao do indicador de Consultas. Dimensao={Dimensao} Forma={Forma} Inicio={Inicio} Fim={Fim}",
                dimensao,
                forma,
                filtro.StartDate,
                filtro.EndDate);

            Visao = null;
            DadosJson = "null";
            DadosIndisponiveis = true;
            MensagemIndisponibilidade = $"{erro.GetType().Name}: {erro.Message}";
        }

        if (Visao is null)
        {
            return;
        }

        try
        {
            ConvenioNominal = await _convenioNominalRepository.ObterConvenioNominalAsync(filtro, cancellationToken);
        }
        catch (Exception erro) when (erro is not OperationCanceledException)
        {
            _logger.LogError(
                erro,
                "Falha ao consultar a analise nominal por convenio de Consultas. Inicio={Inicio} Fim={Fim}",
                filtro.StartDate,
                filtro.EndDate);

            ConvenioNominal = null;
            ConvenioNominalIndisponivel = true;
            MensagemConvenioNominal = $"{erro.GetType().Name}: {erro.Message}";
        }
    }

    private static IndicatorFilter MontarFiltro(DateOnly? inicio, DateOnly? fim)
    {
        DateOnly hoje = DateOnly.FromDateTime(DateTime.Today);
        bool periodoInformado = inicio.HasValue && fim.HasValue && inicio.Value <= fim.Value;

        return new IndicatorFilter
        {
            Period = PeriodType.Past,
            StartDate = periodoInformado ? inicio!.Value : hoje.AddDays(-DiasPadraoRetroativos),
            EndDate = periodoInformado ? fim!.Value : hoje,
        };
    }

    private static DimensaoVisao ParseDimensao(string? dimensao) => dimensao switch
    {
        "convenio" => DimensaoVisao.Convenio,
        _ => DimensaoVisao.Convenio,
    };

    private static GraficoForma ParseForma(string? forma) => forma switch
    {
        "linha" => GraficoForma.Linha,
        "donut" => GraficoForma.Donut,
        _ => GraficoForma.Coluna,
    };
}