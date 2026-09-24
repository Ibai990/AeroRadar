using AeroRadar.Configuration;
using Microsoft.Extensions.Options;
using AeroRadar.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AeroRadar.Services;

public class ConsultaAvionesService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AlmacenAviones _almacen;
    private readonly OpenSkyOptions _opciones;
    private readonly ILogger<ConsultaAvionesService> _logger;
    private readonly IHubContext<AvionesHub, IAvionesCliente> _hubContext;

    public ConsultaAvionesService(
    IServiceScopeFactory scopeFactory,
    AlmacenAviones almacen,
    IHubContext<AvionesHub, IAvionesCliente> hubContext,
    IOptions<OpenSkyOptions> opciones,
    ILogger<ConsultaAvionesService> logger)
    {
        _scopeFactory = scopeFactory;
        _almacen = almacen;
        _hubContext = hubContext;
        _opciones = opciones.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromSeconds(_opciones.IntervaloConsultaSegundos);
        using var temporizador = new PeriodicTimer(intervalo);

        do
        {
            await ConsultarAsync(stoppingToken);
        }
        while (await temporizador.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ConsultarAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var cliente = scope.ServiceProvider.GetRequiredService<IOpenSkyClient>();

            var aviones = await cliente.ObtenerAvionesAsync(stoppingToken);
            _almacen.Actualizar(aviones);

            await _hubContext.Clients.All.RecibirAviones(_almacen.Actual);

            _logger.LogInformation("Consulta completada: {Total} aviones", aviones.Count);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // La aplicación se está cerrando: no es un error.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar OpenSky");
        }
    }
}