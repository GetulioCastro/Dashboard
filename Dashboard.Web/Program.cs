using Dashboard.Core.Contratos;
using Dashboard.Data.Connections;
using Dashboard.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<ISqlConnectionFactory>(
    new SqlConnectionFactory(builder.Configuration.GetConnectionString("SisacDatabase")));

builder.Services.AddScoped<IIndicadorVisaoRepository, AtendimentosVisaoRepository>();

builder.Services.AddScoped<IIndicadorConvenioNominalRepository, AtendimentosConvenioNominalRepository>();
builder.Services.AddScoped<AtendimentosConvenioNominalRepository>();

builder.Services.AddScoped<ConsultasVisaoRepository>();
builder.Services.AddScoped<IIndicadorConvenioNominalRepository, ConsultasConvenioNominalRepository>();
builder.Services.AddScoped<ConsultasConvenioNominalRepository>();

builder.Services.AddScoped<ExamesVisaoRepository>();
builder.Services.AddScoped<IIndicadorConvenioNominalRepository, ExamesConvenioNominalRepository>();
builder.Services.AddScoped<ExamesConvenioNominalRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
