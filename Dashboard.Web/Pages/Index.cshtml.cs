using System.Text.Json;
using Dashboard.Web.Dados;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dashboard.Web.Pages;

public class IndexModel : PageModel
{
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
}