using AeroRadar.Configuration;
using AeroRadar.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<OpenSkyOptions>(builder.Configuration.GetSection(OpenSkyOptions.SectionName));

builder.Services.AddHttpClient<IOpenSkyClient, OpenSkyClient>((serviceProvider, client) =>
{
    var opciones = serviceProvider.GetRequiredService<IOptions<OpenSkyOptions>>().Value;
    client.BaseAddress = new Uri(opciones.UrlBase.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(20);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
