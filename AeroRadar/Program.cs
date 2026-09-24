using AeroRadar.Configuration;
using AeroRadar.Services;
using Microsoft.Extensions.Options;
using AeroRadar.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<OpenSkyOptions>(builder.Configuration.GetSection(OpenSkyOptions.SectionName));

builder.Services.AddSingleton<GestorTokenOpenSky>();
builder.Services.AddTransient<OpenSkyAuthHandler>();

builder.Services.AddHttpClient<IOpenSkyClient, OpenSkyClient>((serviceProvider, client) =>
{
    var opciones = serviceProvider.GetRequiredService<IOptions<OpenSkyOptions>>().Value;
    client.BaseAddress = new Uri(opciones.UrlBase.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(20);
}).AddHttpMessageHandler<OpenSkyAuthHandler>();

builder.Services.AddSingleton<AlmacenAviones>();
builder.Services.AddHostedService<ConsultaAvionesService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ContadorConexiones>();

var app = builder.Build();

app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseStaticFiles();
}

app.UseAuthorization();
app.MapControllers();
app.MapHub<AvionesHub>("/hubs/aviones");

app.Run();
